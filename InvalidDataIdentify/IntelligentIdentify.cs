using IntegratedDisplayCommon.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvalidDataProcessing;
using System.IO;
using CitIndexFileSDK;
using System.Runtime.InteropServices;
using CitFileSDK;


namespace InvalidDataIdentify
{
    public class IntelligentIdentify
    {
        /// <summary>
        /// 无效区段智能识别
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(131)]
        public string InvalidData(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                
                //cit文件路径
                string citFile = Convert.ToString(obj["citFile"]);

                //将文件按传入点位数分段读取
                int pointCount = Convert.ToInt32(obj["pointCount"].ToString());

                string idfName = Path.GetFileNameWithoutExtension(citFile) + "_InvalidData.idf";
                string idfFile = Path.Combine(Path.GetDirectoryName(citFile), idfName);
                bool data = _invalidData(citFile, pointCount, idfFile);

                if (data)
                {
                    resultInfo.flag = 1;
                    resultInfo.msg = "执行成功";
                    resultInfo.data = idfFile;
                }
                else
                {
                    resultInfo.flag = 0;
                    resultInfo.msg = "执行失败";
                    resultInfo.data = "";
                }

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
        /// 无效区段智能识别
        /// </summary>
        /// <param name="citFile">cit文件名</param>
        /// <param name="pointCount">按照点位数分段进行读取</param>
        /// <param name="idfFile">idf文件名</param>
        /// <returns></returns>
        private bool _invalidData(string citFile, int pointCount,string idfFile)
        {
            //初始化idf文件
            IndexOperator oper = new IndexOperator();
            oper.IndexFilePath = idfFile;

            //情况无效数据表
            string cmdText = "delete from InvalidData";
            oper.ExcuteSql(cmdText);

            DataProcessing IDP = new DataProcessing();
            ////处理单通道数据
            ////IDP.GetDataInfo(CommonClass.listDIC[0].sFilePath, CommonClass.listDIC[0].sAddFile);
            ////处理多通道数据
            //bool result = IDP.GetDataInfoMulti(citFile, pointCount, idfFile);

            bool result = GetDataInfoMulti(citFile, pointCount, idfFile);
            return result;
        }

        #region 接口函数：无效数据滤除---处理多个通道数据
        /// <summary>
        /// 接口函数：无效数据滤除---处理多个通道数据
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="sAddFileName"></param>
        /// <returns></returns>
        private bool GetDataInfoMulti(string FileName,int pointCount, string sAddFileName)
        {
            // CIT文件相关操作类
            CITFileProcess cfprocess = new CITFileProcess();

            // 通道定义相关操作类
            ChannelDefinitionList cdlist = new ChannelDefinitionList();

            //matlab算法
            PreproceingDeviationClass pdc = new PreproceingDeviationClass();

            //获取文件信息
            FileInformation fileinfo = new FileInformation();

            try
            {
                long[] position = cfprocess.GetPositons(FileName);
                long startPos = position[0]; //开始位置、结束位置
                long endPos = position[1];

                cdlist.channelDefinitionList = cfprocess.GetChannelDefinitionList(FileName);

                //分段读取方法////////////////////

                long totleSample = cfprocess.GetTotalSampleCount(FileName);
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

                for (int z = 0; z < count; z++)
                {
                    if (iszero)
                    {
                        endPos = cfprocess.GetAppointEndPostion(FileName, startPos, residue);
                    }
                    else
                    {
                        if (residue == 0)
                        {
                            endPos = cfprocess.GetAppointEndPostion(FileName, startPos, pointCount);
                        }
                        else
                        {
                            if (z == (count - 1))
                            {
                                endPos = cfprocess.GetAppointEndPostion(FileName, startPos, residue);
                            }
                            else
                            {
                                endPos = cfprocess.GetAppointEndPostion(FileName, startPos, pointCount);
                            }
                        }
                    }

                //分段读取方法////////////////////

                    //根据里程list获取里程数组
                    List<Milestone> dualmilelist = cfprocess.GetMileStoneByRange(FileName, startPos, endPos);
                    double[] tt = new double[dualmilelist.Count];
                    for (int i = 0; i < dualmilelist.Count; i++)
                    {
                        double obj = dualmilelist[i].GetMeter() / 1000;
                        tt[i] = obj;
                    }

                    double[] wvelo = cfprocess.GetOneChannelDataInRange(FileName, cdlist.GetChannelIdByName("Speed", "速度"), startPos, endPos);
                    double[] L_Prof_SC = cfprocess.GetOneChannelDataInRange(FileName, cdlist.GetChannelIdByName("L_Prof_SC", "左高低_中波"), startPos, endPos);
                    double[] R_Prof_SC = cfprocess.GetOneChannelDataInRange(FileName, cdlist.GetChannelIdByName("R_Prof_SC", "右高低_中波"), startPos, endPos);
                    double[] L_Align_SC = cfprocess.GetOneChannelDataInRange(FileName, cdlist.GetChannelIdByName("L_Align_SC", "左轨向_中波"), startPos, endPos);
                    double[] R_Align_SC = cfprocess.GetOneChannelDataInRange(FileName, cdlist.GetChannelIdByName("R_Align_SC", "右轨向_中波"), startPos, endPos);
                    double[] Gage = cfprocess.GetOneChannelDataInRange(FileName, cdlist.GetChannelIdByName("Gage", "轨距"), startPos, endPos);
                    double[] Crosslevel = cfprocess.GetOneChannelDataInRange(FileName, cdlist.GetChannelIdByName("Crosslevel", "水平"), startPos, endPos);

                    int tmpChannelNumber = cdlist.GetChannelIdByName("Gage_L", "单边轨距左");
                    double[] Gage_L = null;
                    if (tmpChannelNumber == -1)
                    {
                        Gage_L = new double[wvelo.Length];
                    }
                    else
                    {
                        Gage_L = cfprocess.GetOneChannelDataInRange(FileName, tmpChannelNumber, startPos, endPos);
                    }

                    tmpChannelNumber = cdlist.GetChannelIdByName("Gage_R", "单边轨距右");
                    double[] Gage_R = null;
                    if (tmpChannelNumber == -1)
                    {
                        Gage_R = new double[wvelo.Length];
                    }
                    else
                    {
                        Gage_R = cfprocess.GetOneChannelDataInRange(FileName, tmpChannelNumber, startPos, endPos);
                    }

                    DataProcessing dp = new DataProcessing();
                    //调用刘博士的算法---处理多个通道
                    dp.preProcess(tt, L_Prof_SC, R_Prof_SC, L_Align_SC, R_Align_SC, Gage, Crosslevel, wvelo, Gage_L, Gage_R, FileName, sAddFileName, "自动标识",true);
                
                //分段读取方法////////////////////
                    startPos = endPos;
                }
                //分段读取方法////////////////////
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally
            {

            }
            return true;
        }


        #endregion

        /*
        #region 无效数据滤除--处理多个通道
        /// <summary>
        /// 无效数据滤除--处理多个通道
        /// </summary>
        /// <param name="tt">里程数组：公里</param>
        /// <param name="wx_prof_L">轨道左高低不平顺</param>
        /// <param name="wx_prof_R">轨道右高低不平顺</param>
        /// <param name="wx_align_L">轨道左轨向不平顺</param>
        /// <param name="wx_align_R">轨道右轨向不平顺</param>
        /// <param name="wx_gauge">轨距</param>
        /// <param name="wx_level">水平</param>
        /// <param name="wvelo">速度：km/h</param>
        /// <param name="wgauge_L">单边轨距左</param>
        /// <param name="wgauge_R">单边轨距右</param>
        /// <param name="FileName">cit文件名</param>
        /// <param name="sAddFileName">idf文件名</param>
        /// <param name="swx">英文通道名：这里统一写成"自动标识"</param>
        /// <returns>结果</returns>
        public bool preProcess(double[] tt, double[] wx_prof_L, double[] wx_prof_R, double[] wx_align_L, double[] wx_align_R, double[] wx_gauge, double[] wx_level, double[] wvelo, double[] wgauge_L, double[] wgauge_R, string FileName, string sAddFileName, string swx)
        {
            try
            {
                List<PointIDX> Lidx = new List<PointIDX>();
                //调用刘博士的算法，得到索引数组idx
                int oneTimeLength = 1000000; //一次处理的点数

                for (int i = 0; i < tt.Length; i += oneTimeLength)
                {
                    int remain = 0;
                    int index = (i / oneTimeLength) * oneTimeLength;
                    remain = tt.Length - oneTimeLength * (i / oneTimeLength + 1);
                    int ThisTimeLength = remain > 0 ? oneTimeLength : (remain += oneTimeLength);
                    double[] tmp_tt = new double[ThisTimeLength];
                    double[] tmp_wx_prof_L = new double[ThisTimeLength];
                    double[] tmp_wx_prof_R = new double[ThisTimeLength];
                    double[] tmp_wx_align_L = new double[ThisTimeLength];
                    double[] tmp_wx_align_R = new double[ThisTimeLength];
                    double[] tmp_wx_gauge = new double[ThisTimeLength];
                    double[] tmp_wx_level = new double[ThisTimeLength];
                    double[] tmp_wvelo = new double[ThisTimeLength];

                    double[] tmp_wgauge_L = new double[ThisTimeLength];
                    double[] tmp_wgauge_R = new double[ThisTimeLength];

                    for (int j = 0; j < ThisTimeLength; j++)
                    {
                        tmp_tt[j] = tt[index + j];
                        tmp_wx_prof_L[j] = wx_prof_L[index + j];
                        tmp_wx_prof_R[j] = wx_prof_R[index + j];
                        tmp_wx_align_L[j] = wx_align_L[index + j];
                        tmp_wx_align_R[j] = wx_align_R[index + j];
                        tmp_wx_gauge[j] = wx_gauge[index + j];
                        tmp_wx_level[j] = wx_level[index + j];
                        tmp_wvelo[j] = wvelo[index + j];

                        tmp_wgauge_L[j] = wgauge_L[index + j];
                        tmp_wgauge_R[j] = wgauge_R[index + j];
                    }

                    MWNumericArray d_tt = new MWNumericArray(tmp_tt);
                    MWNumericArray d_wx_prof_L = new MWNumericArray(tmp_wx_prof_L);
                    MWNumericArray d_wx_prof_R = new MWNumericArray(tmp_wx_prof_R);
                    MWNumericArray d_wx_align_L = new MWNumericArray(tmp_wx_align_L);
                    MWNumericArray d_wx_align_R = new MWNumericArray(tmp_wx_align_R);
                    MWNumericArray d_wx_gauge = new MWNumericArray(tmp_wx_gauge);
                    MWNumericArray d_wx_level = new MWNumericArray(tmp_wx_level);
                    MWNumericArray d_wvelo = new MWNumericArray(tmp_wvelo);

                    MWNumericArray d_wgauge_L = new MWNumericArray(tmp_wgauge_L);
                    MWNumericArray d_wgauge_R = new MWNumericArray(tmp_wgauge_R);

                    //调用算法
                    MWNumericArray resultArrayAB = (MWNumericArray)ppmc.sub_identify_abnormal_point(d_tt, d_wx_prof_L, d_wx_prof_R, d_wx_align_L, d_wx_align_R, d_wx_gauge, d_wx_level, d_wvelo, d_wgauge_L, d_wgauge_R);
                    double[,] result = (double[,])resultArrayAB.ToArray();
                    if (result.GetLength(1) == 0) continue;
                    Lidx.Clear();
                    for (int m = 0; m < result.GetLength(0); m++)
                    {
                        PointIDX pi = new PointIDX();
                        pi.s = result[m, 0] + index;
                        pi.e = result[m, 1] + index;
                        pi.type = (int)(result[m, 2]);
                        Lidx.Add(pi);
                    }

                    //按对处理索引数组
                    List<PointIDX>.Enumerator listCredentials = Lidx.GetEnumerator();

                    try
                    {
                        using (OleDbConnection sqlconn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + sAddFileName + ";Persist Security Info=True"))
                        {
                            sqlconn.Open();
                            OleDbCommand sqlcom;

                            string sSql = null;

                            int id = 0;//无效区段id
                            sSql = "select max(Id) from InvalidData";
                            sqlcom = new OleDbCommand(sSql, sqlconn);
                            OleDbDataReader oledbReader = sqlcom.ExecuteReader();
                            Boolean isNull = oledbReader.HasRows;//是否是第一条记录，第一条记录id为1；
                            if (isNull == false)
                            {
                                id = 1;
                            }
                            else
                            {
                                while (oledbReader.Read())
                                {
                                    if (String.IsNullOrEmpty(oledbReader.GetValue(0).ToString()))
                                    {
                                        id = 1;
                                    }
                                    else
                                    {
                                        id = int.Parse(oledbReader.GetValue(0).ToString()) + 1;
                                    }

                                }
                            }

                            while (listCredentials.MoveNext())
                            {
                                //根据索引值获取对应的文件指针。
                                double sPox = cdp.GetPosByIdx(FileName, listCredentials.Current.s);
                                double ePox = cdp.GetPosByIdx(FileName, listCredentials.Current.e);
                                //根据文件指针，获取里程信息。
                                double smile = cdp.GetMileByPos(sAddFileName, FileName, sPox);
                                double emile = cdp.GetMileByPos(sAddFileName, FileName, ePox);
                                int type = listCredentials.Current.GetType();

                                sSql = "insert into InvalidData values(" + (id++).ToString() + ",'" + sPox.ToString() +
    "','" + ePox.ToString() + "','" + smile.ToString() + "','" + emile.ToString() + "'," + type + ",'无效数据',0,'" + swx + "')";

                                //插入数据库
                                sqlcom = new OleDbCommand(sSql, sqlconn);
                                sqlcom.ExecuteNonQuery();
                            }

                            sqlconn.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("无效区段设置异常:" + ex.Message);
                    }
                }
                //InfoLabel2.Visible = false;
            }
            catch (Exception ex)
            {
                throw new Exception(swx + "通道处理出错；" + ex.Source + "；" + ex.StackTrace + "；" + ex.Message);
            }
            return true;
        }

        #endregion

        public class PointIDX
        {
            public double s = 0;
            public double e = 0;
            public int type = 0;

            public new int GetType()
            {
                int retVal = 6;//其他
                if (type == 0)
                {  //速度偏低
                    retVal = 3;//对应于innerdb里的无效区段类型
                }
                if (type == 1)
                {  //分布异常---阳光干扰
                    retVal = 1;
                }
                if (type == 2)
                {  //局部毛刺
                    retVal = 8;
                }
                if (type == 3)
                {  //轨距加宽--加宽道岔
                    retVal = 5;
                }
                if (type == 5)
                {  //单边轨距拉直线
                    retVal = 11;
                }

                return retVal;
            }
        }


        #region 根据索引值获取对应的文件指针。
        public double GetPosByIdx(string sSourceFile, double idx)
        {
            try
            {
                FileStream fs = new FileStream(sSourceFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                BinaryReader br = new BinaryReader(fs, Encoding.Default);
                br.BaseStream.Position = 0;
                br.ReadBytes(120);
                br.ReadBytes(65 * dhi.iChannelNumber);
                br.ReadBytes(BitConverter.ToInt32(br.ReadBytes(4), 0));

                double pos = br.BaseStream.Position + (idx - 1) * 2 * dhi.iChannelNumber;

                br.Close();
                fs.Close();

                return pos;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region 根据文件指针，获取里程信息---20140506--ygx
        /// <summary>
        /// 根据文件指针，获取里程信息---只获取文件中的原始里程
        /// </summary>
        /// <param name="sFile">idf文件名</param>
        /// <param name="sFileName">cit文件名</param>
        /// <param name="Pos">文件指针</param>
        /// <returns>里程：公里</returns>
        public double GetMileByPos(string sFile, string sFileName, double Pos)
        {
            List<DataChannelInfo> m_dciL = GetDataChannelInfoHeadNew(sFileName);
            DataHeadInfo m_dhi = GetDataInfoHeadNew(sFileName);

            try
            {
                double mile = -1;
                double lStartPoint = 0;
                double lStartMeter = 0;
                double lEndPoint = 0;
                double lEndMeter = 0;
                bool ifCorrect = false;
                using (OleDbConnection sqlconn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + sFile + ";Persist Security Info=False"))
                {
                    OleDbCommand sqlcom = new OleDbCommand("select * from IndexOri", sqlconn);
                    sqlconn.Open();
                    OleDbDataReader sqloledr = sqlcom.ExecuteReader();
                    if (sqloledr.Read())
                    {
                        ifCorrect = true;
                    }
                    sqlconn.Close();
                }
                //考虑到无效数据显示的地方有更新里程功能，所以这里只需要取cit原始里程，所以把ifCorrect=false;
                ifCorrect = false;
                if (ifCorrect)
                {
                    List<IndexStaClass> listIC = new List<IndexStaClass>();
                    try
                    {
                        using (OleDbConnection sqlconn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + sFile + ";Persist Security Info=False"))
                        {
                            OleDbCommand sqlcom = new OleDbCommand("select * from IndexSta order by clng(StartPoint)", sqlconn);
                            sqlconn.Open();
                            OleDbDataReader sqloledr = sqlcom.ExecuteReader();
                            while (sqloledr.Read())
                            {
                                IndexStaClass ic = new IndexStaClass();
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

                    if (Pos < listIC[0].lStartPoint)
                    {
                        FileStream fs = new FileStream(sFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        BinaryReader br = new BinaryReader(fs, Encoding.Default);
                        br.BaseStream.Position = Convert.ToInt64(Pos); ;
                        int iChannelNumberSize = m_dhi.iChannelNumber * 2;
                        byte[] b = new byte[iChannelNumberSize];

                        b = br.ReadBytes(iChannelNumberSize);
                        if (dhi.sDataVersion.StartsWith("3."))
                        {
                            b = ByteXORByte(b);
                        }
                        double fGL = (BitConverter.ToInt16(b, 0) / m_dciL[0].fScale) + m_dciL[0].fOffset;
                        if (fGL < 0)
                            fGL = 0;

                        //根据采样点数计算公里数
                        fGL += (((BitConverter.ToInt16(b, 2) / m_dciL[1].fScale + m_dciL[1].fOffset)) / 1000.0);
                        mile = fGL;
                        return mile;
                    }
                    if (Pos > listIC[listIC.Count - 1].lStartPoint)
                    {
                        lStartPoint = listIC[listIC.Count - 1].lStartPoint;
                        lStartMeter = double.Parse(listIC[listIC.Count - 1].lStartMeter);
                        lEndPoint = listIC[listIC.Count - 1].lEndPoint;
                        lEndMeter = double.Parse(listIC[listIC.Count - 1].LEndMeter);
                        mile = ((Pos - lStartPoint) / (lEndPoint - lStartPoint) * (lEndMeter - lStartMeter)) + lStartMeter;
                        return mile;
                    }

                    List<IndexStaClass>.Enumerator listCredentials = listIC.GetEnumerator();
                    while (listCredentials.MoveNext())
                    {
                        if (Pos >= listCredentials.Current.lStartPoint && Pos <= listCredentials.Current.lEndPoint)
                        {
                            lStartPoint = listCredentials.Current.lStartPoint;
                            lStartMeter = double.Parse(listCredentials.Current.lStartMeter);
                            lEndPoint = listCredentials.Current.lEndPoint;
                            lEndMeter = double.Parse(listCredentials.Current.LEndMeter);
                            double Absmile = ((Pos - lStartPoint) / (lEndPoint - lStartPoint) * Math.Abs(lEndMeter - lStartMeter));
                            if (lStartMeter < lEndMeter)
                            {
                                mile = Absmile + lStartMeter;
                            }
                            else
                            {
                                mile = Absmile + lEndMeter;
                            }
                            break;
                        }
                    }
                }
                else
                {
                    FileStream fs = new FileStream(sFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    BinaryReader br = new BinaryReader(fs, Encoding.Default);
                    br.BaseStream.Position = Convert.ToInt64(Pos); ;
                    int iChannelNumberSize = m_dhi.iChannelNumber * 2;
                    byte[] b = new byte[iChannelNumberSize];

                    b = br.ReadBytes(iChannelNumberSize);
                    if (dhi.sDataVersion.StartsWith("3."))
                    {
                        b = ByteXORByte(b);
                    }
                    double fGL = (BitConverter.ToInt16(b, 0) / m_dciL[0].fScale) + m_dciL[0].fOffset;
                    if (fGL < 0)
                        fGL = 0;

                    //根据采样点数计算公里数
                    fGL += (((BitConverter.ToInt16(b, 2) / m_dciL[1].fScale + m_dciL[1].fOffset)) / 1000.0);
                    mile = fGL;
                }

                return mile;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return 0;
            }
        }
        #endregion
        */
    }
}
