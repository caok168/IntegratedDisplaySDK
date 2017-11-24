using CitFileSDK;
using CitIndexFileSDK;
using CitIndexFileSDK.MileageFix;
using CommonFileSDK;
using IntegratedDisplayCommon.Model;
using InvalidDataProcessing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PeakValue
{
    public class PeakValue
    {
        // CIT文件相关操作类
        CITFileProcess cfprocess = new CITFileProcess();

        // 通道定义相关操作类
        ChannelDefinitionList cdlist = new ChannelDefinitionList();

        //matlab算法
        PreproceingDeviationClass pdc = new PreproceingDeviationClass();

        //获取文件信息
        FileInformation fileinfo = new FileInformation();

        /// <summary>
        /// 峰峰值处理事件
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(161)]
        public string HandlePeakValue(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                string idfFile = Convert.ToString(obj["idfFile"]);
                string innerdbpath = Convert.ToString(obj["innerdbpath"]);

                //将文件按传入点位数分段读取
                int pointCount = Convert.ToInt32(obj["pointCount"].ToString());

                InnerFileOperator.InnerFilePath = innerdbpath;
                InnerFileOperator.InnerConnString = "Provider = Microsoft.Jet.OLEDB.4.0; Data Source = {0}; Persist Security Info = True; Mode = Share Exclusive; Jet OLEDB:Database Password = iicdc; ";
            
                List<String> list ;
                if (!String.IsNullOrEmpty(idfFile))
                {
                    //修正
                    list = PreProcessDeviation(citFile, pointCount,idfFile);
                }
                else
                {
                    //未修正
                    list = PreProcessDeviation(citFile, pointCount);
                }
                
                string data = JsonConvert.SerializeObject(list);
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

        /// <summary>
        /// 保存到CSV文件方法
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(162)]
        public string ExportExcel(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                //cit1文件路径
                string citFile = Convert.ToString(obj["citFile"]);
                string idfFile = Convert.ToString(obj["idfFile"]);
                string innerdbpath = Convert.ToString(obj["innerdbpath"]);
                //将文件按传入点位数分段读取
                int pointCount = Convert.ToInt32(obj["pointCount"].ToString());

                InnerFileOperator.InnerFilePath = innerdbpath;
                InnerFileOperator.InnerConnString = "Provider = Microsoft.Jet.OLEDB.4.0; Data Source = {0}; Persist Security Info = True; Mode = Share Exclusive; Jet OLEDB:Database Password = iicdc; ";

                List<String> list;
                if (!String.IsNullOrEmpty(idfFile))
                {
                    //修正
                    list = PreProcessDeviation(citFile, pointCount, idfFile);
                }
                else
                {
                    //未修正
                    list = PreProcessDeviation(citFile, pointCount);
                }

                string data = _exportExcel(citFile, list);

                resultInfo.flag = 1;
                resultInfo.msg = "";
                resultInfo.data = data;

            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
                return JsonConvert.SerializeObject(resultInfo);
            }

            return JsonConvert.SerializeObject(resultInfo);
        }

        /// <summary>
        /// 接口函数：计算峰峰值指标
        /// </summary>
        /// <param name="citFileName">cit文件全路径</param>
        /// <param name="citFileName">idf文件全路径</param>
        /// <returns></returns>
        private List<String> PreProcessDeviation2(String citFileName, int pointCount, string idfFileName = null)
        {
            List<String> dataStrList = new List<String>();

            cdlist.channelDefinitionList = cfprocess.GetChannelDefinitionList(citFileName);

            fileinfo = cfprocess.GetFileInformation(citFileName);
            //int tds = fileinfo.iChannelNumber;

            long[] position = cfprocess.GetPositons(citFileName);
            long startPos = position[0]; //开始位置、结束位置
            long endPos = position[1];

            List<Milestone> allmilelist;
            List<Milestone> milelist = cfprocess.GetAllMileStone(citFileName);

            //验证是否修正
            if (!String.IsNullOrEmpty(idfFileName))
            {
                IndexOperator _op = new IndexOperator();
                _op.IndexFilePath = idfFileName;
                MilestoneFix mile = new MilestoneFix(citFileName, _op);
                mile.ReadMilestoneFixTable();
                allmilelist = mile.GetMileageReviseData(milelist);
            }
            else
            {
                allmilelist = milelist;
            }

            //开始里程  和结束里程
            double[] d_tt = new double[allmilelist.Count];
            for (int i = 0; i < allmilelist.Count; i++)
            {
                double obj = allmilelist[i].GetMeter() / 1000;
                d_tt[i] = obj;
            }

            double[] d_wvelo = cfprocess.GetOneChannelDataInRange(citFileName, cdlist.GetChannelIdByName("Speed", "速度"), startPos, endPos);
            double[] d_gauge = cfprocess.GetOneChannelDataInRange(citFileName, cdlist.GetChannelIdByName("Gage", "轨距"), startPos, endPos);

            double[] d_wx = cfprocess.GetOneChannelDataInRange(citFileName, cdlist.GetChannelIdByName("L_Prof_SC", "左高低_中波"), startPos, endPos);

            //StreamWriter sw2 = new StreamWriter("d:/peakvalue_all.csv", true, Encoding.Default);
            //StringBuilder sbtmp = new StringBuilder();
            //sbtmp.Append("d_tt,");
            //sbtmp.Append("d_wvelo,");
            //sbtmp.Append("d_gauge,");
            //sbtmp.Append("d_wx");
            //sw2.WriteLine(sbtmp.ToString());
            //for (int i = 0; i < d_tt.Length; i++)
            //{
            //    sw2.Write(d_tt[i]);
            //    sw2.Write(",");
            //    sw2.Write(d_wvelo[i]);
            //    sw2.Write(",");
            //    sw2.Write(d_gauge[i]);
            //    sw2.Write(",");
            //    sw2.Write(d_wx[i]);
            //    sw2.Write("\n");
            //}
            //sw2.Close();

            List<String> tmpDataStrList = pdc.WideGaugePreProcess("左高低_中波", d_tt, d_wx, d_wvelo, d_gauge, 8.0);
            dataStrList.AddRange(tmpDataStrList);

            d_wx = cfprocess.GetOneChannelDataInRange(citFileName, cdlist.GetChannelIdByName("R_Prof_SC", "右高低_中波"), startPos, endPos);

            tmpDataStrList = pdc.WideGaugePreProcess("右高低_中波", d_tt, d_wx, d_wvelo, d_gauge, 8.0);
            dataStrList.AddRange(tmpDataStrList);

            d_wx = cfprocess.GetOneChannelDataInRange(citFileName, cdlist.GetChannelIdByName("L_Align_SC", "左轨向_中波"), startPos, endPos);

            tmpDataStrList = pdc.WideGaugePreProcess("左轨向_中波", d_tt, d_wx, d_wvelo, d_gauge, 8.0);
            dataStrList.AddRange(tmpDataStrList);

            d_wx = cfprocess.GetOneChannelDataInRange(citFileName, cdlist.GetChannelIdByName("R_Align_SC", "右轨向_中波"), startPos, endPos);

            tmpDataStrList = pdc.WideGaugePreProcess("右轨向_中波", d_tt, d_wx, d_wvelo, d_gauge, 8.0);
            dataStrList.AddRange(tmpDataStrList);

            return dataStrList;
        }

        /// <summary>
        /// 接口函数：计算峰峰值指标
        /// </summary>
        /// <param name="citFileName">cit文件全路径</param>
        /// <param name="citFileName">idf文件全路径</param>
        /// <returns></returns>
        private List<String> PreProcessDeviation(String citFileName, int pointCount, string idfFileName = null)
        {
            //StreamWriter sw3 = new StreamWriter("d:/peakvalue_40000.csv", true, Encoding.Default);
            //StringBuilder sbtmp = new StringBuilder();
            //sbtmp.Append("d_tt,");
            //sbtmp.Append("d_wvelo,");
            //sbtmp.Append("d_gauge,");
            //sbtmp.Append("d_wx");
            //sw3.WriteLine(sbtmp.ToString());

            List<String> dataStrList = new List<String>();

            cdlist.channelDefinitionList = cfprocess.GetChannelDefinitionList(citFileName);

            fileinfo = cfprocess.GetFileInformation(citFileName);
            //int tds = fileinfo.iChannelNumber;

            long[] position = cfprocess.GetPositons(citFileName);
            long startPos = position[0]; //开始位置、结束位置
            long endPos = position[1];

            //分段读取方法////////////////////

            long totleSample = cfprocess.GetTotalSampleCount(citFileName);
            //循环次数
            int count =Convert.ToInt32(totleSample / pointCount);
            //是否有余点
            int residue = Convert.ToInt32(totleSample % pointCount);

            bool iszero = false;
            //是否执行一次
            if (count == 0){
                iszero = true;
            }
            //如果有余数循环次数加1
            if (residue > 0) {
                count++;
            } 

            for (int z = 0; z < count; z++)
            {
                if (iszero)
                {
                    endPos = cfprocess.GetAppointEndPostion(citFileName, startPos, residue);
                }
                else {
                    if (residue == 0)
                    {
                        endPos = cfprocess.GetAppointEndPostion(citFileName, startPos, pointCount);
                    }
                    else
                    {
                        if (z == (count - 1))
                        {
                            endPos = cfprocess.GetAppointEndPostion(citFileName, startPos, residue);
                        }
                        else
                        {
                            endPos = cfprocess.GetAppointEndPostion(citFileName, startPos, pointCount);
                        }
                    }
                }
                
                //分段读取方法////////////////////

                List<Milestone> allmilelist;
                //List<Milestone> milelist = cfprocess.GetAllMileStone(citFileName);
                ///分段读取使用//////////////////////////
                List<Milestone> milelist = cfprocess.GetMileStoneByRange(citFileName, startPos, endPos);
                /////////////////////////////

                //验证是否修正
                if (!String.IsNullOrEmpty(idfFileName))
                {
                    IndexOperator _op = new IndexOperator();
                    _op.IndexFilePath = idfFileName;
                    MilestoneFix mile = new MilestoneFix(citFileName, _op);
                    mile.ReadMilestoneFixTable();
                    allmilelist = mile.GetMileageReviseData(milelist);
                }
                else
                {
                    allmilelist = milelist;
                }

                //开始里程  和结束里程
                double[] d_tt = new double[allmilelist.Count];
                for (int i = 0; i < allmilelist.Count; i++)
                {
                    double obj = allmilelist[i].GetMeter() / 1000;
                    d_tt[i] = obj;
                }

                double[] d_wvelo = cfprocess.GetOneChannelDataInRange(citFileName, cdlist.GetChannelIdByName("Speed", "速度"), startPos, endPos);
                double[] d_gauge = cfprocess.GetOneChannelDataInRange(citFileName, cdlist.GetChannelIdByName("Gage", "轨距"), startPos, endPos);

                double[] d_wx = cfprocess.GetOneChannelDataInRange(citFileName, cdlist.GetChannelIdByName("L_Prof_SC", "左高低_中波"), startPos, endPos);


                //for (int i = 0; i < d_tt.Length; i++)
                //{
                //    sw3.Write(d_tt[i]);
                //    sw3.Write(",");
                //    sw3.Write(d_wvelo[i]);
                //    sw3.Write(",");
                //    sw3.Write(d_gauge[i]);
                //    sw3.Write(",");
                //    sw3.Write(d_wx[i]);
                //    sw3.Write("\n");
                //}


                List<String> tmpDataStrList = pdc.WideGaugePreProcess("左高低_中波", d_tt, d_wx, d_wvelo, d_gauge, 8.0);
                dataStrList.AddRange(tmpDataStrList);

                d_wx = cfprocess.GetOneChannelDataInRange(citFileName, cdlist.GetChannelIdByName("R_Prof_SC", "右高低_中波"), startPos, endPos);

                tmpDataStrList = pdc.WideGaugePreProcess("右高低_中波", d_tt, d_wx, d_wvelo, d_gauge, 8.0);
                dataStrList.AddRange(tmpDataStrList);

                d_wx = cfprocess.GetOneChannelDataInRange(citFileName, cdlist.GetChannelIdByName("L_Align_SC", "左轨向_中波"), startPos, endPos);

                tmpDataStrList = pdc.WideGaugePreProcess("左轨向_中波", d_tt, d_wx, d_wvelo, d_gauge, 8.0);
                dataStrList.AddRange(tmpDataStrList);

                d_wx = cfprocess.GetOneChannelDataInRange(citFileName, cdlist.GetChannelIdByName("R_Align_SC", "右轨向_中波"), startPos, endPos);

                tmpDataStrList = pdc.WideGaugePreProcess("右轨向_中波", d_tt, d_wx, d_wvelo, d_gauge, 8.0);
                dataStrList.AddRange(tmpDataStrList);

                //分段读取方法////////////////////
                startPos = endPos;
            }

            //sw3.Close();

            //分段读取方法////////////////////
            return dataStrList;
        }

        /// <summary>
        /// 峰峰值导出
        /// </summary>
        /// <param name="citFile"></param>
        /// <returns></returns>
        private string _exportExcel(string citFile, List<string> result)
        {

            String excelPath = null;
            String excelName = null;

            if (result.Count == 0)
            {
                throw new Exception("输出结果为空！");
            }

            excelPath = Path.GetDirectoryName(citFile);
            excelName = Path.GetFileNameWithoutExtension(citFile);


            excelName = excelName + "_PeakValue.csv";

            excelPath = Path.Combine(excelPath, excelName);

            StreamWriter sw = new StreamWriter(excelPath, false, Encoding.Default);

            StringBuilder sbtmp = new StringBuilder();

            sbtmp.Append("序号,");
            sbtmp.Append("通道名,");
            sbtmp.Append("起点位置,");
            sbtmp.Append("终点位置,");
            sbtmp.Append("峰峰值差的绝对值");

            sw.WriteLine(sbtmp.ToString());

            for (int i = 0; i < result.Count; i++)
            {
                String[] dataStrArry = result[i].Split(',');

                sw.Write(i + 1);
                sw.Write(",");
                sw.Write(dataStrArry[0]);
                sw.Write(",");
                sw.Write(dataStrArry[1]);
                sw.Write(",");
                sw.Write(dataStrArry[2]);
                sw.Write(",");
                sw.Write(dataStrArry[3]);
                sw.Write("\n");
            }

            sw.Close();

            return excelPath;
        }

    }
}
