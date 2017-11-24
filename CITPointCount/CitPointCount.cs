using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using CitFileSDK;
using IntegratedDisplayCommon.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CitIndexFileSDK;
using CitIndexFileSDK.MileageFix;
using System.Linq;
using System.Runtime.InteropServices;

namespace CITPointCount
{

    public class CitPointCount
    {
        IOperator _op = null;

        CITFileProcess citHelper = new CITFileProcess();

        //CIT里程断点统计模块  获取断点数
        [DispId(91)]
        public string GetBreakPiontCount(string json) {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            resultInfo.flag = 1;
            resultInfo.msg = "转化成功";
            resultInfo.data = citList(json).Count.ToString();
            return JsonConvert.SerializeObject(resultInfo);
        }

        //CIT里程断点统计模块  最大正向跳变==>所有正差值中的最大值
        [DispId(92)]
        public string GetBreakPiontMax(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            resultInfo.flag = 1;
            resultInfo.msg = "转化成功";
            resultInfo.data = citList(json).Max(p => p.meter_between).ToString();
            return JsonConvert.SerializeObject(resultInfo);
        }

        //CIT里程断点统计模块  最大负向跳变==>所有负差值中的最小值(绝对值最大)
        [DispId(93)]
        public string GetBreakPiontMim(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            resultInfo.flag = 1;
            resultInfo.msg = "转化成功";
            resultInfo.data = citList(json).Min(p => p.meter_between).ToString();
            return JsonConvert.SerializeObject(resultInfo);
        }
        
        //CIT里程断点统计模块  平均跳变1==>所有差值求和，除以cit的里程长度。
        [DispId(94)]
        public string GetBreakPiontAverage1(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            JObject obj = (JObject)JsonConvert.DeserializeObject(json);
            string citFile = Convert.ToString(obj["citFile"]);
            double count = citHelper.GetTotalSampleCount(citFile) / 4 / 1000;
            double tiaobian1 = citList(json).Sum(p => p.meter_between) / count;
            resultInfo.flag = 1;
            resultInfo.msg = "转化成功";
            resultInfo.data = tiaobian1.ToString();
            return JsonConvert.SerializeObject(resultInfo);
        }
        
        //CIT里程断点统计模块  平均跳变2==>所以差值绝对值之和除以跳变个数。
        [DispId(95)]
        public string GetBreakPiontAverage2(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            resultInfo.flag = 1;
            resultInfo.msg = "转化成功";
            double tiaobian2 = citList(json).Sum(p => Math.Abs(p.meter_between))/citList(json).Count;
            resultInfo.data = tiaobian2.ToString();
            return JsonConvert.SerializeObject(resultInfo);
        }

        //获取list
        private List<AutoIndex> citList(string json)
        {
            List<AutoIndex> autoIndexClsList=new List<AutoIndex>();
            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                int jumpValue = Convert.ToInt32(obj["jumpValue"].ToString());//跳变允许值jumpValue
                autoIndexClsList = _readCit(citFile, jumpValue);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return autoIndexClsList;
        }

        /// <summary>
        /// 读取CIT文件
        /// </summary>
        /// <param name="citFilePath">cit文件路径----->来自里程快速校正中的选择cit文件的按钮</param
        /// <param name="numericUpDown1">跳变允许值----->来自里程快速校正中的  跳变容许值</param>
        private List<AutoIndex> _readCit(String citFilePath,int numericUpDown1)
        {
            List<AutoIndex> autoIndexClsList = new List<AutoIndex>();
            //autoIndexClsList.Max(p => p.meter_between);
            //autoIndexClsList.Min(p => p.meter_between);
            //double  count = citHelper.GetTotalSampleCount(citFilePath)/4/1000;
            //double tiaobian1=autoIndexClsList.Sum(p => p.meter_between)/count;
            //double tiaobian1i = autoIndexClsList.Sum(p => Math.Abs(p.meter_between))/autoIndexClsList.Count;
            if (numericUpDown1<= 0)
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
               
                Console.WriteLine("读取CIT文件跳变点失败:" + ex.Message + ",堆栈：" + ex.StackTrace);
                
            }
            return autoIndexClsList;
        }

    }
}
