using CitFileSDK;
using CitIndexFileSDK;
using CitIndexFileSDK.MileageFix;
using CommonFileSDK;
using IntegratedDisplayCommon.Model;
using InvalidDataProcessing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SmallChangeIdentify
{
    public class ChangeDetection
    {

        // CIT文件相关操作类
        CITFileProcess cfprocess = new CITFileProcess();

        // 通道定义相关操作类
        ChannelDefinitionList cdlist = new ChannelDefinitionList();

        // 通道定义相关操作类2
        ChannelDefinitionList cdlist2 = new ChannelDefinitionList();

        //获取文件信息
        FileInformation fileinfo = new FileInformation();

        //获取文件信息2
        FileInformation fileinfo2 = new FileInformation();

        //matlab算法
        ChangeDetectionProcess changeDetcPro =new ChangeDetectionProcess();

        double startMile = 0;
        double endMile = 0;
        long startPos = 0;
        long endPos = 0;

        /// <summary>
        /// 微小变化识别处理（读取）
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(141)]
        public string SmallChange(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                //cit1文件路径
                string citFile = Convert.ToString(obj["citFile"]);
                //cit2文件路径
                string citFile2 = Convert.ToString(obj["citFile2"]);
                //是否修正
                bool isCorrect = Convert.ToBoolean(obj["isCorrect"].ToString());
                //idf1文件路径
                string idfFile = Convert.ToString(obj["idfFile"]);
                //idf2文件路径
                string idfFile2 = Convert.ToString(obj["idfFile2"]);
                //innerdb路径
                string innerdbpath = Convert.ToString(obj["innerdbpath"]);

                //将文件按传入点位数分段读取
                int pointCount = Convert.ToInt32(obj["pointCount"].ToString());

                InnerFileOperator.InnerFilePath = innerdbpath;
                InnerFileOperator.InnerConnString = "Provider = Microsoft.Jet.OLEDB.4.0; Data Source = {0}; Persist Security Info = True; Mode = Share Exclusive; Jet OLEDB:Database Password = iicdc; ";
            
                List<string> result =new List<string>();
                if(isCorrect){
                    result = _validsmallchange(citFile, idfFile, citFile2, idfFile2, pointCount);
                }
                else {
                    result = _unvalidsmallchange(citFile, citFile2, pointCount);
                }                

                string data = JsonConvert.SerializeObject(result);

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
        /// 保存到CSV文件方法
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(142)]
        public string ExportExcel(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                //cit1文件路径
                string citFile = Convert.ToString(obj["citFile"]);
                //cit2文件路径
                string citFile2 = Convert.ToString(obj["citFile2"]);
                //是否修正
                bool isCorrect = Convert.ToBoolean(obj["isCorrect"].ToString());
                //idf1文件路径
                string idfFile = Convert.ToString(obj["idfFile"]);
                //idf2文件路径
                string idfFile2 = Convert.ToString(obj["idfFile2"]);
                //innerdb路径
                string innerdbpath = Convert.ToString(obj["innerdbpath"]);
                //将文件按传入点位数分段读取
                int pointCount = Convert.ToInt32(obj["pointCount"].ToString());

                InnerFileOperator.InnerFilePath = innerdbpath;
                InnerFileOperator.InnerConnString = "Provider = Microsoft.Jet.OLEDB.4.0; Data Source = {0}; Persist Security Info = True; Mode = Share Exclusive; Jet OLEDB:Database Password = iicdc; ";

                List<string> result = new List<string>();
                if (isCorrect)
                {
                    result = _validsmallchange(citFile, idfFile, citFile2, idfFile2, pointCount);
                }
                else
                {
                    result = _unvalidsmallchange(citFile, citFile2, pointCount);
                }  

                string data = _exportExcel(citFile, result);

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
        /// 保存数据到数据库
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(143)]
        public string SaveData(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                //cit1文件路径
                string citFile = Convert.ToString(obj["citFile"]);
                //cit2文件路径
                string citFile2 = Convert.ToString(obj["citFile2"]);
                //是否修正
                bool isCorrect = Convert.ToBoolean(obj["isCorrect"].ToString());
                //idf1文件路径
                string idfFile = Convert.ToString(obj["idfFile"]);
                //idf2文件路径
                string idfFile2 = Convert.ToString(obj["idfFile2"]);
                //innerdb路径
                string innerdbpath = Convert.ToString(obj["innerdbpath"]);
                //将文件按传入点位数分段读取
                int pointCount = Convert.ToInt32(obj["pointCount"].ToString());

                InnerFileOperator.InnerFilePath = innerdbpath;
                InnerFileOperator.InnerConnString = "Provider = Microsoft.Jet.OLEDB.4.0; Data Source = {0}; Persist Security Info = True; Mode = Share Exclusive; Jet OLEDB:Database Password = iicdc; ";

                List<string> result = new List<string>();
                if (isCorrect)
                {
                    result = _validsmallchange(citFile, idfFile, citFile2, idfFile2, pointCount);
                }
                else
                {
                    result = _unvalidsmallchange(citFile, citFile2, pointCount);
                }  

                string data = _saveData(citFile,citFile2, idfFile, result);

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
        /// 保存到CSV文件方法
        /// </summary>
        /// <param name="citPath"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private string _exportExcel(string citPath,List<string> result)
        {
            String excelPath = null;
            String excelName = null;

            if (result.Count == 0)
            {
                throw new Exception("输出结果为空！");
            }

            excelPath = Path.GetDirectoryName(citPath);
            excelName = Path.GetFileNameWithoutExtension(citPath);


            excelName = excelName + "_SmallChange.csv";

            excelPath = Path.Combine(excelPath, excelName);

            StreamWriter sw = new StreamWriter(excelPath, false, Encoding.Default);

            StringBuilder sbtmp = new StringBuilder();

            sbtmp.Append("序号,");
            sbtmp.Append("通道名,");
            sbtmp.Append("起点位置,");
            sbtmp.Append("终点位置,");
            sbtmp.Append("幅值绝对值差");

            sw.WriteLine(sbtmp.ToString());

            for (int i = 0; i < result.Count; i++)
            {
                String[] dataStrArry = result[i].Split(',');

                sw.Write(i+1);
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

        private string _saveData(string citFile, string citFile2, string idfFile, List<string> result)
        {
            //需要考虑是修正前还是修正后，修正后数据可保存到idf，修正前如何处理
            IndexOperator indexOperator = new IndexOperator();
            if (File.Exists(idfFile))
            {
                //清空微小变化表
                indexOperator.IndexFilePath = idfFile;
                string cmdText = "delete from ChangeInfo";
                indexOperator.ExcuteSql(cmdText);

                for (int i = 0; i < result.Count; i++)
                {
                    String[] dataStrArry = result[i].Split(',');
                    string sqlInsert = "insert into ChangeInfo values(" + (i + 1) + ",'" + citFile2 + "','" + dataStrArry[0].ToString() + "','" + dataStrArry[1].ToString() + "','" + dataStrArry[2].ToString() + "','" + dataStrArry[3].ToString() + "')";
                    indexOperator.ExcuteSql(sqlInsert);
                }

                return idfFile;
            }
            else {
                string excelPath = Path.GetDirectoryName(citFile);
                string excelName = Path.GetFileNameWithoutExtension(citFile);
                excelName = excelName + "_UnMileageFix.idf";
                excelPath = Path.Combine(excelPath, excelName);

                //清空微小变化表
                indexOperator.IndexFilePath = excelPath;
                string cmdText = "delete from ChangeInfo";
                indexOperator.ExcuteSql(cmdText);

                for (int i = 0; i < result.Count; i++)
                {
                    String[] dataStrArry = result[i].Split(',');
                    string sqlInsert = "insert into ChangeInfo values(" + (i + 1) + ",'" + citFile + "','" + dataStrArry[0].ToString() + "','" + dataStrArry[1].ToString() + "','" + dataStrArry[2].ToString() + "','" + dataStrArry[3].ToString() + "')";
                    indexOperator.ExcuteSql(sqlInsert);
                }

                return idfFile;
            }
        }

        /// <summary>
        /// 修正cit文件对比
        /// </summary>
        /// <param name="cit_1"></param>
        /// <param name="cit_2"></param>
        /// <returns></returns>
        private List<string> _validsmallchange(string cit_1, string idf_1, string cit_2, string idf_2, int pointCount)
        {
            List<String> dataStrList = new List<String>();

            cdlist.channelDefinitionList = cfprocess.GetChannelDefinitionList(cit_1);
            cdlist2.channelDefinitionList = cfprocess.GetChannelDefinitionList(cit_2);

            fileinfo = cfprocess.GetFileInformation(cit_1);
            fileinfo2 = cfprocess.GetFileInformation(cit_2);

            //根据修正后的文件获取起始里程
            IndexOperator _op = new IndexOperator();
            _op.IndexFilePath = idf_1;
            MilestoneFix mile = new MilestoneFix(cit_1, _op);
            mile.ReadMilestoneFixTable();
            if (mile.FixData.Count == 0)
            {
                throw new Exception(idf_1 + "文件中的IndexSta表中无数据");
            }

            double startMeter_0 = mile.FixData[0].MarkedStartPoint.UserSetMileage;
            double endMeter_0 = mile.FixData[mile.FixData.Count - 1].MarkedEndPoint.UserSetMileage;
            //long startPos_0 = mile.FixData[0].MarkedStartPoint.FilePointer;
            //long endPos_0 = mile.FixData[mile.FixData.Count - 1].MarkedStartPoint.FilePointer;

            IndexOperator _op2 = new IndexOperator();
            _op2.IndexFilePath = idf_2;
            MilestoneFix mile2 = new MilestoneFix(cit_2, _op2);
            mile2.ReadMilestoneFixTable();

            if (mile2.FixData.Count == 0)
            {
                throw new Exception(idf_2 + "文件中的IndexSta表中无数据");
            }

            double startMeter_1 = mile2.FixData[0].MarkedStartPoint.UserSetMileage;
            double endMeter_1 = mile2.FixData[mile2.FixData.Count - 1].MarkedEndPoint.UserSetMileage;
            //long startPos_1 = mile2.FixData[0].MarkedStartPoint.FilePointer;
            //long endPos_1 = mile2.FixData[mile2.FixData.Count - 1].MarkedEndPoint.FilePointer;

            //判断增减里程
            if (fileinfo.iKmInc != fileinfo2.iKmInc)
            {
                //两个波形增减里程不一致
                throw new Exception("两个波形增减里程不一致");
            }

            Boolean isKmInc_0 = false; //false代表减里程
            Boolean isKmInc_1 = false; //false代表减里程
            if (startMeter_0 < endMeter_0)
            {
                isKmInc_0 = true;//true代表增里程
            }
            if (startMeter_1 < endMeter_1)
            {
                isKmInc_1 = true;//true代表增里程
            }
            if (isKmInc_0 != isKmInc_1)
            {
                //两个波形增减里程不一致
                throw new Exception("两个波形增减里程不一致");
            }


            //取两文件的公共里程，公共指针
            if (fileinfo.iKmInc == 0)
            {
                startMile = startMeter_0;
                //startPos = startPos_0;
                if (startMeter_1 > startMeter_0)
                {
                    startMile = startMeter_1;
                    //startPos = startPos_1;
                }

                endMile = endMeter_0;
                //endPos = endPos_0;
                if (endMeter_1 < endMeter_0)
                {
                    endMile = endMeter_1;
                    //endPos = endPos_1;
                }
            }
            else
            {
                startMile = startMeter_0;
                //startPos = startPos_0;
                if (startMeter_1 < startMeter_0)
                {
                    startMile = startMeter_1;
                    //startPos = startPos_1;
                }

                endMile = endMeter_0;
                //endPos = endPos_0;
                if (endMeter_1 > endMeter_0)
                {
                    endMile = endMeter_1;
                    //endPos = endPos_1;
                }
            }

            #region cit1
            //分段读取方法////////////////////
            long[] position = cfprocess.GetPositons(cit_1);
            long startPos_cit1 = position[0];
            long endPos_cit1 = position[1];


            long totleSample_cit1 = cfprocess.GetTotalSampleCount(cit_1);
            //循环次数
            int count_cit1 = Convert.ToInt32(totleSample_cit1 / pointCount);
            //是否有余点
            int residue_cit1 = Convert.ToInt32(totleSample_cit1 % pointCount);

            bool iszero_cit1 = false;
            //是否执行一次
            if (count_cit1 == 0)
            {
                iszero_cit1 = true;
            }
            //如果有余数循环次数加1
            if (residue_cit1 > 0)
            {
                count_cit1++;
            }

            long commonstartPos_cit1 = -1;
            long commonendPos_cit1 = -1;
            for (int l = 0; l < count_cit1; l++)
            {
                if (iszero_cit1)
                {
                    endPos_cit1 = cfprocess.GetAppointEndPostion(cit_1, startPos_cit1, residue_cit1);
                }
                else
                {
                    if (residue_cit1 == 0)
                    {
                        endPos_cit1 = cfprocess.GetAppointEndPostion(cit_1, startPos_cit1, pointCount);
                    }
                    else
                    {
                        if (l == (count_cit1 - 1))
                        {
                            endPos_cit1 = cfprocess.GetAppointEndPostion(cit_1, startPos_cit1, residue_cit1);
                        }
                        else
                        {
                            endPos_cit1 = cfprocess.GetAppointEndPostion(cit_1, startPos_cit1, pointCount);
                        }
                    }
                }

                //先矫正
                List<Milestone> allmilelist = mile.GetMileageReviseData(cfprocess.GetMileStoneByRange(cit_1, startPos_cit1, endPos_cit1));

                //从公共指针位置开始查找,指针位置部分增减性
                
                //取两文件的公共里程，公共指针
                if (fileinfo.iKmInc == 0)
                {
                    if (commonstartPos_cit1 == -1)
                    {
                        Milestone stone_cit1 = allmilelist.Find(p => (p.GetMeter() >= startMile));

                        if (stone_cit1 != null)
                        {
                            commonstartPos_cit1 = stone_cit1.mFilePosition;
                        }
                    }
                    else
                    {
                        Milestone stone_cit1 = allmilelist.Find(p => (p.GetMeter() >= endMile));

                        if (stone_cit1 != null)
                        {
                            commonendPos_cit1 = stone_cit1.mFilePosition;
                            break;
                        }
                    }
                }
                else
                {
                    if (commonstartPos_cit1 == -1)
                    {
                        Milestone stone_cit1 = allmilelist.Find(p => (p.GetMeter() <= startMile));

                        if (stone_cit1 != null)
                        {
                            commonstartPos_cit1 = stone_cit1.mFilePosition;
                        }
                    }
                    else
                    {
                        Milestone stone_cit1 = allmilelist.Find(p => (p.GetMeter() <= endMile));

                        if (stone_cit1 != null)
                        {
                            commonendPos_cit1 = stone_cit1.mFilePosition;
                            break;
                        }
                    }
                }

                startPos_cit1 = endPos_cit1;

            }

            #endregion

            #region cit2
            //cit2文件
            long[] position2 = cfprocess.GetPositons(cit_2);
            long startPos_cit2 = position2[0];
            long endPos_cit2 = position2[1];


            long totleSample_cit2 = cfprocess.GetTotalSampleCount(cit_2);
            //循环次数
            int count_cit2 = Convert.ToInt32(totleSample_cit2 / pointCount);
            //是否有余点
            int residue_cit2 = Convert.ToInt32(totleSample_cit2 % pointCount);

            bool iszero_cit2 = false;
            //是否执行一次
            if (count_cit2 == 0)
            {
                iszero_cit2 = true;
            }
            //如果有余数循环次数加1
            if (residue_cit2 > 0)
            {
                count_cit2++;
            }

            long commonstartPos_cit2 = -1;
            long commonendPos_cit2 = -1;
            for (int l = 0; l < count_cit2; l++)
            {
                if (iszero_cit2)
                {
                    endPos_cit2 = cfprocess.GetAppointEndPostion(cit_2, startPos_cit2, residue_cit2);
                }
                else
                {
                    if (residue_cit2 == 0)
                    {
                        endPos_cit2 = cfprocess.GetAppointEndPostion(cit_2, startPos_cit2, pointCount);
                    }
                    else
                    {
                        if (l == (count_cit2 - 1))
                        {
                            endPos_cit2 = cfprocess.GetAppointEndPostion(cit_2, startPos_cit2, residue_cit2);
                        }
                        else
                        {
                            endPos_cit2 = cfprocess.GetAppointEndPostion(cit_2, startPos_cit2, pointCount);
                        }
                    }
                }

                //先矫正
                List<Milestone> allmilelist = mile2.GetMileageReviseData(cfprocess.GetMileStoneByRange(cit_2, startPos_cit2, endPos_cit2));

                //从公共指针位置开始查找,指针位置部分增减性

                //取两文件的公共里程，公共指针
                if (fileinfo2.iKmInc == 0)
                {
                    if (commonstartPos_cit2 == -1)
                    {
                        Milestone stone_cit2 = allmilelist.Find(p => (p.GetMeter() >= startMile));

                        if (stone_cit2 != null)
                        {
                            commonstartPos_cit2 = stone_cit2.mFilePosition;
                        }
                    }
                    else
                    {
                        Milestone stone_cit2 = allmilelist.Find(p => (p.GetMeter() >= endMile));

                        if (stone_cit2 != null)
                        {
                            commonendPos_cit2 = stone_cit2.mFilePosition;
                            break;
                        }
                    }
                }
                else
                {
                    if (commonstartPos_cit2 == -1)
                    {
                        Milestone stone_cit2 = allmilelist.Find(p => (p.GetMeter() <= startMile));

                        if (stone_cit2 != null)
                        {
                            commonstartPos_cit2 = stone_cit2.mFilePosition;
                        }
                    }
                    else
                    {
                        Milestone stone_cit2 = allmilelist.Find(p => (p.GetMeter() <= endMile));

                        if (stone_cit2 != null)
                        {
                            commonendPos_cit2 = stone_cit2.mFilePosition;
                            break;
                        }
                    }
                }                

                startPos_cit2 = endPos_cit2;

            }
            #endregion

            //比较两个文件的点数，取少的
            long cit1Point = cfprocess.GetSampleCountByRange(cit_1, commonstartPos_cit1, commonendPos_cit1);
            long cit2Point = cfprocess.GetSampleCountByRange(cit_2, commonstartPos_cit2, commonendPos_cit2);

            long totleSample = cit1Point > cit2Point ? cit2Point : cit1Point;

            //循环次数
            int count = Convert.ToInt32(totleSample / pointCount);
            //是否有余数
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

            //重置开始位置
            startPos_cit1 = commonstartPos_cit1;
            startPos_cit2 = commonstartPos_cit2;

            for (int z = 0; z < count; z++)
            {
                if (iszero)
                {
                    endPos_cit1 = cfprocess.GetAppointEndPostion(cit_1, startPos_cit1, residue);
                    endPos_cit2 = cfprocess.GetAppointEndPostion(cit_2, startPos_cit2, residue);
                }
                else
                {
                    if (residue == 0)
                    {
                        endPos_cit1 = cfprocess.GetAppointEndPostion(cit_1, startPos_cit1, pointCount);
                        endPos_cit2 = cfprocess.GetAppointEndPostion(cit_2, startPos_cit2, pointCount);
                    }
                    else
                    {
                        if (z == (count - 1))
                        {
                            endPos_cit1 = cfprocess.GetAppointEndPostion(cit_1, startPos_cit1, residue);
                            endPos_cit2 = cfprocess.GetAppointEndPostion(cit_2, startPos_cit2, residue);
                        }
                        else
                        {
                            endPos_cit1 = cfprocess.GetAppointEndPostion(cit_1, startPos_cit1, pointCount);
                            endPos_cit2 = cfprocess.GetAppointEndPostion(cit_2, startPos_cit2, pointCount);
                        }
                    }
                }

                //获取区间段的数据，然后根据该区间段数据进行修正，根据里程list获取里程数组
                //List<Milestone> totlelist = allmilelist.FindAll(p => p.mFilePosition >= startPos_0 && p.mFilePosition <= endPos_0);
                List<Milestone> tempmilelist = cfprocess.GetMileStoneByRange(cit_1, startPos_cit1, endPos_cit1);
                List<Milestone> dualmilelist = mile.GetMileageReviseData(tempmilelist);
                double[] d_tt_1 = new double[dualmilelist.Count];
                for (int i = 0; i < dualmilelist.Count; i++)
                {
                    double obj = dualmilelist[i].GetMeter() / 1000;
                    d_tt_1[i] = obj;
                }

                double[] d_wvelo_1 = cfprocess.GetOneChannelDataInRange(cit_1, cdlist.GetChannelIdByName("Speed", "速度"), startPos_cit1, endPos_cit1);
                double[] d_wx_gauge_1 = cfprocess.GetOneChannelDataInRange(cit_1, cdlist.GetChannelIdByName("Gage", "轨距"), startPos_cit1, endPos_cit1);

                //获取区间段的数据2，然后根据该区间段数据进行修正，根据里程list获取里程数组
                List<Milestone> tempmilelist2 = cfprocess.GetMileStoneByRange(cit_2, startPos_cit2, endPos_cit2);
                List<Milestone> dualmilelist2 = mile2.GetMileageReviseData(tempmilelist2);
                double[] d_tt_2 = new double[dualmilelist2.Count];
                for (int i = 0; i < dualmilelist2.Count; i++)
                {
                    double obj = dualmilelist2[i].GetMeter() / 1000;
                    d_tt_2[i] = obj;
                }
                double[] d_wvelo_2 = cfprocess.GetOneChannelDataInRange(cit_2, cdlist2.GetChannelIdByName("Speed", "速度"), startPos_cit2, endPos_cit2);
                double[] d_wx_gauge_2 = cfprocess.GetOneChannelDataInRange(cit_2, cdlist2.GetChannelIdByName("Gage", "轨距"), startPos_cit2, endPos_cit2);

                double[] d_wx_1 = cfprocess.GetOneChannelDataInRange(cit_1, cdlist.GetChannelIdByName("L_Prof_SC", "左高低_中波"), startPos_cit1, endPos_cit1);
                double[] d_wx_2 = cfprocess.GetOneChannelDataInRange(cit_2, cdlist2.GetChannelIdByName("L_Prof_SC", "左高低_中波"), startPos_cit2, endPos_cit2);

                List<String> tmpDataStrList = changeDetcPro.ChangeDetectionPrcs("左高低_中波", d_tt_1, d_wx_1, d_wvelo_1, d_wx_gauge_1, d_tt_2, d_wx_2, d_wvelo_2, d_wx_gauge_2);
                dataStrList.AddRange(tmpDataStrList);

                d_wx_1 = cfprocess.GetOneChannelDataInRange(cit_1, cdlist.GetChannelIdByName("R_Prof_SC", "右高低_中波"), startPos_cit1, endPos_cit1);
                d_wx_2 = cfprocess.GetOneChannelDataInRange(cit_2, cdlist2.GetChannelIdByName("R_Prof_SC", "右高低_中波"), startPos_cit2, endPos_cit2);

                tmpDataStrList = changeDetcPro.ChangeDetectionPrcs("右高低_中波", d_tt_1, d_wx_1, d_wvelo_1, d_wx_gauge_1, d_tt_2, d_wx_2, d_wvelo_2, d_wx_gauge_2);
                dataStrList.AddRange(tmpDataStrList);

                d_wx_1 = cfprocess.GetOneChannelDataInRange(cit_1, cdlist.GetChannelIdByName("L_Align_SC", "左轨向_中波"), startPos_cit1, endPos_cit1);
                d_wx_2 = cfprocess.GetOneChannelDataInRange(cit_2, cdlist2.GetChannelIdByName("L_Align_SC", "左轨向_中波"), startPos_cit2, endPos_cit2);

                tmpDataStrList = changeDetcPro.ChangeDetectionPrcs("左轨向_中波", d_tt_1, d_wx_1, d_wvelo_1, d_wx_gauge_1, d_tt_2, d_wx_2, d_wvelo_2, d_wx_gauge_2);
                dataStrList.AddRange(tmpDataStrList);

                d_wx_1 = cfprocess.GetOneChannelDataInRange(cit_1, cdlist.GetChannelIdByName("R_Align_SC", "右轨向_中波"), startPos_cit1, endPos_cit1);
                d_wx_2 = cfprocess.GetOneChannelDataInRange(cit_2, cdlist2.GetChannelIdByName("R_Align_SC", "右轨向_中波"), startPos_cit2, endPos_cit2);

                tmpDataStrList = changeDetcPro.ChangeDetectionPrcs("右轨向_中波", d_tt_1, d_wx_1, d_wvelo_1, d_wx_gauge_1, d_tt_2, d_wx_2, d_wvelo_2, d_wx_gauge_2);
                dataStrList.AddRange(tmpDataStrList);

            //分段读取方法////////////////////
                startPos_cit1 = endPos_cit1;
                startPos_cit2 = endPos_cit2;
            }
            //分段读取方法////////////////////
            return dataStrList;
        }


        /// <summary>
        /// 未修正cit文件对比
        /// </summary>
        /// <param name="cit_1"></param>
        /// <param name="cit_2"></param>
        /// <returns></returns>
        private List<string> _unvalidsmallchange(string cit_1, string cit_2, int pointCount)
        {
            List<String> dataStrList = new List<String>();

            cdlist.channelDefinitionList = cfprocess.GetChannelDefinitionList(cit_1);
            cdlist2.channelDefinitionList = cfprocess.GetChannelDefinitionList(cit_2);

            fileinfo = cfprocess.GetFileInformation(cit_1);
            fileinfo2 = cfprocess.GetFileInformation(cit_2);

            ////先找里程,不修正情况,增减里程读取cit文件
            //MilestoneList milelist = new MilestoneList();
            //milelist.milestoneList = cfprocess.GetAllMileStone(cit_1);

            //double startMeter_0 = milelist.GetStart();
            //double endMeter_0 = milelist.GetEnd();

            //MilestoneList milelist2 = new MilestoneList();
            //milelist2.milestoneList = cfprocess.GetAllMileStone(cit_2);

            //double startMeter_1 = milelist2.GetStart();
            //double endMeter_1 = milelist2.GetEnd();

            if (fileinfo.iKmInc != fileinfo2.iKmInc)
            {
                //两个波形增减里程不一致
                throw new Exception("两个波形增减里程不一致");
            }

            //Boolean isKmInc_0 = false; //false代表减里程
            //Boolean isKmInc_1 = false; //false代表减里程
            //if (startMeter_0 < endMeter_0)
            //{
            //    isKmInc_0 = true;//true代表增里程
            //}
            //if (startMeter_1 < endMeter_1)
            //{
            //    isKmInc_1 = true;//true代表增里程
            //}
            //if (isKmInc_0 != isKmInc_1)
            //{
            //    //两个波形增减里程不一致
            //    throw new Exception("两个波形增减里程不一致");
            //}

            #region 操作cit1文件
            long[] position = cfprocess.GetPositons(cit_1);
            long startPos_cit1 = position[0];
            long endPos_cit1 = position[1];

            #endregion

            #region 操作cit2文件
            long[] position2 = cfprocess.GetPositons(cit_2);
            long startPos_cit2 = position2[0];
            long endPos_cit2 = position2[1];

            #endregion

            ////获取指针公共部分，不用区分增减里程
            //startPos = startPos_cit1;
            //if (startPos_cit2 > startPos_cit1)
            //{
            //    startPos = startPos_cit2;
            //}

            //endPos = endPos_cit1;
            //if (endPos_cit2 < endPos_cit1)
            //{
            //    startPos = endPos_cit2;
            //}

            //根据指针直接找里程
            float startMeter_0 = cfprocess.GetAppointMilestone(cit_1, startPos_cit1).GetMeter();
            float endMeter_0 = cfprocess.GetAppointMilestone(cit_1, endPos_cit1).GetMeter();

            float startMeter_1 = cfprocess.GetAppointMilestone(cit_2, startPos_cit2).GetMeter();
            float endMeter_1 = cfprocess.GetAppointMilestone(cit_2, endPos_cit2).GetMeter();

            //获取两个文件公共里程
            if (fileinfo.iKmInc == 0)
            {
                startMile = startMeter_0;
                //startPos = startPos_0;
                if (startMeter_1 > startMeter_0)
                {
                    startMile = startMeter_1;
                    //startPos = startPos_1;
                }

                endMile = endMeter_0;
                //endPos = endPos_0;
                if (endMeter_1 < endMeter_0)
                {
                    endMile = endMeter_1;
                    //endPos = endPos_1;
                }
            }
            else
            {
                startMile = startMeter_0;
                //startPos = startPos_0;
                if (startMeter_1 < startMeter_0)
                {
                    startMile = startMeter_1;
                    //startPos = startPos_1;
                }

                endMile = endMeter_0;
                //endPos = endPos_0;
                if (endMeter_1 > endMeter_0)
                {
                    endMile = endMeter_1;
                    //endPos = endPos_1;
                }
            }

            //根据公共里程直接找位置
            long commonstartPos_cit1 = cfprocess.GetCurrentPositionByMilestone(cit_1, (float)startMile, true);
            long commonendPos_cit1 = cfprocess.GetCurrentPositionByMilestone(cit_1, (float)endMile, true);

            long commonstartPos_cit2 = cfprocess.GetCurrentPositionByMilestone(cit_2, (float)startMile, true);
            long commonendPos_cit2 = cfprocess.GetCurrentPositionByMilestone(cit_2, (float)endMile, true);


            //分段读取方法////////////////////
            //比较两个文件的点数，取少的
            long cit1Point = cfprocess.GetSampleCountByRange(cit_1, commonstartPos_cit1, commonendPos_cit1);
            long cit2Point = cfprocess.GetSampleCountByRange(cit_2, commonstartPos_cit2, commonendPos_cit2);

            long totleSample = cit1Point > cit2Point ? cit2Point : cit1Point;

            //循环次数
            int count = Convert.ToInt32(totleSample / pointCount);
            //是否有余点
            int residue = Convert.ToInt32(totleSample % pointCount);

            bool iszero = false;
            //是否执行一次
            if (count == 0)
            {
                iszero = true;
            }
            //如果有余数循环次数加1
            if (residue > 0)
            {
                count++;
            }

            //重置开始位置
            startPos_cit1 = commonstartPos_cit1;
            startPos_cit2 = commonstartPos_cit2;

            for (int z = 0; z < count; z++)
            {
                if (iszero)
                {
                    endPos_cit1 = cfprocess.GetAppointEndPostion(cit_1, startPos_cit1, residue);
                    endPos_cit2 = cfprocess.GetAppointEndPostion(cit_2, startPos_cit2, residue);
                }
                else
                {
                    if (residue == 0)
                    {
                        endPos_cit1 = cfprocess.GetAppointEndPostion(cit_1, startPos_cit1, pointCount);
                        endPos_cit2 = cfprocess.GetAppointEndPostion(cit_2, startPos_cit2, pointCount);
                    }
                    else
                    {
                        if (z == (count - 1))
                        {
                            endPos_cit1 = cfprocess.GetAppointEndPostion(cit_1, startPos_cit1, residue);
                            endPos_cit2 = cfprocess.GetAppointEndPostion(cit_2, startPos_cit2, residue);
                        }
                        else
                        {
                            endPos_cit1 = cfprocess.GetAppointEndPostion(cit_1, startPos_cit1, pointCount);
                            endPos_cit2 = cfprocess.GetAppointEndPostion(cit_2, startPos_cit2, pointCount);
                        }
                    }
                }

                //根据里程list获取里程数组
                List<Milestone> dualmilelist = cfprocess.GetMileStoneByRange(cit_1, startPos_cit1, endPos_cit1);
                double[] d_tt_1 = new double[dualmilelist.Count];
                for (int i = 0; i < dualmilelist.Count; i++)
                {
                    double obj = dualmilelist[i].GetMeter() / 1000;
                    d_tt_1[i] = obj;
                }

                double[] d_wvelo_1 = cfprocess.GetOneChannelDataInRange(cit_1, cdlist.GetChannelIdByName("Speed", "速度"), startPos_cit1, endPos_cit1);
                double[] d_wx_gauge_1 = cfprocess.GetOneChannelDataInRange(cit_1, cdlist.GetChannelIdByName("Gage", "轨距"), startPos_cit1, endPos_cit1);

                //根据里程list获取里程数组
                List<Milestone> dualmilelist2 = cfprocess.GetMileStoneByRange(cit_2, startPos_cit2, endPos_cit2);
                double[] d_tt_2 = new double[dualmilelist2.Count];
                for (int i = 0; i < dualmilelist2.Count; i++)
                {
                    double obj = dualmilelist2[i].GetMeter() / 1000;
                    d_tt_2[i] = obj;
                }
                double[] d_wvelo_2 = cfprocess.GetOneChannelDataInRange(cit_2, cdlist2.GetChannelIdByName("Speed", "速度"), startPos_cit2, endPos_cit2);
                double[] d_wx_gauge_2 = cfprocess.GetOneChannelDataInRange(cit_2, cdlist2.GetChannelIdByName("Gage", "轨距"), startPos_cit2, endPos_cit2);

                double[] d_wx_1 = cfprocess.GetOneChannelDataInRange(cit_1, cdlist.GetChannelIdByName("L_Prof_SC", "左高低_中波"), startPos_cit1, endPos_cit1);
                double[] d_wx_2 = cfprocess.GetOneChannelDataInRange(cit_2, cdlist2.GetChannelIdByName("L_Prof_SC", "左高低_中波"), startPos_cit2, endPos_cit2);

                List<String> tmpDataStrList = changeDetcPro.ChangeDetectionPrcs("左高低_中波", d_tt_1, d_wx_1, d_wvelo_1, d_wx_gauge_1, d_tt_2, d_wx_2, d_wvelo_2, d_wx_gauge_2);
                dataStrList.AddRange(tmpDataStrList);

                d_wx_1 = cfprocess.GetOneChannelDataInRange(cit_1, cdlist.GetChannelIdByName("R_Prof_SC", "右高低_中波"), startPos_cit1, endPos_cit1);
                d_wx_2 = cfprocess.GetOneChannelDataInRange(cit_2, cdlist2.GetChannelIdByName("R_Prof_SC", "右高低_中波"), startPos_cit2, endPos_cit2);

                tmpDataStrList = changeDetcPro.ChangeDetectionPrcs("右高低_中波", d_tt_1, d_wx_1, d_wvelo_1, d_wx_gauge_1, d_tt_2, d_wx_2, d_wvelo_2, d_wx_gauge_2);
                dataStrList.AddRange(tmpDataStrList);

                d_wx_1 = cfprocess.GetOneChannelDataInRange(cit_1, cdlist.GetChannelIdByName("L_Align_SC", "左轨向_中波"), startPos_cit1, endPos_cit1);
                d_wx_2 = cfprocess.GetOneChannelDataInRange(cit_2, cdlist2.GetChannelIdByName("L_Align_SC", "左轨向_中波"), startPos_cit2, endPos_cit2);

                tmpDataStrList = changeDetcPro.ChangeDetectionPrcs("左轨向_中波", d_tt_1, d_wx_1, d_wvelo_1, d_wx_gauge_1, d_tt_2, d_wx_2, d_wvelo_2, d_wx_gauge_2);
                dataStrList.AddRange(tmpDataStrList);

                d_wx_1 = cfprocess.GetOneChannelDataInRange(cit_1, cdlist.GetChannelIdByName("R_Align_SC", "右轨向_中波"), startPos_cit1, endPos_cit1);
                d_wx_2 = cfprocess.GetOneChannelDataInRange(cit_2, cdlist2.GetChannelIdByName("R_Align_SC", "右轨向_中波"), startPos_cit2, endPos_cit2);

                tmpDataStrList = changeDetcPro.ChangeDetectionPrcs("右轨向_中波", d_tt_1, d_wx_1, d_wvelo_1, d_wx_gauge_1, d_tt_2, d_wx_2, d_wvelo_2, d_wx_gauge_2);
                dataStrList.AddRange(tmpDataStrList);

                startPos_cit1 = endPos_cit1;
                startPos_cit2 = endPos_cit2;
            }

            return dataStrList;
        }


        ///// <summary>
        ///// 未修正cit文件对比
        ///// </summary>
        ///// <param name="cit_1"></param>
        ///// <param name="cit_2"></param>
        ///// <returns></returns>
        //private List<string> _unvalidsmallchange(string cit_1, string cit_2, int pointCount)
        //{
        //    List<String> dataStrList = new List<String>();

        //    cdlist.channelDefinitionList = cfprocess.GetChannelDefinitionList(cit_1);
        //    cdlist2.channelDefinitionList = cfprocess.GetChannelDefinitionList(cit_2);

        //    fileinfo= cfprocess.GetFileInformation(cit_1);
        //    fileinfo2=cfprocess.GetFileInformation(cit_2);

        //    //先找里程,不修正情况,增减里程读取cit文件
        //    MilestoneList milelist=new MilestoneList();
        //    milelist.milestoneList = cfprocess.GetAllMileStone(cit_1);

        //    double startMeter_0=milelist.GetStart();
        //    double endMeter_0 = milelist.GetEnd();

        //    MilestoneList milelist2=new MilestoneList();
        //    milelist2.milestoneList = cfprocess.GetAllMileStone(cit_2);

        //    double startMeter_1 = milelist2.GetStart();
        //    double endMeter_1 = milelist2.GetEnd();

        //    if (fileinfo.iKmInc != fileinfo2.iKmInc)
        //    {
        //        //两个波形增减里程不一致
        //        throw new Exception("两个波形增减里程不一致");
        //    }

        //    Boolean isKmInc_0 = false; //false代表减里程
        //    Boolean isKmInc_1 = false; //false代表减里程
        //    if (startMeter_0 < endMeter_0)
        //    {
        //        isKmInc_0 = true;//true代表增里程
        //    }
        //    if (startMeter_1 < endMeter_1)
        //    {
        //        isKmInc_1 = true;//true代表增里程
        //    }
        //    if (isKmInc_0 != isKmInc_1)
        //    {
        //        //两个波形增减里程不一致
        //        throw new Exception("两个波形增减里程不一致");
        //    }

        //    //如果为增里程
        //    if (fileinfo.iKmInc == 0)
        //    {
        //        startMile = startMeter_0;
        //        if (startMeter_1 > startMeter_0)
        //        {
        //            startMile = startMeter_1;
        //        }

        //        endMile = endMeter_0;
        //        if (endMeter_1 < endMeter_0)
        //        {
        //            endMile = endMeter_1;
        //        }
        //    }
        //    else
        //    {
        //        startMile = startMeter_0;
        //        if (startMeter_1 < startMeter_0)
        //        {
        //            startMile = startMeter_1;
        //        }

        //        endMile = endMeter_0;
        //        if (endMeter_1 > endMeter_0)
        //        {
        //            endMile = endMeter_1;
        //        }
        //    }

        //    //根据最小里程 去找指针
        //    long startPos_0 = cfprocess.GetCurrentPositionByMilestone(cit_1,(float)startMile,true);
        //    long endPos_0 = cfprocess.GetCurrentPositionByMilestone(cit_1, (float)endMile, true);

        //    long startPos_1 = cfprocess.GetCurrentPositionByMilestone(cit_2, (float)startMile, true);
        //    long endPos_1 = cfprocess.GetCurrentPositionByMilestone(cit_2, (float)endMile, true);

        //    //根据里程list获取里程数组
        //    List<Milestone> dualmilelist = cfprocess.GetMileStoneByRange(cit_1, startPos_0, endPos_0);
        //    double[] d_tt_1 = new double[dualmilelist.Count];
        //    for (int i = 0; i < dualmilelist.Count; i++)
        //    {
        //        double obj = dualmilelist[i].GetMeter() / 1000;
        //        d_tt_1[i] = obj;
        //    }

        //    double[] d_wvelo_1 = cfprocess.GetOneChannelDataInRange(cit_1, cdlist.GetChannelIdByName("Speed", "速度"), startPos_0, endPos_0);
        //    double[] d_wx_gauge_1 = cfprocess.GetOneChannelDataInRange(cit_1, cdlist.GetChannelIdByName("Gage", "轨距"), startPos_0, endPos_0);

        //    //根据里程list获取里程数组
        //    List<Milestone> dualmilelist2 = cfprocess.GetMileStoneByRange(cit_2, startPos_1, endPos_1);
        //    double[] d_tt_2 = new double[dualmilelist2.Count];
        //    for (int i = 0; i < dualmilelist2.Count; i++)
        //    {
        //        double obj = dualmilelist2[i].GetMeter() / 1000;
        //        d_tt_2[i] = obj;
        //    }
        //    double[] d_wvelo_2 = cfprocess.GetOneChannelDataInRange(cit_2, cdlist2.GetChannelIdByName("Speed", "速度"), startPos_1, endPos_1);
        //    double[] d_wx_gauge_2 = cfprocess.GetOneChannelDataInRange(cit_2, cdlist2.GetChannelIdByName("Gage", "轨距"), startPos_1, endPos_1);

        //    double[] d_wx_1 = cfprocess.GetOneChannelDataInRange(cit_1, cdlist.GetChannelIdByName( "L_Prof_SC", "左高低_中波"), startPos_0, endPos_0);
        //    double[] d_wx_2 = cfprocess.GetOneChannelDataInRange(cit_2, cdlist2.GetChannelIdByName("L_Prof_SC", "左高低_中波"), startPos_1, endPos_1);

        //    List<String> tmpDataStrList = changeDetcPro.ChangeDetectionPrcs("左高低_中波", d_tt_1, d_wx_1, d_wvelo_1, d_wx_gauge_1, d_tt_2, d_wx_2, d_wvelo_2, d_wx_gauge_2);
        //    dataStrList.AddRange(tmpDataStrList);

        //    d_wx_1 = cfprocess.GetOneChannelDataInRange(cit_1, cdlist.GetChannelIdByName( "R_Prof_SC", "右高低_中波"), startPos_0, endPos_0);
        //    d_wx_2 = cfprocess.GetOneChannelDataInRange(cit_2, cdlist2.GetChannelIdByName( "R_Prof_SC", "右高低_中波"), startPos_1, endPos_1);

        //    tmpDataStrList = changeDetcPro.ChangeDetectionPrcs("右高低_中波", d_tt_1, d_wx_1, d_wvelo_1, d_wx_gauge_1, d_tt_2, d_wx_2, d_wvelo_2, d_wx_gauge_2);
        //    dataStrList.AddRange(tmpDataStrList);

        //    d_wx_1 = cfprocess.GetOneChannelDataInRange(cit_1, cdlist.GetChannelIdByName("L_Align_SC", "左轨向_中波"), startPos_0, endPos_0);
        //    d_wx_2 = cfprocess.GetOneChannelDataInRange(cit_2, cdlist2.GetChannelIdByName( "L_Align_SC", "左轨向_中波"), startPos_1, endPos_1);

        //    tmpDataStrList = changeDetcPro.ChangeDetectionPrcs("左轨向_中波", d_tt_1, d_wx_1, d_wvelo_1, d_wx_gauge_1, d_tt_2, d_wx_2, d_wvelo_2, d_wx_gauge_2);
        //    dataStrList.AddRange(tmpDataStrList);

        //    d_wx_1 = cfprocess.GetOneChannelDataInRange(cit_1, cdlist.GetChannelIdByName( "R_Align_SC", "右轨向_中波"), startPos_0, endPos_0);
        //    d_wx_2 = cfprocess.GetOneChannelDataInRange(cit_2, cdlist2.GetChannelIdByName( "R_Align_SC", "右轨向_中波"), startPos_1, endPos_1);

        //    tmpDataStrList = changeDetcPro.ChangeDetectionPrcs("右轨向_中波", d_tt_1, d_wx_1, d_wvelo_1, d_wx_gauge_1, d_tt_2, d_wx_2, d_wvelo_2, d_wx_gauge_2);
        //    dataStrList.AddRange(tmpDataStrList);

        //    return dataStrList;
        //}

        
        ///// <summary>
        ///// 修正cit文件对比
        ///// </summary>
        ///// <param name="cit_1"></param>
        ///// <param name="cit_2"></param>
        ///// <returns></returns>
        //private List<string> _validsmallchange(string cit_1, string idf_1, string cit_2, string idf_2, int pointCount )
        //{
        //    List<String> dataStrList = new List<String>();

        //    cdlist.channelDefinitionList = cfprocess.GetChannelDefinitionList(cit_1);
        //    cdlist2.channelDefinitionList = cfprocess.GetChannelDefinitionList(cit_2);

        //    fileinfo = cfprocess.GetFileInformation(cit_1);
        //    fileinfo2 = cfprocess.GetFileInformation(cit_2);

        //        //根据修正后的文件获取起始里程
        //        IndexOperator _op = new IndexOperator();
        //        _op.IndexFilePath = idf_1;
        //        MilestoneFix mile = new MilestoneFix(cit_1, _op);
        //        mile.ReadMilestoneFixTable();
        //        double startMeter_0 = mile.FixData[0].MarkedStartPoint.UserSetMileage;
        //        double endMeter_0 = mile.FixData[mile.FixData.Count - 1].MarkedEndPoint.UserSetMileage;


        //        IndexOperator _op2 = new IndexOperator();
        //        _op2.IndexFilePath = idf_2;
        //        MilestoneFix mile2 = new MilestoneFix(cit_2, _op2);
        //        mile2.ReadMilestoneFixTable();
        //        double startMeter_1 = mile2.FixData[0].MarkedStartPoint.UserSetMileage;
        //        double endMeter_1 = mile2.FixData[mile2.FixData.Count - 1].MarkedEndPoint.UserSetMileage;

        //        if (fileinfo.iKmInc != fileinfo2.iKmInc)
            //{
            //    //两个波形增减里程不一致
            //    throw new Exception("两个波形增减里程不一致");
            //}

            //Boolean isKmInc_0 = false; //false代表减里程
            //Boolean isKmInc_1 = false; //false代表减里程
            //if (startMeter_0 < endMeter_0)
            //{
            //    isKmInc_0 = true;//true代表增里程
            //}
            //if (startMeter_1 < endMeter_1)
            //{
            //    isKmInc_1 = true;//true代表增里程
            //}
            //if (isKmInc_0 != isKmInc_1)
            //{
            //    //两个波形增减里程不一致
            //    throw new Exception("两个波形增减里程不一致");
            //}

        //如果为增里程
            //if (fileinfo.iKmInc==0)
            //{
            //    startMile = startMeter_0;
            //    if (startMeter_1 > startMeter_0)
            //    {
            //        startMile = startMeter_1;
            //    }

            //    endMile = endMeter_0;
            //    if (endMeter_1 < endMeter_0)
            //    {
            //        endMile = endMeter_1;
            //    }
            //}
            //else
            //{
            //    startMile = startMeter_0;
            //    if (startMeter_1 < startMeter_0)
            //    {
            //        startMile = startMeter_1;
            //    }

            //    endMile = endMeter_0;
            //    if (endMeter_1 > endMeter_0)
            //    {
            //        endMile = endMeter_1;
            //    }
            //}

        //        ////根据最小里程 去找指针
        //        List<Milestone> allmilelist = mile.GetMileageReviseData(cfprocess.GetAllMileStone(cit_1));
        //        long startPos_0;
        //        long endPos_0;

        //        if (fileinfo.iKmInc == 0)
        //        {
        //            startPos_0 = (long)allmilelist.FindLast(p => (p.GetMeter() / 1000) <= startMile).mFilePosition;
        //            endPos_0 = (long)allmilelist.FindLast(p => (p.GetMeter() / 1000) <= endMile).mFilePosition;
        //        }
        //        else
        //        {
        //            startPos_0 = (long)allmilelist.FindLast(p => (p.GetMeter() / 1000) >= startMile).mFilePosition;
        //            endPos_0 = (long)allmilelist.FindLast(p => (p.GetMeter() / 1000) >= endMile).mFilePosition;
        //        }

        //        List<Milestone> allmilelist2 = mile2.GetMileageReviseData(cfprocess.GetAllMileStone(cit_2));
        //        long startPos_1 = 0;
        //        long endPos_1 = 0;

        //        if (fileinfo2.iKmInc == 0)
        //        {
        //            startPos_1 = (long)allmilelist2.FindLast(p => (p.GetMeter() / 1000) <= startMile).mFilePosition;
        //            endPos_1 = (long)allmilelist2.FindLast(p => (p.GetMeter() / 1000) <= endMile).mFilePosition;
        //        }
        //        else
        //        {
        //            startPos_1 = (long)allmilelist2.FindLast(p => (p.GetMeter() / 1000) >= startMile).mFilePosition;
        //            endPos_1 = (long)allmilelist2.FindLast(p => (p.GetMeter() / 1000) >= endMile).mFilePosition;
        //        }

        //        //获取区间段的数据，然后根据该区间段数据进行修正，根据里程list获取里程数组
        //        //List<Milestone> totlelist = allmilelist.FindAll(p => p.mFilePosition >= startPos_0 && p.mFilePosition <= endPos_0);
        //        List<Milestone> tempmilelist = cfprocess.GetMileStoneByRange(cit_1, startPos_0, endPos_0);
        //        List<Milestone> dualmilelist = mile.GetMileageReviseData(tempmilelist);
        //        double[] d_tt_1 = new double[dualmilelist.Count];
        //        for (int i = 0; i < dualmilelist.Count; i++)
        //        {
        //            double obj = dualmilelist[i].GetMeter() / 1000;
        //            d_tt_1[i] = obj;
        //        }

        //        double[] d_wvelo_1 = cfprocess.GetOneChannelDataInRange(cit_1, cdlist.GetChannelIdByName("Speed", "速度"), startPos_0, endPos_0);
        //        double[] d_wx_gauge_1 = cfprocess.GetOneChannelDataInRange(cit_1, cdlist.GetChannelIdByName("Gage", "轨距"), startPos_0, endPos_0);

        //        //获取区间段的数据2，然后根据该区间段数据进行修正，根据里程list获取里程数组
        //        List<Milestone> tempmilelist2 = cfprocess.GetMileStoneByRange(cit_2, startPos_1, endPos_1);
        //        List<Milestone> dualmilelist2 = mile.GetMileageReviseData(tempmilelist2);
        //        double[] d_tt_2 = new double[dualmilelist2.Count];
        //        for (int i = 0; i < dualmilelist2.Count; i++)
        //        {
        //            double obj = dualmilelist2[i].GetMeter() / 1000;
        //            d_tt_2[i] = obj;
        //        }
        //        double[] d_wvelo_2 = cfprocess.GetOneChannelDataInRange(cit_2, cdlist2.GetChannelIdByName("Speed", "速度"), startPos_1, endPos_1);
        //        double[] d_wx_gauge_2 = cfprocess.GetOneChannelDataInRange(cit_2, cdlist2.GetChannelIdByName("Gage", "轨距"), startPos_1, endPos_1);

        //        double[] d_wx_1 = cfprocess.GetOneChannelDataInRange(cit_1, cdlist.GetChannelIdByName("L_Prof_SC", "左高低_中波"), startPos_0, endPos_0);
        //        double[] d_wx_2 = cfprocess.GetOneChannelDataInRange(cit_2, cdlist2.GetChannelIdByName("L_Prof_SC", "左高低_中波"), startPos_1, endPos_1);

        //        List<String> tmpDataStrList = changeDetcPro.ChangeDetectionPrcs("左高低_中波", d_tt_1, d_wx_1, d_wvelo_1, d_wx_gauge_1, d_tt_2, d_wx_2, d_wvelo_2, d_wx_gauge_2);
        //        dataStrList.AddRange(tmpDataStrList);

        //        d_wx_1 = cfprocess.GetOneChannelDataInRange(cit_1, cdlist.GetChannelIdByName("R_Prof_SC", "右高低_中波"), startPos_0, endPos_0);
        //        d_wx_2 = cfprocess.GetOneChannelDataInRange(cit_2, cdlist2.GetChannelIdByName("R_Prof_SC", "右高低_中波"), startPos_1, endPos_1);

        //        tmpDataStrList = changeDetcPro.ChangeDetectionPrcs("右高低_中波", d_tt_1, d_wx_1, d_wvelo_1, d_wx_gauge_1, d_tt_2, d_wx_2, d_wvelo_2, d_wx_gauge_2);
        //        dataStrList.AddRange(tmpDataStrList);

        //        d_wx_1 = cfprocess.GetOneChannelDataInRange(cit_1, cdlist.GetChannelIdByName("L_Align_SC", "左轨向_中波"), startPos_0, endPos_0);
        //        d_wx_2 = cfprocess.GetOneChannelDataInRange(cit_2, cdlist2.GetChannelIdByName("L_Align_SC", "左轨向_中波"), startPos_1, endPos_1);

        //        tmpDataStrList = changeDetcPro.ChangeDetectionPrcs("左轨向_中波", d_tt_1, d_wx_1, d_wvelo_1, d_wx_gauge_1, d_tt_2, d_wx_2, d_wvelo_2, d_wx_gauge_2);
        //        dataStrList.AddRange(tmpDataStrList);

        //        d_wx_1 = cfprocess.GetOneChannelDataInRange(cit_1, cdlist.GetChannelIdByName("R_Align_SC", "右轨向_中波"), startPos_0, endPos_0);
        //        d_wx_2 = cfprocess.GetOneChannelDataInRange(cit_2, cdlist2.GetChannelIdByName("R_Align_SC", "右轨向_中波"), startPos_1, endPos_1);

        //        tmpDataStrList = changeDetcPro.ChangeDetectionPrcs("右轨向_中波", d_tt_1, d_wx_1, d_wvelo_1, d_wx_gauge_1, d_tt_2, d_wx_2, d_wvelo_2, d_wx_gauge_2);
        //        dataStrList.AddRange(tmpDataStrList);

        //    return dataStrList;
        //}
        

    }
}
