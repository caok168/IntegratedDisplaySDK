using CitFileSDK;
using CitIndexFileSDK;
using CitIndexFileSDK.MileageFix;
using CommonFileSDK;
using IntegratedDisplayCommon.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MileageFix
{
    public class CitToIdf
    {

        IOperator _op = null;

        CITFileProcess citHelper = new CITFileProcess();

        //里程快速修正可执行模块 读
        public string ReadCit(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citfile = Convert.ToString(obj["citFile"]);
                int jumpvalue = Convert.ToInt32(obj["jumpValue"].ToString());

                List<AutoIndex> clslist = new List<AutoIndex>();
                clslist = _readCit(citfile, jumpvalue);
                string data = JsonConvert.SerializeObject(clslist);
                resultInfo.flag = 1;
                resultInfo.msg = "";
                resultInfo.data = data;
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(resultInfo);
        }

        //快速修正 写
        [DispId(81)]
        public string WriteIdf(string json)
        {
            JObject obj = (JObject)JsonConvert.DeserializeObject(json);
            string citfile = Convert.ToString(obj["citFile"]);
            int jumpvalue = Convert.ToInt32(obj["jumpValue"].ToString());
            string innerdbpath = Convert.ToString(obj["innerdbpath"]);
            return _writeIdf(citfile, jumpvalue, innerdbpath);
        }

        /// <summary>
        /// 读取CIT文件
        /// </summary>
        /// <param name="citFilePath">cit文件路径----->来自里程快速校正中的选择cit文件的按钮</param
        /// <param name="numericUpDown1">跳变允许值----->来自里程快速校正中的  跳变容许值</param>
        private List<AutoIndex> _readCit(String citFilePath, int numericUpDown1)
        {
            List<AutoIndex> autoIndexClsList = new List<AutoIndex>();
            //autoIndexClsList.Max(p => p.meter_between);
            //autoIndexClsList.Min(p => p.meter_between);
            //double  count = citHelper.GetTotalSampleCount(citFilePath)/4/1000;
            //double tiaobian1=autoIndexClsList.Sum(p => p.meter_between)/count;
            //double tiaobian1i = autoIndexClsList.Sum(p => Math.Abs(p.meter_between))/autoIndexClsList.Count;
            if (numericUpDown1 <= 0)
            {
                ///MessageBox.Show("容许跳变值为 0");
                return autoIndexClsList;
            }

            try
            {

                FileInformation fileInfomation = citHelper.GetFileInformation(citFilePath);
                List<ChannelDefinition> channelList = citHelper.GetChannelDefinitionList(citFilePath);

                FileStream fs = new FileStream(citFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                BinaryReader br = new BinaryReader(fs, Encoding.Default);
                br.BaseStream.Position = 0;

                br.ReadBytes(120);


                br.ReadBytes(65 * fileInfomation.iChannelNumber);
                br.ReadBytes(BitConverter.ToInt32(br.ReadBytes(4), 0));
                int iChannelNumberSize = fileInfomation.iChannelNumber * 2;
                byte[] b = new byte[iChannelNumberSize];

                long milePos = 0;
                int km_pre = 0;
                int meter_pre = 0;
                int km_currrent = 0;
                int meter_current = 0;
                int meter_between = 0;
                int km_index = 0;
                int meter_index = 2;

                long iArray = (br.BaseStream.Length - br.BaseStream.Position) / iChannelNumberSize;

                for (int i = 0; i < iArray; i++)
                {
                    milePos = br.BaseStream.Position;

                    b = br.ReadBytes(iChannelNumberSize);

                    if (Encryption.IsEncryption(fileInfomation.sDataVersion))
                    {
                        b = Encryption.Translate(b);
                    }

                    if (i == 0)
                    {
                        km_pre = (int)(BitConverter.ToInt16(b, km_index));
                        meter_pre = (int)(BitConverter.ToInt16(b, meter_index));
                    }
                    else
                    {
                        km_currrent = (int)(BitConverter.ToInt16(b, km_index));
                        meter_current = (int)(BitConverter.ToInt16(b, meter_index));
                        //第二个通道为采样点，换算为米就要除以4
                        meter_between = (km_currrent - km_pre) * 1000 + (meter_current - meter_pre) / 4;

                        if (Math.Abs(meter_between) > numericUpDown1)
                        {
                            AutoIndex autoIndexCls = new AutoIndex();
                            autoIndexCls.milePos = milePos;
                            autoIndexCls.km_current = km_currrent;
                            autoIndexCls.meter_current = meter_current;
                            autoIndexCls.km_pre = km_pre;
                            autoIndexCls.meter_pre = meter_pre;
                            autoIndexCls.meter_between = meter_between;

                            autoIndexClsList.Add(autoIndexCls);
                        }

                        km_pre = km_currrent;
                        meter_pre = meter_current;

                    }

                }

                br.Close();
                fs.Close();
            }
            catch (Exception ex)
            {

                throw new Exception("读取CIT文件跳变点失败:" + ex.Message + ",堆栈：" + ex.StackTrace);

            }
            return autoIndexClsList;
        }

        /// <summary>
        /// 向idf文件中写入
        /// </summary>
        /// <param name="citFilePath">cit文件路径----->来自里程快速校正中的选择cit文件的按钮</param
        /// <param name="numericUpDown1">跳变允许值----->来自里程快速校正中的  跳变容许值</param>
        /// <param name="innerdbpath">内部数据库-----></param>
        private string _writeIdf(String citFilePath, int numericUpDown1, string innerdbpath)
        {
            //String idfFileName = Path.GetFileNameWithoutExtension(citFilePath) + ".idf";

            //String idfFilePath = Path.Combine(Path.GetDirectoryName(citFilePath), idfFileName);

            //if (!File.Exists(idfFilePath))
            //{
            //    //MessageBox.Show("找不到波形索引文件！");
            //    Console.WriteLine("找不到波形索引文件！");
            //    return;
            //}

            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            //读取cit文件
            List<AutoIndex> autoIndexClsList = _readCit(citFilePath, numericUpDown1);

            String idfFileName = Path.GetFileNameWithoutExtension(citFilePath) + "_MileageFix" + ".idf";

            String idfFilePath = Path.Combine(Path.GetDirectoryName(citFilePath), idfFileName);

            //设置附带数据库路径和链接字符串，流程修正使用
            InnerFileOperator.InnerFilePath = innerdbpath;
            InnerFileOperator.InnerConnString = "Provider = Microsoft.Jet.OLEDB.4.0; Data Source = {0}; Persist Security Info = True; Mode = Share Exclusive; Jet OLEDB:Database Password = iicdc; ";
            //InnerFileOperator.InnerConnString = "provider=Microsoft.Ace.OLEDB.12.0;extended properties=excel 12.0; Data Source = {0}; Persist Security Info = True; Mode = Share Exclusive; Extended Properties=Excel 12.0:Database Password = iicdc; ";

            try
            {
                _op = new IndexOperator();
                _op.IndexFilePath = idfFilePath;
                CITFileProcess cit = new CITFileProcess();
                FileInformation fileforma = cit.GetFileInformation(citFilePath);
                UserFixedTable fixedTable = new UserFixedTable(_op, fileforma.iKmInc);
                fixedTable.Clear();
                for (int i = 0; i < autoIndexClsList.Count; i++)
                {
                    float mile = autoIndexClsList[i].km_current + autoIndexClsList[i].meter_current;
                    UserMarkedPoint markedPoint = new UserMarkedPoint();
                    markedPoint.ID = (i + 1).ToString();
                    markedPoint.FilePointer = autoIndexClsList[i].milePos;
                    markedPoint.UserSetMileage = mile;
                    fixedTable.MarkedPoints.Add(markedPoint);
                }
                fixedTable.Save();

                try
                {
                    MilestoneFix fix = new MilestoneFix(citFilePath, _op);
                    fix.RunFixingAlgorithm();
                    fix.SaveMilestoneFixTable();

                    resultInfo.flag = 1;
                    resultInfo.msg = "";
                    resultInfo.data = idfFilePath;
                }
                catch (Exception ex)
                {
                    resultInfo.flag = 0;
                    resultInfo.msg = ex.Message;
                }

            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(resultInfo);

        }
    }
}
