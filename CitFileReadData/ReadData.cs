using CitFileSDK;
using CitIndexFileSDK;
using CitIndexFileSDK.MileageFix;
using CommonFileSDK;
using GeoFileProcess;
using IntegratedDisplayCommon.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;


/// -------------------------------------------------------------------------------------------
/// FileName：ReadData.cs
/// 说    明：17  数据读模块(数据源可能是CIT, 也可能是别的文件类型，如txt, bny等) 目前只考虑CIT一种情况
/// Version ：2.0
/// Date    ：2017/8/29
/// Author  ：Qinh
/// -------------------------------------------------------------------------------------------

namespace CitFileReadData
{
    /// <summary>
    /// 文件读取
    /// </summary>
    public class ReadData
    {
        // CIT文件相关操作类
        CITFileProcess cfprocess = new CITFileProcess();
        // 通道定义相关操作类
        ChannelDefinitionList cdlist = new ChannelDefinitionList();
        // 里程操作类
        MilestoneList mslist = new MilestoneList();

        public MilestoneFix _mileageFix;

        public IOperator indexOperator { get; private set; }

        #region  ChannelDefinition 

        /// <summary>
        /// 获取通道集合
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(1)]
        public string channelDefinitionList(string json)
        {

            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                cdlist.channelDefinitionList = cfprocess.GetChannelDefinitionList(citFile);

                string data = JsonConvert.SerializeObject(cdlist.channelDefinitionList);
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
        /// 根据通道名称查找通道号
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(2)]
        public string GetChannelIdByName(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                string channelNameEn = Convert.ToString(obj["channelNameEn"]);
                string channelNameCh = Convert.ToString(obj["channelNameCh"]);

                cdlist.channelDefinitionList = cfprocess.GetChannelDefinitionList(citFile);
                int channelid=cdlist.GetChannelIdByName(channelNameEn, channelNameCh);
                
                resultInfo.flag = 1;
                resultInfo.msg = "";
                resultInfo.data = channelid.ToString();
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(resultInfo);
        }

        /// <summary>
        /// 根据通道号查询通道英文名称
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(3)]
        public string GetChannelEnNameById(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                int channelId = Convert.ToInt32(obj["channelId"].ToString());

                cdlist.channelDefinitionList = cfprocess.GetChannelDefinitionList(citFile);
                string channelName = cdlist.GetChannelEnNameById(channelId);

                resultInfo.flag = 1;
                resultInfo.msg = "";
                resultInfo.data = channelName;
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(resultInfo);
        }


        /// <summary>
        /// 根据通道号查询通道中文名称
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(4)]
        public string GetChannelChNameById(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                int channelId = Convert.ToInt32(obj["channelId"].ToString());

                cdlist.channelDefinitionList = cfprocess.GetChannelDefinitionList(citFile);
                string channelName = cdlist.GetChannelChNameById(channelId);

                resultInfo.flag = 1;
                resultInfo.msg = "";
                resultInfo.data = channelName;
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(resultInfo);
        }

        /// <summary>
        /// 根据通道号查询通道比例
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(5)]
        public string GetChannleScale(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                int channelId = Convert.ToInt32(obj["channelId"].ToString());

                cdlist.channelDefinitionList = cfprocess.GetChannelDefinitionList(citFile);
                float channelScale = cdlist.GetChannleScale(channelId);

                resultInfo.flag = 1;
                resultInfo.msg = "";
                resultInfo.data = channelScale.ToString();
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(resultInfo);
        }


        /// <summary>
        /// 根据通道名称获取通道比例
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(6)]
        public string GetChannelScaleByName(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                string channelNameEn = Convert.ToString(obj["channelNameEn"]);
                string channelNameCh = Convert.ToString(obj["channelNameCh"]);

                cdlist.channelDefinitionList = cfprocess.GetChannelDefinitionList(citFile);
                float channelScale = cdlist.GetChannelScale(channelNameEn, channelNameCh);

                resultInfo.flag = 1;
                resultInfo.msg = "";
                resultInfo.data = channelScale.ToString();
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(resultInfo);
        }


        /// <summary>
        /// 根据通道号获取通道基准线
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(7)]
        public string GetChannelOffset(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                int channelId = Convert.ToInt32(obj["channelId"].ToString());

                cdlist.channelDefinitionList = cfprocess.GetChannelDefinitionList(citFile);
                float channelOffset = cdlist.GetChannelOffset(channelId);

                resultInfo.flag = 1;
                resultInfo.msg = "";
                resultInfo.data = channelOffset.ToString();
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(resultInfo);
        }


        /// <summary>
        /// 根据通道名称获取通道基准线
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(8)]
        public string GetChannelOffsetByName(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                string channelNameEn = Convert.ToString(obj["channelNameEn"]);
                string channelNameCh = Convert.ToString(obj["channelNameCh"]);

                cdlist.channelDefinitionList = cfprocess.GetChannelDefinitionList(citFile);
                float channelOffset = cdlist.GetChannelOffset(channelNameEn, channelNameCh);

                resultInfo.flag = 1;
                resultInfo.msg = "";
                resultInfo.data = channelOffset.ToString();
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(resultInfo);
        }

        /// <summary>
        /// 根据通道号获取通道单位
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(9)]
        public string GetChannelUnit(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                int channelId = Convert.ToInt32(obj["channelId"].ToString());

                cdlist.channelDefinitionList = cfprocess.GetChannelDefinitionList(citFile);
                string channelUnit = cdlist.GetChannelUnit(channelId);

                resultInfo.flag = 1;
                resultInfo.msg = "";
                resultInfo.data = channelUnit;
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(resultInfo);
        }

        /// <summary>
        /// 根据通道名称获取通道单位
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(10)]
        public string GetChannelUnitByName(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                string channelNameEn = Convert.ToString(obj["channelNameEn"]);
                string channelNameCh = Convert.ToString(obj["channelNameCh"]);

                cdlist.channelDefinitionList = cfprocess.GetChannelDefinitionList(citFile);
                string channelUnit = cdlist.GetChannelUnit(channelNameEn, channelNameCh);

                resultInfo.flag = 1;
                resultInfo.msg = "";
                resultInfo.data = channelUnit;
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(resultInfo);
        }

        #endregion

        #region  附加信息 

        /// <summary>
        /// 获取文件头补充信息
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(11)]
        public string GetExtraInfo(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);

                byte[] bytes= cfprocess.GetExtraInfo(citFile);
                string base64 = Convert.ToBase64String(bytes);
                resultInfo.flag = 1;
                resultInfo.msg = "";
                resultInfo.data = base64;
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(resultInfo);
        }

        #endregion

        #region 获取通道数据

        #region 里程信息

        /// <summary>
        /// 得到文件中的所有里程信息
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(12)]
        public string GetAllMileStone(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                string idfFile = Convert.ToString(obj["idfFile"]);
                string dbFile = Convert.ToString(obj["dbFile"]);

                string data = "";

                List<Milestone> listMilestone = new List<Milestone>();
                listMilestone = cfprocess.GetAllMileStone(citFile);

                if (!String.IsNullOrEmpty(idfFile) && !String.IsNullOrEmpty(dbFile))
                {
                    indexOperator = new IndexOperator();
                    indexOperator.IndexFilePath = idfFile;

                    InnerFileOperator.InnerFilePath = dbFile;
                    InnerFileOperator.InnerConnString = "Provider = Microsoft.Jet.OLEDB.4.0; Data Source = {0}; Persist Security Info = True; Mode = Share Exclusive; Jet OLEDB:Database Password = iicdc; ";

                    _mileageFix = new MilestoneFix(citFile, indexOperator);

                    _mileageFix.ReadMilestoneFixTable();

                    listMilestone = _mileageFix.GetMileageReviseData(listMilestone);
                }

                data = JsonConvert.SerializeObject(listMilestone);

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
        /// 得到文件中的指定范文的里程信息
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(13)]
        public string GetMileStoneByRangeByStartFilePosEndFilePos(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                string idfFile = Convert.ToString(obj["idfFile"]);
                string dbFile = Convert.ToString(obj["dbFile"]);

                string data = "";

                long startFilePos = Convert.ToInt64(obj["startFilePos"].ToString());
                long endFilePos = Convert.ToInt64(obj["endFilePos"].ToString());

                List<Milestone> listMilestone = new List<Milestone>();
                listMilestone = cfprocess.GetMileStoneByRange(citFile, startFilePos, endFilePos);

                if (!String.IsNullOrEmpty(idfFile) && !String.IsNullOrEmpty(dbFile))
                {
                    indexOperator = new IndexOperator();
                    indexOperator.IndexFilePath = idfFile;

                    InnerFileOperator.InnerFilePath = dbFile;
                    InnerFileOperator.InnerConnString = "Provider = Microsoft.Jet.OLEDB.4.0; Data Source = {0}; Persist Security Info = True; Mode = Share Exclusive; Jet OLEDB:Database Password = iicdc; ";

                    _mileageFix = new MilestoneFix(citFile, indexOperator);

                    _mileageFix.ReadMilestoneFixTable();

                    listMilestone = _mileageFix.GetMileageReviseData(listMilestone);
                }

                data = JsonConvert.SerializeObject(listMilestone);

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
        /// 得到文件中的指定范围的里程信息，包含参数：cit文件路径、计算完偏移后的开始位置、采样点个数
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(14)]
        public string GetMileStoneByRangeByStartFilePosSampleNum(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                string idfFile = Convert.ToString(obj["idfFile"]);
                string dbFile = Convert.ToString(obj["dbFile"]);

                string data = "";

                long startFilePos = Convert.ToInt64(obj["startFilePos"].ToString());
                int sampleNum = Convert.ToInt32(obj["sampleNum"].ToString());

                List<Milestone> listMilestone = new List<Milestone>();
                long endFilePos = 0;
                listMilestone = cfprocess.GetMileStoneByRange(citFile, startFilePos, sampleNum,ref endFilePos);

                if (!String.IsNullOrEmpty(idfFile) && !String.IsNullOrEmpty(dbFile))
                {
                    indexOperator = new IndexOperator();
                    indexOperator.IndexFilePath = idfFile;

                    InnerFileOperator.InnerFilePath = dbFile;
                    InnerFileOperator.InnerConnString = "Provider = Microsoft.Jet.OLEDB.4.0; Data Source = {0}; Persist Security Info = True; Mode = Share Exclusive; Jet OLEDB:Database Password = iicdc; ";

                    _mileageFix = new MilestoneFix(citFile, indexOperator);

                    _mileageFix.ReadMilestoneFixTable();

                    listMilestone = _mileageFix.GetMileageReviseData(listMilestone);
                }

                data = JsonConvert.SerializeObject(listMilestone);

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
        /// 找到第一个采样点，读取其里程信息
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(15)]
        public string GetStartMilestone(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                string idfFile = Convert.ToString(obj["idfFile"]);
                string dbFile = Convert.ToString(obj["dbFile"]);

                string data = "";

                Milestone Milestone = new Milestone();
                Milestone = cfprocess.GetStartMilestone(citFile);

                if (!String.IsNullOrEmpty(idfFile) && !String.IsNullOrEmpty(dbFile))
                {
                    indexOperator = new IndexOperator();
                    indexOperator.IndexFilePath = idfFile;

                    InnerFileOperator.InnerFilePath = dbFile;
                    InnerFileOperator.InnerConnString = "Provider = Microsoft.Jet.OLEDB.4.0; Data Source = {0}; Persist Security Info = True; Mode = Share Exclusive; Jet OLEDB:Database Password = iicdc; ";

                    _mileageFix = new MilestoneFix(citFile, indexOperator);

                    _mileageFix.ReadMilestoneFixTable();

                    _mileageFix.RunFixingAlgorithm();

                    Milestone = _mileageFix.CalcMilestoneByFixedMilestone(Milestone.mKm * 1000 + Milestone.mMeter);
                }

                data = JsonConvert.SerializeObject(Milestone);

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
        /// 找到最后一个采样点，读取其里程信息
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(16)]
        public string GetEndMilestone(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                string idfFile = Convert.ToString(obj["idfFile"]);
                string dbFile = Convert.ToString(obj["dbFile"]);

                string data = "";

                Milestone Milestone = new Milestone();
                Milestone = cfprocess.GetEndMilestone(citFile);

                if (!String.IsNullOrEmpty(idfFile) && !String.IsNullOrEmpty(dbFile))
                {
                    indexOperator = new IndexOperator();
                    indexOperator.IndexFilePath = idfFile;

                    InnerFileOperator.InnerFilePath = dbFile;
                    InnerFileOperator.InnerConnString = "Provider = Microsoft.Jet.OLEDB.4.0; Data Source = {0}; Persist Security Info = True; Mode = Share Exclusive; Jet OLEDB:Database Password = iicdc; ";

                    _mileageFix = new MilestoneFix(citFile, indexOperator);

                    _mileageFix.ReadMilestoneFixTable();

                    _mileageFix.RunFixingAlgorithm();

                    Milestone = _mileageFix.CalcMilestoneByFixedMilestone(Milestone.mKm * 1000 + Milestone.mMeter);
                }

                data = JsonConvert.SerializeObject(Milestone);

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
        /// 根据文件指针获取对应的里程标
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(17)]
        public string GetAppointMilestone(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]); 
                long filePos = Convert.ToInt64(obj["filePos"].ToString());

                Milestone Milestone = new Milestone();
                Milestone = cfprocess.GetAppointMilestone(citFile, filePos);
                string data = JsonConvert.SerializeObject(Milestone);

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


        #endregion


        #region 里程相关操作

        /// <summary>
        /// 获取指定里程的位置
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(18)]
        public string GetMilestoneFilePosition(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                float mileStone = Convert.ToSingle(obj["mileStone"].ToString());
                mslist.milestoneList = JsonConvert.DeserializeObject<List<Milestone>>(obj["milestoneList"].ToString());

                long position = 0;
                position= mslist.GetMilestoneFilePosition(mileStone);

                resultInfo.flag = 1;
                resultInfo.msg = "";
                resultInfo.data = position.ToString();
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(resultInfo);
        }

        /// <summary>
        /// 获取指定范围的里程信息
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(19)]
        public string GetMilestoneRange(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                float mileStone = Convert.ToSingle(obj["startms"].ToString());
                float endms = Convert.ToSingle(obj["endms"].ToString());
                mslist.milestoneList = JsonConvert.DeserializeObject<List<Milestone>>(obj["milestoneList"].ToString());

                List<Milestone> listMilestoneNew = new List<Milestone>();
                listMilestoneNew = mslist.GetMilestoneRange(mileStone,endms);
                string data=JsonConvert.SerializeObject(listMilestoneNew);

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
        /// 获取里程的开始里程
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(20)]
        public string GetStart(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                mslist.milestoneList = JsonConvert.DeserializeObject<List<Milestone>>(obj["milestoneList"].ToString());

                float data = 0;
                data = mslist.GetStart();

                resultInfo.flag = 1;
                resultInfo.msg = "";
                resultInfo.data = data.ToString();
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(resultInfo);
        }

        /// <summary>
        /// 获取里程的结束里程
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(21)]
        public string GetEnd(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                mslist.milestoneList = JsonConvert.DeserializeObject<List<Milestone>>(obj["milestoneList"].ToString());

                float data = 0;
                data = mslist.GetEnd();

                resultInfo.flag = 1;
                resultInfo.msg = "";
                resultInfo.data = data.ToString();
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(resultInfo);
        }

        #endregion


        #region 获取指定通道的通道数据

        /// <summary>
        /// 获取指定通道的通道数据
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(22)]
        public string GetOneChannelDataInRangeByStartFilePosEndFilePos(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                int channelId = Convert.ToInt32(obj["channelId"].ToString());
                long startFilePos = Convert.ToInt64(obj["startFilePos"].ToString());
                long endFilePos = Convert.ToInt64(obj["endFilePos"].ToString());
                
                double[] fReturnArray = cfprocess.GetOneChannelDataInRange(citFile, channelId, startFilePos, endFilePos);
                string data = JsonConvert.SerializeObject(fReturnArray);

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
        /// 获取指定通道的通道数据(从开始位置开始，获取指定个数的采样点) 包括参数：cit文件路径、通道号、开始位置、采样点个数
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(23)]
        public string GetOneChannelDataInRangeByStartFilePosSampleNum(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                int channelId = Convert.ToInt32(obj["channelId"].ToString());
                long startFilePos = Convert.ToInt64(obj["startFilePos"].ToString());
                int sampleNum = Convert.ToInt32(obj["sampleNum"].ToString());

                double[] fReturnArray = cfprocess.GetOneChannelDataInRange(citFile, channelId, startFilePos, sampleNum);
                string data = JsonConvert.SerializeObject(fReturnArray);

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
        /// 获取指定通道的通道数据(从开始位置开始，获取指定个数的采样点)
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(24)]
        public string GetAppointChannelDataInRange(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                int channelId = Convert.ToInt32(obj["channelId"].ToString());
                long startFilePos = Convert.ToInt64(obj["startFilePos"].ToString());
                int sampleNum = Convert.ToInt32(obj["sampleNum"].ToString()); 
                long endFilePos =0;

                double[] fReturnArray = cfprocess.GetAppointChannelDataInRange(citFile, channelId, startFilePos, sampleNum,ref endFilePos);
                string data = JsonConvert.SerializeObject(fReturnArray);

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
        /// 根据开始里程和采样点个数获取指定通道的通道数据
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(25)]
        public string GetAppointChannelDataInRangeByMilestone(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                int channelId = Convert.ToInt32(obj["channelId"].ToString());
                float startMilestone = Convert.ToSingle(obj["startMilestone"].ToString());
                float endMilestone = Convert.ToSingle(obj["endMilestone"].ToString());

                double[] fReturnArray = cfprocess.GetAppointChannelDataInRangeByMilestone(citFile, channelId, startMilestone, endMilestone);
                string data = JsonConvert.SerializeObject(fReturnArray);

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
        /// 根据开始里程和采样点个数获取指定通道的通道数据 包括参数：cit文件路径、通道号、开始里程、采样点个数
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(26)]
        public string GetAppointChannelDataInRangeByMilestoneByStartMilestoneSampleNum(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                int channelId = Convert.ToInt32(obj["channelId"].ToString());
                float startMilestone = Convert.ToSingle(obj["startMilestone"].ToString());
                int sampleNum = Convert.ToInt32(obj["sampleNum"].ToString());

                double[] fReturnArray = cfprocess.GetAppointChannelDataInRangeByMilestone(citFile, channelId, startMilestone, sampleNum);
                string data = JsonConvert.SerializeObject(fReturnArray);

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

        #endregion


        #region 获取所有通道的数据

        /// <summary>
        /// 获取所有通道的数据
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(27)]
        public string GetAllChannelDataInRangeByStartFilePosEndFilePos(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                long startFilePos = Convert.ToInt64(obj["startFilePos"].ToString());
                long endFilePos = Convert.ToInt64(obj["endFilePos"].ToString());

                List<double[]> allList = new List<double[]>();
                allList = cfprocess.GetAllChannelDataInRange(citFile, startFilePos, endFilePos);
                string data = JsonConvert.SerializeObject(allList);
                
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
        /// 获取所有通道数据    包括参数：cit文件路径、计算完偏移量后的开始位置、采样点个数
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(28)]
        public string GetAllChannelDataInRangeByStartFilePosSampleNum(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                long startFilePos = Convert.ToInt64(obj["startFilePos"].ToString());
                int sampleNum = Convert.ToInt32(obj["sampleNum"].ToString());

                List<double[]> allList = new List<double[]>();
                allList = cfprocess.GetAllChannelDataInRange(citFile, startFilePos, sampleNum);
                string data = JsonConvert.SerializeObject(allList);

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
        /// 获取所有通道数据 包括采样点结束位置
        /// </summary>
        /// <param name="json"></param> 
        /// <returns></returns>
        [DispId(29)]
        public string GetAllChannelDataInRangeByStartFilePosSampleNum2(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                long startFilePos = Convert.ToInt64(obj["startFilePos"].ToString());
                int sampleNum = Convert.ToInt32(obj["sampleNum"].ToString());

                long endFilePos = 0;

                List<double[]> allList = new List<double[]>();
                allList = cfprocess.GetAllChannelDataInRange(citFile, startFilePos, sampleNum,ref endFilePos);
                string data = JsonConvert.SerializeObject(allList);

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
        /// 根据开始里程和采样点个数获取所有通道数据
        /// </summary>
        /// <param name="json"></param> 
        /// <returns></returns>
        [DispId(30)]
        public string GetAllChannelDataInRangeByStartMilestoneSampleNum(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                float startMilestone = Convert.ToSingle(obj["startMilestone"].ToString());
                int sampleNum = Convert.ToInt32(obj["sampleNum"].ToString());

                List<double[]> allList = new List<double[]>();
                allList = cfprocess.GetAllChannelDataInRange(citFile, startMilestone, sampleNum);
                string data = JsonConvert.SerializeObject(allList);

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
        /// 获取指定里程范围内的全部通道数据
        /// </summary>
        /// <param name="json"></param> 
        /// <returns></returns>
        [DispId(31)]
        public string GetAllChannelDataInRangeByMilestone(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                float startMilestone = Convert.ToSingle(obj["startMilestone"].ToString());
                int endMilestone = Convert.ToInt32(obj["endMilestone"].ToString());

                List<double[]> allList = new List<double[]>();
                allList = cfprocess.GetAllChannelDataInRangeByMilestone(citFile, startMilestone, endMilestone);
                string data = JsonConvert.SerializeObject(allList);

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

        #endregion


        #region 获取通道的字节数组

        /// <summary>
        /// 根据开始位置、结束位置获取所有通道的字节数组
        /// </summary>
        /// <param name="json"></param> 
        /// <returns></returns>
        [DispId(32)]
        public string GetChannelDataBytesInRangeByStartFilePosEndFilePos(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                long startFilePos = Convert.ToInt64(obj["startFilePos"].ToString());
                long endFilePos = Convert.ToInt64(obj["endFilePos"].ToString());


                byte[] bytes = cfprocess.GetChannelDataBytesInRange(citFile, startFilePos, endFilePos);
                string data = Convert.ToBase64String(bytes);

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
        /// 根据开始位置以及采样点个数获取所有通道的字节数组
        /// </summary>
        /// <param name="json"></param> 
        /// <returns></returns>
        [DispId(33)]
        public string GetChannelDataBytesInRangeByStartFilePosSampleNum(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                long startFilePos = Convert.ToInt64(obj["startFilePos"].ToString());
                int sampleNum = Convert.ToInt32(obj["sampleNum"].ToString());


                byte[] bytes = cfprocess.GetChannelDataBytesInRange(citFile, startFilePos, sampleNum);
                string data = Convert.ToBase64String(bytes);

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
        /// 根据开始里程和采样点数获取所有通道的字节数组
        /// </summary>
        /// <param name="json"></param> 
        /// <returns></returns>
        [DispId(34)]
        public string GetChannelDataBytesInRangeByStartMilestoneSampleNum(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                float startMilestone = Convert.ToSingle(obj["startMilestone"].ToString());
                int sampleNum = Convert.ToInt32(obj["sampleNum"].ToString());


                byte[] bytes = cfprocess.GetChannelDataBytesInRange(citFile, startMilestone, sampleNum);
                string data = Convert.ToBase64String(bytes);

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
        /// 根据开始里程、结束里程获取所有通道的字节数组
        /// </summary>
        /// <param name="json"></param> 
        /// <returns></returns>
        [DispId(35)]
        public string GetChannelDataBytesInRangeByMilestone(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                float startMilestone = Convert.ToSingle(obj["startMilestone"].ToString());
                int endMilestone = Convert.ToInt32(obj["endMilestone"].ToString());


                byte[] bytes = cfprocess.GetChannelDataBytesInRangeByMilestone(citFile, startMilestone, endMilestone);
                string data = Convert.ToBase64String(bytes);

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

        #endregion

        #endregion

        /// <summary>
        /// 获取cit文件的数据块的开始位置、结束位置
        /// </summary>
        /// <param name="json"></param> 
        /// <returns></returns>
        [DispId(36)]
        public string GetPositons(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);


                long[] positions = cfprocess.GetPositons(citFile);
                string data = JsonConvert.SerializeObject(positions);

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
        /// 获取cit文件当前里程的位置
        /// </summary>
        /// <param name="json"></param> 
        /// <returns></returns>
        [DispId(37)]
        public string GetCurrentPositionByMilestone(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                float mile = Convert.ToSingle(obj["mile"].ToString());
                bool isStrict = Convert.ToBoolean(obj["isStrict"].ToString());

                long position = 0;
                position = cfprocess.GetCurrentPositionByMilestone(citFile, mile, isStrict);

                resultInfo.flag = 1;
                resultInfo.msg = "";
                resultInfo.data = position.ToString();
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(resultInfo);
        }

        /// <summary>
        /// 获取指定采样点后的结束位置，如果溢出会返回溢出后的位置
        /// </summary>
        /// <param name="json"></param> 
        /// <returns></returns>
        [DispId(38)]
        public string GetAppointEndPostion(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                long startFilePos = Convert.ToInt64(obj["startFilePos"].ToString());
                int sampleNum = Convert.ToInt32(obj["sampleNum"].ToString());

                long position = 0;
                position = cfprocess.GetAppointEndPostion(citFile, startFilePos, sampleNum);

                resultInfo.flag = 1;
                resultInfo.msg = "";
                resultInfo.data = position.ToString();
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(resultInfo);
        }

        /// <summary>
        /// 获取文件中指定采样后的结束位置，如果溢出，不会超过文件范围
        /// </summary>
        /// <param name="json"></param> 
        /// <returns></returns>
        [DispId(39)]
        public string GetAppointFileEndPostion(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                long startFilePos = Convert.ToInt64(obj["startFilePos"].ToString());
                int sampleNum = Convert.ToInt32(obj["sampleNum"].ToString());

                long position = 0;
                position = cfprocess.GetAppointFileEndPostion(citFile, startFilePos, sampleNum);

                resultInfo.flag = 1;
                resultInfo.msg = "";
                resultInfo.data = position.ToString();
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(resultInfo);
        }


        /// <summary>
        /// 获取所有采样点的个数
        /// </summary>
        /// <param name="json"></param> 
        /// <returns></returns>
        [DispId(40)]
        public string GetTotalSampleCount(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);

                long position = 0;
                position = cfprocess.GetTotalSampleCount(citFile);

                resultInfo.flag = 1;
                resultInfo.msg = "";
                resultInfo.data = position.ToString();
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(resultInfo);
        }


        /// <summary>
        /// 获取固定里程范文内所有采样点的个数
        /// </summary>
        /// <param name="json"></param> 
        /// <returns></returns>
        [DispId(41)]
        public string GetSampleCountByRange(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                long startPostion = Convert.ToInt64(obj["startPostion"].ToString());
                long endPostion = Convert.ToInt64(obj["endPostion"].ToString());

                long position = 0;
                position = cfprocess.GetSampleCountByRange(citFile, startPostion, endPostion);

                resultInfo.flag = 1;
                resultInfo.msg = "";
                resultInfo.data = position.ToString();
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(resultInfo);
        }


        /// <summary>
        /// 获取指定采样点个数
        /// </summary>
        /// <param name="json"></param> 
        /// <returns></returns>
        [DispId(42)]
        public string GetAppointSampleCount(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                long endPostion = Convert.ToInt64(obj["endPostion"].ToString());

                long position = 0;
                position = cfprocess.GetAppointSampleCount(citFile, endPostion);

                resultInfo.flag = 1;
                resultInfo.msg = "";
                resultInfo.data = position.ToString();
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(resultInfo);
        }

        #region 测试调用结果
        /// <summary>
        /// 链接测试使用
        /// </summary>
        /// <returns></returns>
        [DispId(43)]
        public string testConnect()
        {
            return "ConnectSuccess";
        }
        #endregion

        /// <summary>
        /// 根据cit获取文件头部信息
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(44)]
        public string GetFileInformation(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);

                FileInformation fi= cfprocess.GetFileInformation(citFile);
                string data= JsonConvert.SerializeObject(fi);

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
             


    }
}
