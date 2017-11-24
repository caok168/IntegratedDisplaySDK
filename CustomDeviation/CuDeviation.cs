using CitFileSDK;
using CommonFileSDK;
using DataProcess;
using IntegratedDisplayCommon.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace CustomDeviation
{
    public class CuDeviation
    {

        #region 字典--存放项目类型的中文英文对照
        /// <summary>
        /// 字典--存放项目类型的中文英文对照
        /// </summary>
        public Dictionary<String, String> dicExcptnType = new Dictionary<String, String>();
        #endregion
        #region 字典--存放通道名称的中文英文对照
        /// <summary>
        /// 字典--存放通道名称的中文英文对照
        /// </summary>
        public Dictionary<String, String> dicChannelEnToCh = new Dictionary<String, String>();
        #endregion
        #region 字典--存放自定义偏差写入iic时的通道和偏差英文名的映射
        /// <summary>
        /// 字典--存放自定义偏差写入iic时的通道和偏差英文名的映射
        /// </summary>
        public Dictionary<String, String> dicChannelChToEn = new Dictionary<String, String>();
        #endregion

        #region 列表--自定义超限显示
        /// <summary>
        /// 列表--自定义超限显示
        /// </summary>
        List<ExceptionValueDIYClass> excptnValClsList = new List<ExceptionValueDIYClass>();
        #endregion

        #region 列表--超限标准值和自定义值
        /// <summary>
        /// 列表--超限标准值和自定义值
        /// </summary>
        List<ExceptionStndAndDiyClass> excptnStnAndDiyClsList = new List<ExceptionStndAndDiyClass>();
        #endregion

        #region 列表--存放cit文件所属线路的速度区段信息
        /// <summary>
        /// 列表--存放cit文件所属线路的速度区段信息
        /// </summary>
        public List<SudujiClass> sdjClsList = new List<SudujiClass>();
        #endregion

        #region checkbox列表
        public Dictionary<String, String> dicValid = new Dictionary<String, String>();
        public List<ValidExcptnClass> validexcptnclasslist = new List<ValidExcptnClass>();
        public List<ValidExcptnType> validexcptntypelist = new List<ValidExcptnType>();
        #endregion

        #region 线路编号
        /// <summary>
        /// 线路编号
        /// </summary>
        public String lineCode;
        #endregion
        #region 行别
        /// <summary>
        /// 行别
        /// </summary>
        String sDir = null;//行别
        #endregion

        //读取头部信息
        CITFileProcess cit = new CITFileProcess();

        // 通道定义相关操作类
        ChannelDefinitionList cdlist = new ChannelDefinitionList();

        //获取文件信息
        FileInformation fileforma=new FileInformation();

        /// <summary>
        /// 波形偏差分析可执行模块
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(171)]
        public string ExcptnValCls(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);

                string citFile = Convert.ToString(obj["citFile"]);
                //类型 部标 0 ； 局标 1
                string standardType = Convert.ToString(obj["standardType"].ToString());

                string innerdbpath = Convert.ToString(obj["innerdbpath"]);

                //获取文件信息
                fileforma = cit.GetFileInformation(citFile);

                //获取通道数据
                cdlist.channelDefinitionList = cit.GetChannelDefinitionList(citFile);

                //初始化要比对的项,后续需要考虑使用参数的形式接收,如果采用参数接收则需要将下列两个方法屏蔽
                //InitCheckBoxList_ExcptnClass();
                //InitCheckBoxList_ExcptnType();
                validexcptnclasslist = JsonConvert.DeserializeObject<List<ValidExcptnClass>>(obj["excptnclass"].ToString());
                validexcptntypelist = JsonConvert.DeserializeObject<List<ValidExcptnType>>(obj["excptntype"].ToString());


                //初始化字典
                InitDicValid();
                InitDicChannelEnToCh();
                InitDicChannelChToEn();
                InitDicExcptnType();

                //获取速度集
                InitSdjClsList(innerdbpath, fileforma.sTrackCode, fileforma.iDir);//lineCode 线路编号   sDir  行别 

                //初始化超限标准值和自定义值
                InitExcptnStnAndDiyClsList(innerdbpath, standardType);

                if (excptnStnAndDiyClsList.Count == 0)
                {
                    throw new Exception("获取超限标准值和自定义值为空");
                }

                List<ExceptionStndAndDiyClass> newList = DisplayExcptnStnAndDiyClsList(excptnStnAndDiyClsList);

                foreach (ExceptionStndAndDiyClass tmpCls in newList)
                {
                    excptnValClsList.AddRange(GetExcptnValByType(citFile, tmpCls));
                }

                ReSortExcptnValClsListById();

                string data = JsonConvert.SerializeObject(excptnValClsList);
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
        /// 导出数据
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(172)]
        public string ExportExcel(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);

                string citFile = Convert.ToString(obj["citFile"]);
                //类型 部标 0 ； 局标 1
                string standardType = Convert.ToString(obj["standardType"].ToString());

                string innerdbpath = Convert.ToString(obj["innerdbpath"]);

                //获取文件信息
                fileforma = cit.GetFileInformation(citFile);

                //获取通道数据
                cdlist.channelDefinitionList = cit.GetChannelDefinitionList(citFile);

                //初始化要比对的项,后续需要考虑使用参数的形式接收,如果采用参数接收则需要将下列两个方法屏蔽
                //InitCheckBoxList_ExcptnClass();
                //InitCheckBoxList_ExcptnType();
                validexcptnclasslist = JsonConvert.DeserializeObject<List<ValidExcptnClass>>(obj["excptnclass"].ToString());
                validexcptntypelist = JsonConvert.DeserializeObject<List<ValidExcptnType>>(obj["excptntype"].ToString());

                //初始化字典
                InitDicValid();
                InitDicChannelEnToCh();
                InitDicChannelChToEn();
                InitDicExcptnType();

                //获取速度集
                InitSdjClsList(innerdbpath, fileforma.sTrackCode, fileforma.iDir);//lineCode 线路编号   sDir  行别 

                //初始化超限标准值和自定义值
                InitExcptnStnAndDiyClsList(innerdbpath, standardType);

                if (excptnStnAndDiyClsList.Count == 0)
                {
                    throw new Exception("获取超限标准值和自定义值为空");
                }

                List<ExceptionStndAndDiyClass> newList = DisplayExcptnStnAndDiyClsList(excptnStnAndDiyClsList);

                foreach (ExceptionStndAndDiyClass tmpCls in newList)
                {
                    excptnValClsList.AddRange(GetExcptnValByType(citFile, tmpCls));
                }

                ReSortExcptnValClsListById();

                string data=_exportCsvExcptnValClsList(citFile);

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
        /// 导出IIC
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(173)]
        public string ExportIIC(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);

                string citFile = Convert.ToString(obj["citFile"]);
                //类型 部标 0 ； 局标 1
                string standardType = Convert.ToString(obj["standardType"].ToString());

                string innerdbpath = Convert.ToString(obj["innerdbpath"]);

                string iicFilePath = Convert.ToString(obj["iicFilePath"]);

                string idfFile = Convert.ToString(obj["idfFile"]);

                //获取文件信息
                fileforma = cit.GetFileInformation(citFile);

                //获取通道数据
                cdlist.channelDefinitionList = cit.GetChannelDefinitionList(citFile);

                //初始化要比对的项,后续需要考虑使用参数的形式接收,如果采用参数接收则需要将下列两个方法屏蔽
                //InitCheckBoxList_ExcptnClass();
                //InitCheckBoxList_ExcptnType();
                validexcptnclasslist = JsonConvert.DeserializeObject<List<ValidExcptnClass>>(obj["excptnclass"].ToString());
                validexcptntypelist = JsonConvert.DeserializeObject<List<ValidExcptnType>>(obj["excptntype"].ToString());

                //初始化字典
                InitDicValid();
                InitDicChannelEnToCh();
                InitDicChannelChToEn();
                InitDicExcptnType();

                //获取速度集
                InitSdjClsList(innerdbpath, fileforma.sTrackCode, fileforma.iDir);//lineCode 线路编号   sDir  行别 

                //初始化超限标准值和自定义值
                InitExcptnStnAndDiyClsList(innerdbpath, standardType);

                if (excptnStnAndDiyClsList.Count == 0)
                {
                    throw new Exception("获取超限标准值和自定义值为空");
                }

                List<ExceptionStndAndDiyClass> newList = DisplayExcptnStnAndDiyClsList(excptnStnAndDiyClsList);

                foreach (ExceptionStndAndDiyClass tmpCls in newList)
                {
                    excptnValClsList.AddRange(GetExcptnValByType(citFile, tmpCls));
                }

                ReSortExcptnValClsListById();

                string data = _exportIIC(citFile, idfFile, innerdbpath, iicFilePath);

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
        /// 导出IIC文件
        /// </summary>
        /// <returns></returns>
        private string _exportIIC(string citFile,string idfFile, string innerdbpath, string iicFilePath)
        {
            long m_RecordNumber = 0;
            long m_defectnum = 0;

            try
            {
                using (OleDbConnection sqlconn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + iicFilePath + ";Persist Security Info=True"))
                {
                    String sSQL = "select max(RecordNumber),max(defectnum) from defects where valid<>'N'";
                    OleDbCommand sqlcom = new OleDbCommand(sSQL, sqlconn);

                    sqlconn.Open();
                    OleDbDataReader sdr = sqlcom.ExecuteReader();

                    while (sdr.Read())
                    {
                        m_RecordNumber = long.Parse(sdr.GetValue(0).ToString());
                        m_defectnum = long.Parse(sdr.GetValue(1).ToString());
                    }
                    sdr.Close();
                    sqlconn.Close();
                }
            }
            catch (System.Exception ex)
            {
                throw new Exception("查询defects表中的最大值出错：" + "\n" + ex.Source);
            }

            //查询线路代码，日期和时间
            String trackCode = null;
            String runTime = null;
            String runDate = null;
            try
            {
                using (OleDbConnection sqlconn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + iicFilePath + ";Persist Security Info=True"))
                {
                    sqlconn.Open();

                    String sSQL = "select distinct SubCode from defects";
                    OleDbCommand sqlcom = new OleDbCommand(sSQL, sqlconn);
                    OleDbDataReader sdr = sqlcom.ExecuteReader();
                    while (sdr.Read())
                    {
                        trackCode = sdr.GetValue(0).ToString();
                    }
                    sdr.Close();

                    sSQL = "select distinct RunDate from defects";
                    sqlcom = new OleDbCommand(sSQL, sqlconn);
                    sdr = sqlcom.ExecuteReader();
                    while (sdr.Read())
                    {
                        runDate = DateTime.Parse(sdr.GetValue(0).ToString()).Date.ToShortDateString();
                        runDate = runDate.Replace("/", "-");
                    }
                    sdr.Close();

                    sSQL = "select distinct RunTime from defects";
                    sqlcom = new OleDbCommand(sSQL, sqlconn);
                    sdr = sqlcom.ExecuteReader();
                    while (sdr.Read())
                    {
                        runTime = sdr.GetValue(0).ToString();
                    }
                    sdr.Close();

                    sqlconn.Close();
                }
            }
            catch (System.Exception ex)
            {
                throw new Exception("查询fix_defects表中的日期，时间出错：" + "\n" + ex.Source);
            }

            String str = fileforma.sTrackCode;
            try
            {
                using (OleDbConnection sqlconn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + iicFilePath + ";Persist Security Info=True"))
                {
                    sqlconn.Open();
                    int diyDefectClass = 0;
                    String sSQL = null;
                    OleDbCommand sqlcom = null;
                    String sqlFormat = "insert into defects(RecordNumber,SubCode,RunDate,RunTime,defectnum,defecttype,tbce,length,maxpost,maxminor,maxval1,maxval2,speedatmaxval,severity,postedspd,defectclass,valid,frompost,fromminor)"
                        + " values({0},'{1}','{2}','{3}',{4},'{5}','{6}',{7},{8},{9},{10},{11},{12},{13},{14},{15},'{16}',{17},{18})";
                    for (int i = 0; i < excptnValClsList.Count; i++)
                    {
                        diyDefectClass = GetNewDefectClass(excptnValClsList[i].excptnClass);

                        sSQL = string.Format(sqlFormat,
                            m_RecordNumber + i + 1,
                            trackCode,
                            runDate,
                            runTime,
                            m_defectnum + i + 1,
                            dicChannelChToEn[dicChannelEnToCh[excptnValClsList[i].exceptionType]],
                            "",
                            (int)(excptnValClsList[i].length),
                            (int)(excptnValClsList[i].milePos),
                            (int)(excptnValClsList[i].milePos * 1000 % 1000),
                            excptnValClsList[i].exceptionValue,
                            0,
                            excptnValClsList[i].speed,
                            1,
                            excptnValClsList[i].maxSpeed,
                            diyDefectClass,
                            excptnValClsList[i].valid,
                            (int)(excptnValClsList[i].milePos),
                            (int)(excptnValClsList[i].milePos * 1000 % 1000)
                            );
                        sqlcom = new OleDbCommand(sSQL, sqlconn);
                        sqlcom.ExecuteNonQuery();
                    }

                    sqlconn.Close();
                }

            }
            catch (System.Exception ex)
            {
                throw new Exception(ex.Message + "\n" + ex.Source);
            }

            //如果fix_defects表存在，拷贝自定义大值数据到fix_defects表
            //分两种情况：一种是iic已经修正，另一种是iic未修正。
            if (IsHasFixTable(iicFilePath))
            {
                Boolean m_isIICFixed = IsIICFixed(iicFilePath);

                //删除已经创建的表            
                try
                {
                    using (OleDbConnection sqlconn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + iicFilePath + ";Persist Security Info=True"))
                    {
                        string sqlCreate = "drop table fix_defects";
                        OleDbCommand sqlcom = new OleDbCommand(sqlCreate, sqlconn);
                        sqlconn.Open();
                        sqlcom.ExecuteNonQuery();
                        sqlconn.Close();
                    }
                }
                catch
                {
                    //直接使用原始的iic文件，里面没有fix_defects表，因此删除出错，但是不处理。
                    //throw new Exception("未找到修正后的iic文件");
                }

                try
                {
                    using (OleDbConnection sqlconn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + iicFilePath + ";Persist Security Info=True"))
                    {
                        sqlconn.Open();

                        //原来这里是拷贝所有记录
                        //考虑到很多超限值车上人员已经确认过是无效的，因此这里只拷贝有效的--20140114--和赵主任确认的结果
                        string sqlCreate = "select * into fix_defects from defects where valid<>'N'";
                        OleDbCommand sqlcom = new OleDbCommand(sqlCreate, sqlconn);
                        sqlcom.ExecuteNonQuery();

                        //段级系统要求要保留校正前的里程，因此把maxpost,maxminor拷贝到frompost,fromminor---20140225--严广学
                        sqlCreate = "update  fix_defects set frompost=maxpost";
                        sqlcom = new OleDbCommand(sqlCreate, sqlconn);
                        sqlcom.ExecuteNonQuery();
                        sqlCreate = "update  fix_defects set fromminor=maxminor";
                        sqlcom = new OleDbCommand(sqlCreate, sqlconn);
                        sqlcom.ExecuteNonQuery();

                        sqlconn.Close();
                    }

                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }

                
                if (m_isIICFixed)
                {   
                    //IIC中的里程已经修正
                    //CommonClass.wdp.ExceptionFix(CommonClass.listDIC[0].sFilePath, iicFilePath, CommonClass.listDIC[0].listIC,
    //CommonClass.listDIC[0].iSmaleRate, CommonClass.listDIC[0].iChannelNumber, CommonClass.listDIC[0].bEncrypt, CommonClass.listDIC[0].sKmInc, CommonClass.listETC);
                    
                    List<ExceptionType> listETC = new List<ExceptionType>();
                    
                    try
                    {
                        InnerFileOperator.InnerFilePath = innerdbpath;
                        InnerFileOperator.InnerConnString = "Provider = Microsoft.Jet.OLEDB.4.0; Data Source = {0}; Persist Security Info = True; Mode = Share Exclusive; Jet OLEDB:Database Password = iicdc; ";
                        using (OleDbConnection sqlconn = new OleDbConnection(InnerFileOperator.InnerConnString))
                        {
                            OleDbCommand sqlcom = new OleDbCommand("select EXCEPTIONEN,EXCEPTIONCN from Exceptiontype", sqlconn);
                            sqlconn.Open();
                            OleDbDataReader oldr = sqlcom.ExecuteReader();
                            while (oldr.Read())
                            {
                                ExceptionType etc = new ExceptionType();
                                etc.EXCEPTIONEN = oldr[0].ToString();
                                etc.EXCEPTIONCN = oldr[1].ToString();
                                listETC.Add(etc);
                            }
                            oldr.Close();
                            sqlconn.Close();
                        }
                    }
                    catch
                    {

                    }

                    List<IndexSta> listIC = new List<IndexSta>();

                    try
                    {
                        using (OleDbConnection sqlconn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + idfFile + ";Persist Security Info=False"))
                        {
                            OleDbCommand sqlcom = new OleDbCommand("select * from IndexSta order by id", sqlconn);
                            sqlconn.Open();
                            OleDbDataReader sqloledr = sqlcom.ExecuteReader();
                            while (sqloledr.Read())
                            {
                                IndexSta ic = new IndexSta();
                                ic.iID = (int)sqloledr.GetInt32(0);
                                ic.iIndexID = (int)sqloledr.GetInt32(1);
                                ic.lStartPoint = long.Parse(sqloledr.GetString(2));
                                ic.lStartMeter = sqloledr.GetString(3);
                                ic.lEndPoint = long.Parse(sqloledr.GetString(4));
                                ic.LEndMeter = sqloledr.GetString(5);
                                ic.lContainsPoint = long.Parse(sqloledr.GetString(6));
                                ic.lContainsMeter = sqloledr.GetString(7);
                                ic.sType = sqloledr.GetString(8);

                                listIC.Add(ic);
                            }
                            sqlconn.Close();
                        }

                    }
                    catch
                    {

                    }

                    ExceptionFix(citFile, iicFilePath, listIC, listETC);

                }
                
            }

            return iicFilePath;
        }

        #region 获取与行业标准相对应的铁路局标准的偏差等级
        /// <summary>
        /// 获取与行业标准相对应的铁路局标准的偏差等级
        /// </summary>
        /// <param name="oldClass">行业标准偏差等级</param>
        /// <returns>铁路局标准的偏差等级</returns>
        private int GetNewDefectClass(int standardClass)
        {
            int retVal = 0;

            if (standardClass == 1)
            {
                retVal = 21;
            }
            if (standardClass == 2)
            {
                retVal = 22;
            }
            if (standardClass == 3)
            {
                retVal = 23;
            }
            if (standardClass == 4)
            {
                retVal = 24;
            }

            return retVal;
        }
        #endregion

        #region 判断是否含有fix表
        /// <summary>
        /// 判断是否含有fix表
        /// </summary>
        /// <param name="mIICFilePath"></param>
        /// <returns></returns>
        private Boolean IsHasFixTable(String mIICFilePath)
        {
            Boolean isHasFixTalbe = false;

            WaveformDataProcess obj = new WaveformDataProcess();
            List<String> tableNames = obj.GetTableNames(mIICFilePath);

            foreach (String tableName in tableNames)
            {
                if (tableName.Contains("fix"))
                {
                    isHasFixTalbe = true;
                    break;
                }
            }

            return isHasFixTalbe;
        }
        #endregion

        #region 判断IIc文件是否被修正过---ygx--20140320
        /// <summary>
        /// 判断IIc文件是否被修正过
        /// </summary>
        /// <returns>true：已修正；false：未修正</returns>
        private Boolean IsIICFixed(String mIICFilePath)
        {
            Boolean retVal = false;
            Boolean isHasFix = IsHasFixTable(mIICFilePath);


            if (isHasFix == true)
            {
                try
                {
                    using (OleDbConnection sqlconn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + mIICFilePath + ";Persist Security Info=True"))
                    {
                        sqlconn.Open();

                        string sqlCreate = "select DISTINCT maxval2 from fix_defects ";
                        OleDbCommand sqlcom = new OleDbCommand(sqlCreate, sqlconn);

                        OleDbDataReader oldr = sqlcom.ExecuteReader();

                        int maxval2 = 0;

                        while (oldr.Read())
                        {
                            if (int.TryParse(oldr[0].ToString(), out maxval2))
                            {
                                if (maxval2 == -200)
                                {
                                    retVal = true;//里程已经修正
                                    break;
                                }
                            }
                        }

                        oldr.Close();
                        sqlconn.Close();
                        //return retVal;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    retVal = false;
                }
            }
            else
            {
                retVal = false;//里程未修正
            }

            return retVal;
        }
        #endregion

        //导出Excel
        private string _exportCsvExcptnValClsList(string citPath)
        {
            String excelPath = null;
            String excelName = null;

            if (excptnValClsList.Count == 0)
            {
                throw new Exception("输出结果为空！");
            }

            excelPath = Path.GetDirectoryName(citPath);
            excelName = Path.GetFileNameWithoutExtension(citPath);


            excelName = excelName + "_Diy.csv";

            excelPath = Path.Combine(excelPath, excelName);

            String head = "ID,类型,里程(公里),起点,终点,长度(米),限速(km/h),速度(km/h),行业标准,铁路局标准,超限值,有效性,原始里程";

            StringBuilder sbToCsv = new StringBuilder();
            String strFormat = "{0},";

            sbToCsv.AppendLine(head);

            foreach (ExceptionValueDIYClass tmpCls in excptnValClsList)
            {
                sbToCsv.AppendFormat(strFormat, tmpCls.id);
                sbToCsv.AppendFormat(strFormat, dicChannelEnToCh[tmpCls.exceptionType]);
                sbToCsv.AppendFormat(strFormat, tmpCls.milePos);
                sbToCsv.AppendFormat(strFormat, tmpCls.startMilePos);
                sbToCsv.AppendFormat(strFormat, tmpCls.endMilePos);
                sbToCsv.AppendFormat(strFormat, tmpCls.length);
                sbToCsv.AppendFormat(strFormat, tmpCls.maxSpeed);
                sbToCsv.AppendFormat(strFormat, tmpCls.speed);
                sbToCsv.AppendFormat(strFormat, tmpCls.excptnValueStandard);
                sbToCsv.AppendFormat(strFormat, tmpCls.excptnValRecmmd);
                sbToCsv.AppendFormat(strFormat, tmpCls.exceptionValue);
                sbToCsv.AppendFormat(strFormat, dicValid[tmpCls.valid]);
                sbToCsv.AppendFormat(strFormat, tmpCls.milePos_Original);

                sbToCsv.AppendLine();
            }

            File.WriteAllText(excelPath, sbToCsv.ToString(), Encoding.Default);

            return excelPath;
        }

        private void InitExcptnStnAndDiyClsList(string innerdbpath, string standardType)
        {
            excptnStnAndDiyClsList.Clear();

            String speeds = null;
            foreach (SudujiClass sudujiCls in sdjClsList)
            {
                if (String.IsNullOrEmpty(speeds) || !speeds.Contains(sudujiCls.speedClass.ToString()))
                {
                    speeds += sudujiCls.speedClass.ToString() + ",";
                }
            }
            speeds = speeds.Remove(speeds.Length - 1, 1);

            foreach (ValidExcptnClass cbLevel in validexcptnclasslist)
            {
                if (cbLevel.Checked == true)
                {
                    foreach (ValidExcptnType cbType in validexcptntypelist)
                    {
                        if (cbType.Checked == true)
                        {
                            string excptnType = null;

                            foreach (KeyValuePair<String, String> kvp in dicExcptnType)
                            {
                                if (kvp.Value == cbType.Text)
                                {
                                    excptnType = kvp.Key;
                                    break;
                                }
                            }

                            InnerFileOperator.InnerFilePath = innerdbpath;
                            InnerFileOperator.InnerConnString = "Provider = Microsoft.Jet.OLEDB.4.0; Data Source = {0}; Persist Security Info = True; Mode = Share Exclusive; Jet OLEDB:Database Password = iicdc; ";
                            using (OleDbConnection sqlconn = new OleDbConnection(InnerFileOperator.InnerConnString))
                            {
                                String cmd = String.Format("select *  from 大值国家标准表 where CLASS = {0} and TYPE = '{1}' and SPEED in ({2}) and STANDARDTYPE = {3}", int.Parse((String)(cbLevel.Tag)), excptnType, speeds, standardType);
                                OleDbCommand sqlcom = new OleDbCommand(cmd, sqlconn);

                                sqlconn.Open();
                                OleDbDataReader oddr = sqlcom.ExecuteReader();


                                ExceptionStndAndDiyClass tmpCls;

                                while (oddr.Read())
                                {
                                    String channenlType = oddr[3].ToString();
                                    if (channenlType.Contains("WideGage") || channenlType.Contains("NarrowGage"))
                                    {
                                        //把当前的超限值读取
                                        tmpCls = new ExceptionStndAndDiyClass();
                                        tmpCls.id = int.Parse(oddr[0].ToString());
                                        tmpCls.speed = int.Parse(oddr[1].ToString());
                                        tmpCls.level = int.Parse(oddr[2].ToString());
                                        tmpCls.type = oddr[3].ToString();
                                        tmpCls.valueStandard = float.Parse(oddr[4].ToString());
                                        //如果自定义值不存在，自动置比国家标准值小1
                                        if (oddr[5].ToString() == "")
                                        {
                                            if (tmpCls.valueStandard >= 0)
                                            {
                                                tmpCls.valueDIY = tmpCls.valueStandard - 1;
                                            }
                                            else
                                            {
                                                tmpCls.valueDIY = tmpCls.valueStandard + 1;
                                            }
                                        }

                                        else
                                        {
                                            tmpCls.valueDIY = float.Parse(oddr[5].ToString());
                                        }

                                        excptnStnAndDiyClsList.Add(tmpCls);
                                    }
                                    else
                                    {
                                        //把当前的超限值读取
                                        tmpCls = new ExceptionStndAndDiyClass();
                                        tmpCls.id = int.Parse(oddr[0].ToString());
                                        tmpCls.speed = int.Parse(oddr[1].ToString());
                                        tmpCls.level = int.Parse(oddr[2].ToString());
                                        tmpCls.type = oddr[3].ToString(); //正值部分
                                        tmpCls.valueStandard = float.Parse(oddr[4].ToString());
                                        //如果自定义值不存在，自动置比国家标准值小1
                                        if (oddr[5].ToString() == "")
                                        {
                                            //tmpCls.valueDIY = 0f;

                                            if (tmpCls.valueStandard >= 0)
                                            {
                                                tmpCls.valueDIY = tmpCls.valueStandard - 1;
                                            }
                                            else
                                            {
                                                tmpCls.valueDIY = tmpCls.valueStandard + 1;
                                            }
                                        }

                                        else
                                        {
                                            tmpCls.valueDIY = float.Parse(oddr[5].ToString());
                                        }

                                        excptnStnAndDiyClsList.Add(tmpCls);


                                        //负值部分
                                        //把当前的超限值读取
                                        tmpCls = new ExceptionStndAndDiyClass();
                                        tmpCls.id = int.Parse(oddr[0].ToString());
                                        tmpCls.speed = int.Parse(oddr[1].ToString());
                                        tmpCls.level = int.Parse(oddr[2].ToString());
                                        tmpCls.type = oddr[3].ToString();
                                        //负值部分
                                        tmpCls.valueStandard = float.Parse(oddr[4].ToString()) * (-1);
                                        //如果自定义值不存在，自动置比国家标准值小1
                                        if (oddr[5].ToString() == "")
                                        {
                                            //tmpCls.valueDIY = 0f;

                                            if (tmpCls.valueStandard >= 0)
                                            {
                                                tmpCls.valueDIY = tmpCls.valueStandard - 1;
                                            }
                                            else
                                            {
                                                tmpCls.valueDIY = tmpCls.valueStandard + 1;
                                            }
                                        }

                                        else
                                        {
                                            tmpCls.valueDIY = float.Parse(oddr[5].ToString()) * (-1);
                                        }

                                        excptnStnAndDiyClsList.Add(tmpCls);
                                    }
                                }

                                oddr.Close();
                                sqlconn.Close();
                            }

                        }

                    }
                }
            }

        }

        /// <summary>
        /// 把excptnStnAndDiyClsList中的类型转换成cit文件中的通道英文名
        /// </summary>
        private List<ExceptionStndAndDiyClass> DisplayExcptnStnAndDiyClsList(List<ExceptionStndAndDiyClass> oldList)
        {
            List<ExceptionStndAndDiyClass> newList = new List<ExceptionStndAndDiyClass>();

            foreach (ExceptionStndAndDiyClass tmpCls in oldList)
            {
                if (tmpCls.type.Contains("Prof_SC") || tmpCls.type.Contains("Align_SC"))
                {
                    String newTypeNameL = String.Format("L_{0}", tmpCls.type);
                    ExceptionStndAndDiyClass esdClsL = new ExceptionStndAndDiyClass();
                    esdClsL.id = tmpCls.id;
                    esdClsL.level = tmpCls.level;
                    esdClsL.speed = tmpCls.speed;
                    esdClsL.type = newTypeNameL;
                    esdClsL.valueDIY = tmpCls.valueDIY;
                    esdClsL.valueStandard = tmpCls.valueStandard;
                    newList.Add(esdClsL);

                    String newTypeNameR = String.Format("R_{0}", tmpCls.type);
                    ExceptionStndAndDiyClass esdClsR = new ExceptionStndAndDiyClass();
                    esdClsR.id = tmpCls.id;
                    esdClsR.level = tmpCls.level;
                    esdClsR.speed = tmpCls.speed;
                    esdClsR.type = newTypeNameR;
                    esdClsR.valueDIY = tmpCls.valueDIY;
                    esdClsR.valueStandard = tmpCls.valueStandard;
                    newList.Add(esdClsR);
                }
                else
                {
                    ExceptionStndAndDiyClass esdCls = new ExceptionStndAndDiyClass();

                    esdCls.id = tmpCls.id;
                    esdCls.level = tmpCls.level;
                    esdCls.speed = tmpCls.speed;
                    esdCls.type = tmpCls.type;
                    esdCls.valueDIY = tmpCls.valueDIY;
                    esdCls.valueStandard = tmpCls.valueStandard;


                    newList.Add(esdCls);
                }
            }


            return newList;
        }

        private List<ExceptionValueDIYClass> GetExcptnValByType(string citFile, ExceptionStndAndDiyClass esdCls)
        {
            List<ExceptionValueDIYClass> retList = new List<ExceptionValueDIYClass>();

            Boolean isNarrowGauge = false;
            if (esdCls.type.ToLower().Equals("narrowgage"))
            {
                isNarrowGauge = true;
            }

            Boolean isWideGauge = false;
            if (esdCls.type.ToLower().Equals("widegage"))
            {
                isWideGauge = true;
            }

            int channelIndex = GetChannelIndex(citFile, esdCls.type);
            if (channelIndex == -1)
                throw new Exception(esdCls.type + " channelIndex 找不到");
            int speedIndex = GetChannelIndex(citFile,"Speed");
            if (speedIndex == -1)
                throw new Exception("Speed speedIndex 找不到");
            float excptnStd = esdCls.valueStandard;
            float excptnDiy = esdCls.valueDIY;
            int maxSpeed = esdCls.speed;
            String excptnType = esdCls.type;
            int m_excptnClass = esdCls.level;

            FileStream fs = new FileStream(citFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            BinaryReader br = new BinaryReader(fs);
            br.ReadBytes(120);
            int channelNumbers = fileforma.iChannelNumber;
            br.ReadBytes(65 * channelNumbers);
            br.ReadBytes(BitConverter.ToInt32(br.ReadBytes(4), 0));

            long arrayLen = (br.BaseStream.Length - br.BaseStream.Position) / (2 * channelNumbers);
            long pos = 0;
            float maxVal = 0;
            long maxIndex = 0;
            float maxKmAndMeter = 0;
            float maxValSpeed = 0;//波峰时的列车速度

            float value = 0f;
            float speed = 0f;
            float kmAndMeter = 0f;
            int km = 0;
            int meter = 0;
            ExceptionValueDIYClass excptnCls = null;

            for (long i = 0; i < arrayLen; i++)
            {
                pos = br.BaseStream.Position;
                Byte[] data = br.ReadBytes(2 * channelNumbers);

                if (Encryption.IsEncryption(fileforma.sDataVersion))
                {
                    data = cit.ByteXORByte(data);
                }

                km = BitConverter.ToInt16(data, 0);
                meter = BitConverter.ToInt16(data, 2);
                kmAndMeter = km + meter / 4.0f / 1000;

                speed = BitConverter.ToInt16(data, speedIndex * 2) / cdlist.GetChannleScale(speedIndex) + cdlist.GetChannelOffset(speedIndex);
                value = BitConverter.ToInt16(data, channelIndex * 2) /cdlist.GetChannleScale(channelIndex) + cdlist.GetChannelOffset(channelIndex);

                //if (isWideGauge)
                if ((excptnDiy > 0 && excptnStd > 0) && excptnStd > excptnDiy)
                {
                    if (value >= excptnDiy)
                    {
                        if (excptnCls == null)
                        {
                            excptnCls = new ExceptionValueDIYClass();

                            excptnCls.startMileIndex = pos;
                            excptnCls.startMilePos = kmAndMeter;
                        }

                        if (value > maxVal)
                        {
                            maxVal = value;
                            maxIndex = pos;
                            maxKmAndMeter = kmAndMeter;
                            maxValSpeed = speed;
                        }
                    }

                    if (value < excptnDiy)
                    {
                        if (excptnCls != null)
                        {
                            excptnCls.exceptionValue = maxVal;
                            excptnCls.endMileIndex = pos;
                            excptnCls.endMilePos = kmAndMeter;
                            excptnCls.length = Math.Abs(excptnCls.endMileIndex - excptnCls.startMileIndex) / (2 * channelNumbers) * 0.25f;

                            excptnCls.mileIndex = maxIndex;
                            excptnCls.milePos = maxKmAndMeter;
                            excptnCls.milePos_Original = maxKmAndMeter;
                            excptnCls.speed = maxValSpeed;
                            excptnCls.maxSpeed = maxSpeed;
                            excptnCls.valid = "N";
                            excptnCls.exceptionType = excptnType;
                            excptnCls.excptnValRecmmd = excptnDiy;
                            excptnCls.excptnValueStandard = excptnStd;
                            excptnCls.excptnClass = m_excptnClass;



                            ExceptionValueDIYClass tmpCls = new ExceptionValueDIYClass();
                            tmpCls.id = excptnCls.id;
                            tmpCls.length = excptnCls.length;
                            tmpCls.maxSpeed = excptnCls.maxSpeed;
                            tmpCls.mileIndex = excptnCls.mileIndex;
                            tmpCls.milePos = excptnCls.milePos;
                            tmpCls.milePos_Original = excptnCls.milePos_Original;
                            tmpCls.speed = excptnCls.speed;
                            tmpCls.startMileIndex = excptnCls.startMileIndex;
                            tmpCls.startMilePos = excptnCls.startMilePos;
                            tmpCls.valid = excptnCls.valid;
                            tmpCls.endMileIndex = excptnCls.endMileIndex;
                            tmpCls.endMilePos = excptnCls.endMilePos;
                            tmpCls.exceptionType = excptnCls.exceptionType;
                            tmpCls.exceptionValue = excptnCls.exceptionValue;
                            tmpCls.excptnValRecmmd = excptnCls.excptnValRecmmd;
                            tmpCls.excptnClass = excptnCls.excptnClass;
                            tmpCls.excptnValueStandard = excptnCls.excptnValueStandard;

                            retList.Add(tmpCls);

                            excptnCls = null;

                            maxVal = 0;
                            maxIndex = 0;
                            maxKmAndMeter = 0;
                            maxValSpeed = 0;
                        }
                    }

                }
                //else if (isNarrowGauge)
                if ((excptnDiy < 0 && excptnStd < 0) && excptnStd < excptnDiy)
                {
                    if (value <= excptnDiy)
                    {
                        if (excptnCls == null)
                        {
                            excptnCls = new ExceptionValueDIYClass();

                            excptnCls.startMileIndex = pos;
                            excptnCls.startMilePos = kmAndMeter;
                        }

                        if (value < maxVal)
                        {
                            maxVal = value;
                            maxIndex = pos;
                            maxKmAndMeter = kmAndMeter;
                            maxValSpeed = speed;
                        }
                    }


                    if (value > excptnDiy)
                    {
                        if (excptnCls != null)
                        {
                            excptnCls.exceptionValue = maxVal;
                            excptnCls.endMileIndex = pos;
                            excptnCls.endMilePos = kmAndMeter;
                            excptnCls.length = Math.Abs(excptnCls.endMileIndex - excptnCls.startMileIndex) / (2 * channelNumbers) * 0.25f;

                            excptnCls.mileIndex = maxIndex;
                            excptnCls.milePos = maxKmAndMeter;
                            excptnCls.milePos_Original = maxKmAndMeter;
                            excptnCls.speed = maxValSpeed;
                            excptnCls.maxSpeed = maxSpeed;
                            excptnCls.valid = "N";
                            excptnCls.exceptionType = excptnType;
                            excptnCls.excptnValRecmmd = excptnDiy;
                            excptnCls.excptnValueStandard = excptnStd;
                            excptnCls.excptnClass = m_excptnClass;


                            ExceptionValueDIYClass tmpCls = new ExceptionValueDIYClass();
                            tmpCls.id = excptnCls.id;
                            tmpCls.length = excptnCls.length;
                            tmpCls.maxSpeed = excptnCls.maxSpeed;
                            tmpCls.mileIndex = excptnCls.mileIndex;
                            tmpCls.milePos = excptnCls.milePos;
                            tmpCls.milePos_Original = excptnCls.milePos_Original;
                            tmpCls.speed = excptnCls.speed;
                            tmpCls.startMileIndex = excptnCls.startMileIndex;
                            tmpCls.startMilePos = excptnCls.startMilePos;
                            tmpCls.valid = excptnCls.valid;
                            tmpCls.endMileIndex = excptnCls.endMileIndex;
                            tmpCls.endMilePos = excptnCls.endMilePos;
                            tmpCls.exceptionType = excptnCls.exceptionType;
                            tmpCls.exceptionValue = excptnCls.exceptionValue;
                            tmpCls.excptnValRecmmd = excptnCls.excptnValRecmmd;
                            tmpCls.excptnClass = excptnCls.excptnClass;
                            tmpCls.excptnValueStandard = excptnCls.excptnValueStandard;

                            retList.Add(tmpCls);

                            excptnCls = null;

                            maxVal = 0;
                            maxIndex = 0;
                            maxKmAndMeter = 0;
                            maxValSpeed = 0;
                        }
                    }
                }
                else
                {

                }


            }

            //删除无效的大值：大于等于标准值；长度小于等于1米；速度区段之外的

            for (int i = retList.Count - 1; i >= 0; i--)
            {
                if (retList[i].length <= 1)
                {
                    retList.RemoveAt(i);
                }
            }

            for (int i = retList.Count - 1; i >= 0; i--)
            {
                //滤除负半边的值
                if ((excptnDiy < 0 && excptnStd < 0) && excptnStd < excptnDiy)
                {
                    if (retList[i].exceptionValue <= excptnStd)
                    {
                        retList.RemoveAt(i);
                    }
                }
                //滤除正半边的值
                if ((excptnDiy > 0 && excptnStd > 0) && excptnStd > excptnDiy)
                {
                    if (retList[i].exceptionValue >= excptnStd)
                    {
                        retList.RemoveAt(i);
                    }
                }

            }

            foreach (SudujiClass tmpCls in sdjClsList)
            {
                if (tmpCls.speedClass == maxSpeed)
                {
                    float startMile = 0;
                    float endMile = 0;

                    if (tmpCls.startMile < tmpCls.endMile)
                    {
                        startMile = tmpCls.startMile;
                        endMile = tmpCls.endMile;
                    }
                    else
                    {
                        startMile = tmpCls.endMile;
                        endMile = tmpCls.startMile;
                    }

                    for (int i = retList.Count - 1; i >= 0; i--)
                    {
                        if (retList[i].milePos >= startMile && retList[i].milePos <= endMile)
                        {
                            retList[i].valid = "E";
                        }
                    }


                }
            }

            for (int i = retList.Count - 1; i >= 0; i--)
            {
                if (retList[i].valid == "N")
                {
                    retList.RemoveAt(i);
                }
            }

            return retList;
        }

        private byte[] ByteXORByte(byte[] b)
        {
            for (int iIndex = 0; iIndex < b.Length; iIndex++)
            {
                b[iIndex] = (byte)(b[iIndex] ^ 128);
            }
            return b;
        }

        private int GetChannelIndex(string citFile,string excptnType88)
        {
            String channelNameEn = null;
            //String excptnTypeLower = excptnType88.ToLower();
            if (excptnType88.Contains("gage"))
            {
                channelNameEn = "Gage";
            }
            else
            {
                channelNameEn = excptnType88;
            }

            int channelIndex = cdlist.GetChannelIdByName(channelNameEn, "");
            return channelIndex;
        }

        private void ReSortExcptnValClsListById()
        {
            for (int i = 0; i < excptnValClsList.Count; i++)
            {
                excptnValClsList[i].id = i + 1;
            }
        }

        public class ValidExcptnClass {
            public string Name;
            public bool Checked;
            public string Tag;
            public string Text;
        }

        private void InitCheckBoxList_ExcptnClass()
        {
            ValidExcptnClass validexcptn = new ValidExcptnClass();
            validexcptn.Name = "checkBoxLevel1";
            validexcptn.Checked = true;
            validexcptn.Tag = "1";
            validexcptn.Text = "一级";
            validexcptnclasslist.Add(validexcptn);

            validexcptn = new ValidExcptnClass();
            validexcptn.Name = "checkBoxLevel2";
            validexcptn.Checked = false;
            validexcptn.Tag = "2";
            validexcptn.Text = "二级";
            validexcptnclasslist.Add(validexcptn);

            validexcptn = new ValidExcptnClass();
            validexcptn.Name = "checkBoxLevel3";
            validexcptn.Checked = false;
            validexcptn.Tag = "3";
            validexcptn.Text = "三级";
            validexcptnclasslist.Add(validexcptn);

            validexcptn = new ValidExcptnClass();
            validexcptn.Name = "checkBoxLevel4";
            validexcptn.Checked = false;
            validexcptn.Tag = "4";
            validexcptn.Text = "四级";
            validexcptnclasslist.Add(validexcptn);
        }

        public class ValidExcptnType
        {
            public string Name;
            public bool Checked;
            public string Tag;
            public string Text;
        }

        private void InitCheckBoxList_ExcptnType()
        {
            ValidExcptnType validexcptntype = new ValidExcptnType();
            validexcptntype.Name = "checkBox_Prof_SC";
            validexcptntype.Checked = true;
            validexcptntype.Tag = "Prof_SC";
            validexcptntype.Text = "高低_中波";
            validexcptntypelist.Add(validexcptntype);

            validexcptntype = new ValidExcptnType();
            validexcptntype.Name = "checkBox_Prof_SC_70";
            validexcptntype.Checked = true;
            validexcptntype.Tag = "Prof_SC_70";
            validexcptntype.Text = "高低_70长波";
            validexcptntypelist.Add(validexcptntype);

            validexcptntype = new ValidExcptnType();
            validexcptntype.Name = "checkBox_Prof_SC_120";
            validexcptntype.Checked = true;
            validexcptntype.Tag = "Prof_SC_120";
            validexcptntype.Text = "高低_120长波";
            validexcptntypelist.Add(validexcptntype);

            validexcptntype = new ValidExcptnType();
            validexcptntype.Name = "checkBox_Align_SC";
            validexcptntype.Checked = true;
            validexcptntype.Tag = "Align_SC";
            validexcptntype.Text = "轨向_中波";
            validexcptntypelist.Add(validexcptntype);

            validexcptntype = new ValidExcptnType();
            validexcptntype.Name = "checkBox_Align_SC_70";
            validexcptntype.Checked = true;
            validexcptntype.Tag = "Align_SC_70";
            validexcptntype.Text = "轨向_70长波";
            validexcptntypelist.Add(validexcptntype);


            validexcptntype = new ValidExcptnType();
            validexcptntype.Name = "checkBox_Align_SC_120";
            validexcptntype.Checked = true;
            validexcptntype.Tag = "Align_SC_120";
            validexcptntype.Text = "轨向_120长波";
            validexcptntypelist.Add(validexcptntype);

            validexcptntype = new ValidExcptnType();
            validexcptntype.Name = "checkBox_WideGage";
            validexcptntype.Checked = false;
            validexcptntype.Tag = "WideGage";
            validexcptntype.Text = "大轨距";
            validexcptntypelist.Add(validexcptntype);

            validexcptntype = new ValidExcptnType();
            validexcptntype.Name = "checkBox_NarrowGage";
            validexcptntype.Checked = false;
            validexcptntype.Tag = "NarrowGage";
            validexcptntype.Text = "小轨距";
            validexcptntypelist.Add(validexcptntype);

            validexcptntype = new ValidExcptnType();
            validexcptntype.Name = "checkBox_CrossLevel";
            validexcptntype.Checked = false;
            validexcptntype.Tag = "CrossLevel";
            validexcptntype.Text = "水平";
            validexcptntypelist.Add(validexcptntype);

            validexcptntype = new ValidExcptnType();
            validexcptntype.Name = "checkBox_Short_Twist";
            validexcptntype.Checked = true;
            validexcptntype.Tag = "Short_Twist";
            validexcptntype.Text = "三角坑";
            validexcptntypelist.Add(validexcptntype);

            validexcptntype = new ValidExcptnType();
            validexcptntype.Name = "checkBox_VACC";
            validexcptntype.Checked = true;
            validexcptntype.Tag = "VACC";
            validexcptntype.Text = "车体垂加";
            validexcptntypelist.Add(validexcptntype);

            validexcptntype = new ValidExcptnType();
            validexcptntype.Name = "checkBox_LACC";
            validexcptntype.Checked = true;
            validexcptntype.Tag = "LACC";
            validexcptntype.Text = "车体横加";
            validexcptntypelist.Add(validexcptntype);
            
        }

        private void InitDicValid()
        {
            dicValid.Clear();
            dicValid.Add("N", "无效");
            dicValid.Add("E", "有效");
        }

        //窗口加载---初始的时候
        private void InitSdjClsList(string innerdbpath, string lineCode, int sDir)
        {
            InnerFileOperator.InnerFilePath = innerdbpath;
            InnerFileOperator.InnerConnString = "Provider = Microsoft.Jet.OLEDB.4.0; Data Source = {0}; Persist Security Info = True; Mode = Share Exclusive; Jet OLEDB:Database Password = iicdc; ";
            using (OleDbConnection sqlconn = new OleDbConnection(InnerFileOperator.InnerConnString))
            {
                string rDir = "";
                if (sDir == 1)
                {
                    rDir = "上";
                }
                if (sDir == 2)
                {
                    rDir = "下";
                }
                if (sDir == 3)
                {
                    rDir = "单";
                }

                String cmd = String.Format("select *  from Suduji where 线编号='{0}' and 行别='{1}'", lineCode, rDir);
                OleDbCommand sqlcom = new OleDbCommand(cmd, sqlconn);
                sqlconn.Open();
                OleDbDataReader oddr = sqlcom.ExecuteReader();
                sdjClsList.Clear();
                SudujiClass tmpCls;

                while (oddr.Read())
                {
                    //把当前的超限值读取
                    tmpCls = new SudujiClass();
                    tmpCls.Id = int.Parse(oddr[0].ToString());
                    tmpCls.lineNameCh = oddr[2].ToString();
                    tmpCls.lineCode = oddr[3].ToString();
                    tmpCls.sDir = oddr[4].ToString();
                    tmpCls.sDirCode = int.Parse(oddr[5].ToString());
                    tmpCls.startMile = float.Parse(oddr[6].ToString());
                    tmpCls.endMile = float.Parse(oddr[7].ToString());
                    tmpCls.speedClass = int.Parse(oddr[8].ToString());//速度等级
                    tmpCls.tqiSpeedClass = int.Parse(oddr[9].ToString());//tqi速度等级

                    sdjClsList.Add(tmpCls);
                }

                oddr.Close();
                sqlconn.Close();
            }

            if (sdjClsList.Count==0) {
                throw new Exception("未获取到速度集");
            }
        }

        private void InitDicExcptnType()
        {
            dicExcptnType.Clear();

            dicExcptnType.Add("Prof_SC", "高低_中波");
            dicExcptnType.Add("Prof_SC_70", "高低_70长波");
            dicExcptnType.Add("Prof_SC_120", "高低_120长波");
            dicExcptnType.Add("Align_SC", "轨向_中波");
            dicExcptnType.Add("Align_SC_70", "轨向_70长波");
            dicExcptnType.Add("Align_SC_120", "轨向_120长波");
            dicExcptnType.Add("WideGage", "大轨距");
            dicExcptnType.Add("NarrowGage", "小轨距");
            dicExcptnType.Add("CrossLevel", "水平");
            dicExcptnType.Add("Short_Twist", "三角坑");
            dicExcptnType.Add("LACC", "车体横加");
            dicExcptnType.Add("VACC", "车体垂加");
        }
        private void InitDicChannelEnToCh()
        {
            dicChannelEnToCh.Clear();

            dicChannelEnToCh.Add("L_Prof_SC", "左高低_中波");
            dicChannelEnToCh.Add("L_Prof_SC_70", "左高低_70长波");
            dicChannelEnToCh.Add("L_Prof_SC_120", "左高低_120长波");

            dicChannelEnToCh.Add("R_Prof_SC", "右高低_中波");
            dicChannelEnToCh.Add("R_Prof_SC_70", "右高低_70长波");
            dicChannelEnToCh.Add("R_Prof_SC_120", "右高低_120长波");

            dicChannelEnToCh.Add("L_Align_SC", "左轨向_中波");
            dicChannelEnToCh.Add("L_Align_SC_70", "左轨向_70长波");
            dicChannelEnToCh.Add("L_Align_SC_120", "左轨向_120长波");

            dicChannelEnToCh.Add("R_Align_SC", "右轨向_中波");
            dicChannelEnToCh.Add("R_Align_SC_70", "右轨向_70长波");
            dicChannelEnToCh.Add("R_Align_SC_120", "右轨向_120长波");

            dicChannelEnToCh.Add("WideGage", "大轨距");
            dicChannelEnToCh.Add("NarrowGage", "小轨距");
            dicChannelEnToCh.Add("CrossLevel", "水平");
            dicChannelEnToCh.Add("Short_Twist", "三角坑");
            dicChannelEnToCh.Add("LACC", "车体横向加速度");
            dicChannelEnToCh.Add("VACC", "车体垂向加速度");
        }
        private void InitDicChannelChToEn()
        {
            dicChannelChToEn.Clear();

            dicChannelChToEn.Add("左高低_中波", "L SURFACE");
            dicChannelChToEn.Add("左高低_70长波", "L SURFACE 70M");
            dicChannelChToEn.Add("左高低_120长波", "L SURFACE 120M");

            dicChannelChToEn.Add("右高低_中波", "R SURFACE");
            dicChannelChToEn.Add("右高低_70长波", "R SURFACE 70M");
            dicChannelChToEn.Add("右高低_120长波", "R SURFACE 120M");

            dicChannelChToEn.Add("左轨向_中波", "L ALIGNMENT");
            dicChannelChToEn.Add("左轨向_70长波", "L ALIGNMENT 70M");
            dicChannelChToEn.Add("左轨向_120长波", "L ALIGNMENT 120M");

            dicChannelChToEn.Add("右轨向_中波", "R ALIGNMENT");
            dicChannelChToEn.Add("右轨向_70长波", "R ALIGNMENT 70M");
            dicChannelChToEn.Add("右轨向_120长波", "R ALIGNMENT 120M");

            dicChannelChToEn.Add("大轨距", "WDGA");
            dicChannelChToEn.Add("小轨距", "TGTGA");
            dicChannelChToEn.Add("水平", "CROSSLEVEL");
            dicChannelChToEn.Add("三角坑", "TWIST");
            dicChannelChToEn.Add("车体横向加速度", "LAT ACCEL");
            dicChannelChToEn.Add("车体垂向加速度", "VERT ACCEL");
        }

        #region 数据类--自定义超限显示
        /// <summary>
        /// 数据类--自定义超限显示
        /// </summary>
        public class ExceptionValueDIYClass
        {
            #region 序号
            /// <summary>
            /// 序号
            /// </summary>
            public int id;
            #endregion
            #region 超限类型
            /// <summary>
            /// 超限类型
            /// </summary>
            public String exceptionType;
            #endregion
            #region 超限峰值的文件指针
            /// <summary>
            /// 超限峰值的文件指针
            /// </summary>
            public long mileIndex;
            #endregion
            #region 超限峰值里程--单位：公里
            /// <summary>
            /// 超限峰值里程--单位：公里
            /// </summary>
            public float milePos;
            #endregion
            #region 超限峰值原始里程--单位：公里
            /// <summary>
            /// 超限峰值原始里程--单位：公里
            /// </summary>
            public float milePos_Original;
            #endregion
            #region 超限起点文件指针
            /// <summary>
            /// 超限起点文件指针
            /// </summary>
            public long startMileIndex;
            #endregion
            #region 超限起点里程--单位：公里
            /// <summary>
            /// 超限起点里程--单位：公里
            /// </summary>
            public float startMilePos;
            #endregion
            #region 超限终点文件指针
            /// <summary>
            /// 超限终点文件指针
            /// </summary>
            public long endMileIndex;
            #endregion
            #region 超限终点里程--单位：公里
            /// <summary>
            /// 超限终点里程--单位：公里
            /// </summary>
            public float endMilePos;
            #endregion
            #region 超限长度--单位：米
            /// <summary>
            /// 超限长度--单位：米
            /// </summary>
            public float length;
            #endregion
            #region 超限等级
            /// <summary>
            /// 超限等级
            /// </summary>
            public int excptnClass;
            #endregion
            #region 最高限速
            /// <summary>
            /// 最高限速
            /// </summary>
            public float maxSpeed;
            #endregion
            #region 列车速度--单位：km/h
            /// <summary>
            /// 列车速度--单位：km/h
            /// </summary>
            public float speed;
            #endregion
            #region 行业标准
            /// <summary>
            /// 行业标准
            /// </summary>
            public float excptnValueStandard;
            #endregion
            #region 铁路局标准
            /// <summary>
            /// 铁路局标准
            /// </summary>
            public float excptnValRecmmd;
            #endregion
            #region 超限值
            /// <summary>
            /// 超限值
            /// </summary>
            public float exceptionValue;
            #endregion
            #region 是否有效--有效：E；无效：N
            /// <summary>
            /// 是否有效--有效：E；无效：N
            /// </summary>
            public String valid;
            #endregion
        }
        #endregion

        #region 数据类--Suduji
        /// <summary>
        /// 数据类--Suduji
        /// </summary>
        public class SudujiClass
        {
            #region 速度区段Id
            /// <summary>
            /// 速度区段Id
            /// </summary>
            public int Id;
            #endregion
            #region 中文线路名
            /// <summary>
            /// 中文线路名
            /// </summary>
            public String lineNameCh;
            #endregion
            #region 线路编号
            /// <summary>
            /// 线路编号
            /// </summary>
            public String lineCode;
            #endregion
            #region 行别
            /// <summary>
            /// 行别
            /// </summary>
            public String sDir;
            #endregion
            #region 起止里程
            /// <summary>
            /// 起止里程
            /// </summary>
            public float startMile;
            #endregion
            #region 行别ID
            /// <summary>
            /// 行别ID
            /// </summary>
            public int sDirCode;
            #endregion
            #region 速度等级
            /// <summary>
            /// 速度等级
            /// </summary>
            public int speedClass;
            #endregion
            #region 终止里程
            /// <summary>
            /// 终止里程
            /// </summary>
            public float endMile;
            #endregion
            #region TQI速度等级
            /// <summary>
            /// TQI速度等级
            /// </summary>
            public int tqiSpeedClass;
            #endregion
            #region 允许速度
            /// <summary>
            /// 允许速度
            /// </summary>
            public int speed;
            #endregion
        }
        #endregion

        #region 数据类--超限标准值和自定义值表
        /// <summary>
        /// 数据类--超限标准值和自定义值表
        /// </summary>
        public class ExceptionStndAndDiyClass
        {
            #region 序号
            /// <summary>
            /// 序号
            /// </summary>
            public int id;
            #endregion
            #region 速度等级
            /// <summary>
            /// 速度等级
            /// </summary>
            public int speed;
            #endregion
            #region 超限等级
            /// <summary>
            /// 超限等级
            /// </summary>
            public int level;
            #endregion
            #region 超限类型
            /// <summary>
            /// 超限类型
            /// </summary>
            public String type;
            #endregion
            #region 超限值--国家标准
            /// <summary>
            /// 超限值--国家标准
            /// </summary>
            public float valueStandard;
            #endregion
            #region 超限值--自定义
            /// <summary>
            /// 超限值--自定义
            /// </summary>
            public float valueDIY;
            #endregion
            #region 标准类型
            /// <summary>
            /// 标准类型
            /// </summary>
            public int standardType;
            #endregion
        }
        #endregion

        private void ExceptionFix(string citFilePath, string iicFilePath, List<IndexSta> listIC, List<ExceptionType> listETC)
        {
            List<Defects> listDC = new List<Defects>();
            try
            {
                using (OleDbConnection sqlconn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + iicFilePath + ";Persist Security Info=True"))
                {
                    string sqlCreate = "select RecordNumber,maxpost,maxminor from fix_defects where maxval2 is null or maxval2<>-200";
                    OleDbCommand sqlcom = new OleDbCommand(sqlCreate, sqlconn);
                    sqlconn.Open();
                    OleDbDataReader oleDBdr = sqlcom.ExecuteReader();
                    while (oleDBdr.Read())
                    {
                        Defects dc = new Defects();
                        dc.iRecordNumber = int.Parse(oleDBdr.GetValue(0).ToString());
                        dc.iMaxpost = int.Parse(oleDBdr.GetValue(1).ToString());
                        dc.dMaxminor = double.Parse(oleDBdr.GetValue(2).ToString());
                        listDC.Add(dc);
                    }

                    oleDBdr.Close();
                    sqlconn.Close();
                }
            }
            catch
            {

            }

            //
            List<Milestone> listMilestone = cit.GetAllMileStone(citFilePath);
            List<cPointFindMeter> listcpfm = new List<cPointFindMeter>();

            for (int i = 0; i < listMilestone.Count; i++)
            {
                cPointFindMeter cpfm = new cPointFindMeter();
                cpfm.lLoc = listMilestone[i].mFilePosition;
                cpfm.lMeter = Convert.ToInt64(listMilestone[i].mKm * 100000 + listMilestone[i].mMeter * 100);

                listcpfm.Add(cpfm);
            }


            for (int i = 0; i < listDC.Count; i++)
            {
                for (int j = 0; j < listcpfm.Count; j++)
                {
                    if (listcpfm[j].lMeter == listDC[i].GetMeter())
                    {
                        int iValue = PointToMeter(listIC, listcpfm[j].lLoc, fileforma.iChannelNumber, fileforma.iKmInc);
                        if (iValue > 0)
                        {
                            listDC[i].bFix = true;
                            listDC[i].iMaxpost = iValue / 1000;
                            listDC[i].dMaxminor = iValue % 1000;
                        }
                        break;
                    }
                }
            }


            //将修正后的偏差数据存储到iic中

            try
            {
                using (OleDbConnection sqlconn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + iicFilePath + ";Persist Security Info=True"))
                {
                    string sqlCreate = "";
                    OleDbCommand sqlcom = new OleDbCommand(sqlCreate, sqlconn);
                    sqlconn.Open();
                    for (int i = 0; i < listDC.Count; i++)
                    {
                        if (listDC[i].bFix)
                        {
                            sqlcom.CommandText = "update fix_defects set maxpost=" + listDC[i].iMaxpost.ToString() +
                                ",maxminor=" + listDC[i].dMaxminor.ToString() + ",maxval2=-200 where RecordNumber=" + listDC[i].iRecordNumber.ToString();
                            sqlcom.ExecuteNonQuery();
                        }
                    }
                    sqlconn.Close();
                }
            }
            catch
            {

            }

        }

        /// <summary>
        /// 根据点返回索引文件里对应的里程信息
        /// </summary>
        /// <param name="listIC">索引信息</param>
        /// <param name="lPosition">点的位置</param>
        /// <param name="tds">文件通道书</param>
        /// <param name="sKmInc">增减里程标 【0增里程；1减里程】</param>
        /// <returns>索引里程：单位为米</returns>
        private int PointToMeter(List<IndexSta> listIC, long lPosition, int tds, int iKmInc)
        {
            int iMeter = 0;
            //处理里程
            for (int i = 0; i < listIC.Count; i++)
            {
                if (lPosition >= listIC[i].lStartPoint && lPosition < listIC[i].lEndPoint)
                {
                    int iCount = 1;
                    long lCurPos = lPosition - listIC[i].lStartPoint;
                    int iIndex = 0;
                    if (listIC[i].sType.Contains("长链"))
                    {
                        int iKM = 0;
                        double dCDLMeter = float.Parse(listIC[i].lContainsMeter) * 1000;
                        //减里程
                        if (iKmInc == 1)
                        {
                            iKM = (int)float.Parse(listIC[i].LEndMeter);
                        }
                        else
                        {
                            iKM = (int)float.Parse(listIC[i].lStartMeter);
                        }
                        for (iIndex = 0; iIndex < iCount && (lPosition + iIndex * tds * 2) < listIC[i].lEndPoint; )
                        {
                            float f = (lCurPos / tds / 2 + iIndex) * ((float.Parse(listIC[i].lContainsMeter) * 1000 / listIC[i].lContainsPoint));

                            Milestone wm = new Milestone();
                            //减里程
                            if (iKmInc == 1)
                            {
                                wm.mKm = iKM;
                                wm.mMeter = (float)(dCDLMeter - f);
                            }
                            else
                            {
                                wm.mKm = iKM;
                                wm.mMeter = (float)(dCDLMeter + f);
                            }
                            wm.mFilePosition = (lPosition + (iIndex * tds * 2));
                            iMeter = Convert.ToInt32(wm.GetMeter());
                            return iMeter;
                        }
                    }
                    else
                    {
                        double dMeter = float.Parse(listIC[i].lStartMeter) * 1000;
                        for (iIndex = 0; iIndex < iCount && (lPosition + iIndex * tds * 2) < listIC[i].lEndPoint; )
                        {
                            float f = (lCurPos / tds / 2 + iIndex) * ((float.Parse(listIC[i].lContainsMeter) * 1000 / listIC[i].lContainsPoint));
                            Milestone wm = new Milestone();
                            //减里程
                            if (iKmInc == 1)
                            {
                                wm.mKm = (int)((dMeter - f) / 1000);
                                wm.mMeter = (float)((dMeter - f) % 1000);
                            }
                            else
                            {
                                wm.mKm = (int)((dMeter + f) / 1000);
                                wm.mMeter = (float)((dMeter + f) % 1000);
                            }
                            wm.mFilePosition = (lPosition + (iIndex * tds * 2));
                            iMeter = Convert.ToInt32(wm.GetMeter());
                            return iMeter;
                        }
                    }
                    break;

                }

            }
            return iMeter;
        }


        /// <summary>
        /// 偏差类
        /// </summary>
        public class Defects
        {
            public int iRecordNumber = 0;
            /// <summary>
            /// 单位为公里
            /// </summary>
            public int iMaxpost = 0;
            /// <summary>
            /// 单位为米
            /// </summary>
            public double dMaxminor = 0;
            public bool bFix = false;
            /// <summary>
            /// 获取公里标，单位为厘米
            /// </summary>
            /// <returns></returns>
            public int GetMeter()
            {
                return iMaxpost * 100000 + (int)(dMaxminor * 100);
            }
        }

        /// <summary>
        /// 与数据库中IndexSta表对应的长短链索引数据类
        /// </summary>
        public class IndexSta
        {
            /// <summary>
            /// 长短链索引id
            /// </summary>
            public int iID { get; set; }

            /// <summary>
            /// 这个值估计没什么特殊含义
            /// </summary>
            public int iIndexID { get; set; }

            /// <summary>
            /// 长短链对应的起始文件指针
            /// </summary>
            public long lStartPoint { get; set; }

            /// <summary>
            /// 长短链对应的起始公里标
            /// </summary>
            public string lStartMeter { get; set; }

            /// <summary>
            /// 长短链对应的终止文件指针
            /// </summary>
            public long lEndPoint { get; set; }

            /// <summary>
            /// 长短链对应的终止公里标
            /// </summary>
            public string LEndMeter { get; set; }

            /// <summary>
            /// 长短链所包含的采样点数
            /// </summary>
            public long lContainsPoint { get; set; }

            /// <summary>
            /// 长短链所包含的公里数（单位为公里）
            /// </summary>
            public string lContainsMeter { get; set; }

            /// <summary>
            /// 长短链类别
            /// </summary>
            public string sType { get; set; }
        }

        /// <summary>
        /// 偏差类型类
        /// </summary>
        public class ExceptionType
        {
            public string EXCEPTIONEN { get; set; }
            public string EXCEPTIONCN { get; set; }
        }

        /// <summary>
        /// iic修正时使用的数据类
        /// </summary>
        public class cPointFindMeter
        {
            /// <summary>
            /// 公里标：单位为厘米
            /// </summary>
            public long lMeter = 0;
            /// <summary>
            /// 里程对应的文件指针
            /// </summary>
            public long lLoc = 0;
        }


    }
}
