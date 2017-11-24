using GeoFileProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using IntegratedDisplayCommon.Model;
using MileageCheckTool.Model;
using MileageCheckTool.Common;
using System.Windows.Forms;
using MileageCheckTool.DAL;
using System.Runtime.InteropServices;

namespace MileageFixGadget
{

    public class MileageGadget
    {
        public const string Constpath = "Result.mdb";
        GeoFileHelper geoHelper = new GeoFileHelper();
        private double totalLength = 0;
        /// <summary>
        ///  //里程准确性评定小工具中计算计算跳变
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(111)]
        public string CalculatJump(string json) {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string geofile = Convert.ToString(obj["geofile"]);
                int jumpvalue = Convert.ToInt32(obj["jumpvalue"].ToString());
                List<JumpPoints> clslist =GadgetList(geofile, jumpvalue);
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
        /// <summary>
        /// 保存索引点
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(112)]
        public string SaveIndexPoint(string json) {
          
            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            try
            {
                //此处需要改动，传入应该是一个list的json
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string geofile = Convert.ToString(obj["geofile"]);
                string LineName = Convert.ToString(obj["LineName"]);//线路名
                string Train = Convert.ToString(obj["Train"]);//轨检车型号
                int jumpvalue = Convert.ToInt32(obj["jumpvalue"].ToString());
                List<JumpPoints> listPoints = GadgetList(geofile, jumpvalue);
                if (listPoints.Count > 0)
                {

                     TotalFileDAL totalFileDal = new TotalFileDAL(Constpath);
                      TotalFile file = new TotalFile();
                    file.TotalLength = totalLength;
                    file.LineName = LineName;
                    if (Train != null)
                    {
                        file.TrainCode = Train;
                    }
                    file.GeoFileName = geofile;
                    file.ResultTableName = "Result" + file.LineName + file.TrainCode + DateTime.Now.ToString("yyyyMMddHHmmss");


                    totalFileDal.Add(file);

                     JumpPointsDAL jumpDal = new JumpPointsDAL(Constpath);

                    jumpDal.CreateTable(Constpath, file.ResultTableName);
                    jumpDal.DeleteAll(file.ResultTableName);
                    for (int i = 0; i < listPoints.Count; i++)
                    {
                        jumpDal.Add(listPoints[i], file.ResultTableName);
                    }
                    resultInfo.flag = 1;
                    resultInfo.msg = "成功";
                    resultInfo.data = Application.StartupPath+"\\"+ Constpath;
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return JsonConvert.SerializeObject(resultInfo);
        }

        //查看左边展示框的列表信息
        [DispId(113)]
        public string BindResult(string json) {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            JObject obj = (JObject)JsonConvert.DeserializeObject(json);
            string geofile = Convert.ToString(obj["geofile"]);
            string LineName = Convert.ToString(obj["LineName"]);
            string Train = Convert.ToString(obj["Train"]);//线路名
            List<TotalFile> totalData = leftlist(Train, LineName, geofile);//方法的调用
            string data = JsonConvert.SerializeObject(totalData);
            resultInfo.flag = 1;
            resultInfo.msg = "";
            resultInfo.data = data;
            return JsonConvert.SerializeObject(resultInfo);
        }

        //查看中的左边的展示框--中list集合的返回
        private List<TotalFile> leftlist(string Train, string txtLineName,string geoFileName) {
            TotalFile file = new TotalFile();
            try
            {
                file.LineName =txtLineName;
                if (Train != null)
                {
                    file.TrainCode = Train.ToString().Replace("-", "_");
                }
                else
                {
                    file.TrainCode = ""; 
                }
                file.GeoFileName = geoFileName;
             
            }
            catch (Exception ex)
            {
                throw ex;
            }
             IOperator _dbOperator = new DbOperator();
            _dbOperator.DbFilePath = Constpath;
            JumpPointsDAL _jumpDal = new JumpPointsDAL(_dbOperator.DbFilePath);
            TotalFileDAL _totalFileDal = new TotalFileDAL(_dbOperator.DbFilePath);
            List<TotalFile> totalData = _totalFileDal.GetList(file.LineName, file.TrainCode);//得到了数据集
            return totalData;

        }
        //查看中左下方的页面的展示
        [DispId(114)]
        public string DataList(string json) {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            JObject obj = (JObject)JsonConvert.DeserializeObject(json);
            string geofile = Convert.ToString(obj["geofile"]);
            string tableName = Convert.ToString(obj["tableName"]);//geo文件名称
            string LineName = Convert.ToString(obj["LineName"]);
            string Train = Convert.ToString(obj["Train"]);//线路名
            List<JumpPoints> jumpPoints = leftBelowList(tableName, Train, LineName, geofile);
            string data = JsonConvert.SerializeObject(jumpPoints);
            resultInfo.flag = 1;
            resultInfo.msg = "";
            resultInfo.data = data;
            return JsonConvert.SerializeObject(resultInfo);
        }

        //查看中左下方的页面的展示     
        private List<JumpPoints> leftBelowList(string tableName, string Train, string txtLineName,string geofile) {//返回的是结果集
            //string tableName = "";//geo文件
            // 
            List<TotalFile> totalData = leftlist(Train, txtLineName, geofile);
            List<JumpPoints> jumpPoints = new List<JumpPoints>();
            if (totalData.Count>0) {
                IOperator _dbOperator = new DbOperator();
                _dbOperator.DbFilePath = Constpath;
                JumpPointsDAL _jumpDal = new JumpPointsDAL(_dbOperator.DbFilePath);
                jumpPoints = _jumpDal.Load(tableName);
            }
            return jumpPoints;
        }
     
        //右边的数据1------最大正向跳变
        [DispId(115)]
        public string MaxForward(string json) {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            JObject obj = (JObject)JsonConvert.DeserializeObject(json);
            string geofile = Convert.ToString(obj["geofile"]);
            string tableName = Convert.ToString(obj["tableName"]);//geo文件名称
            string LineName = Convert.ToString(obj["LineName"]);
            string Train = Convert.ToString(obj["Train"]);//线路名
            List<JumpPoints> jumpPoints = leftBelowList(tableName, Train, LineName, geofile);
            string MaxForward = jumpPoints.Max(p => p.DiffSample).ToString();//最大正向跳变
            resultInfo.flag = 1;
            resultInfo.msg = "";
            resultInfo.data = MaxForward;
            return JsonConvert.SerializeObject(resultInfo); 
           
        }

        //右边的数据2----最大负向跳变
        [DispId(116)]
        public string MaxNegative(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            JObject obj = (JObject)JsonConvert.DeserializeObject(json);
            string geofile = Convert.ToString(obj["geofile"]);
            string tableName = Convert.ToString(obj["tableName"]);//geo文件名称
            string LineName = Convert.ToString(obj["LineName"]);
            string Train = Convert.ToString(obj["Train"]);//线路名
            List<JumpPoints> jumpPoints = leftBelowList(tableName, Train, LineName, geofile);
            string MaxNegative = jumpPoints.Min(p => p.DiffSample).ToString();//最大负向跳变
            resultInfo.flag = 1;
            resultInfo.msg = "";
            resultInfo.data = MaxNegative;
            return JsonConvert.SerializeObject(resultInfo);
        }

        //右边的数据3-----跳变平均值
        [DispId(117)]
        public string Average(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            JObject obj = (JObject)JsonConvert.DeserializeObject(json);
            string geofile = Convert.ToString(obj["geofile"]);
            string tableName = Convert.ToString(obj["tableName"]);//geo文件名称
            string LineName = Convert.ToString(obj["LineName"]);
            string Train = Convert.ToString(obj["Train"]);//线路名
            double totalLength = Convert.ToDouble(obj["totalLength"]);//计算结果表名
            List<JumpPoints> jumpPoints = leftBelowList(tableName, Train, LineName, geofile);
            double arr = jumpPoints.Sum(p => p.DiffSample) / totalLength;
           string Average = arr.ToString("F5");//跳变平均值
            resultInfo.flag = 1;
            resultInfo.msg = "";
            resultInfo.data = Average;
            return JsonConvert.SerializeObject(resultInfo);
        }

        //右边的数据4----//跳变平均绝对值
        [DispId(118)]
        public string AbsoluteAverage(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            JObject obj = (JObject)JsonConvert.DeserializeObject(json);
            string geofile = Convert.ToString(obj["geofile"]);
            string tableName = Convert.ToString(obj["tableName"]);//geo文件名称
            string LineName = Convert.ToString(obj["LineName"]);
            string Train = Convert.ToString(obj["Train"]);//线路名
            List<JumpPoints> jumpPoints = leftBelowList(tableName, Train, LineName, geofile);
            double arr1 = jumpPoints.Sum(p => Math.Abs(p.DiffSample)) / jumpPoints.Count;
            string AbsoluteAverage = arr1.ToString("F5");//跳变平均绝对值
            resultInfo.flag = 1;
            resultInfo.msg = "";
            resultInfo.data = AbsoluteAverage;
            return JsonConvert.SerializeObject(resultInfo);
        }

        //右边数据展示
        [DispId(119)]
        public string DataRight(string json) {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            JObject obj = (JObject)JsonConvert.DeserializeObject(json);
            string geofile = Convert.ToString(obj["geofile"]);
            string tableName = Convert.ToString(obj["tableName"]);//geo文件名称
            string LineName = Convert.ToString(obj["LineName"]);
            string Train = Convert.ToString(obj["Train"]);//线路名
            double totalLength = Convert.ToDouble(obj["totalLength"]);//计算结果表名
            List<JumpPoints> jumpPoints = leftBelowList(tableName, Train, LineName, geofile);
            double arr1 = jumpPoints.Sum(p => Math.Abs(p.DiffSample)) / jumpPoints.Count;
            double arr = jumpPoints.Sum(p => p.DiffSample) / totalLength;
            string AbsoluteAverage = arr1.ToString("F5");//跳变平均绝对值
            string Average = arr.ToString("F5");//跳变平均值
            string MaxNegative = jumpPoints.Min(p => p.DiffSample).ToString();//最大负向跳变
            string MaxForward = jumpPoints.Max(p => p.DiffSample).ToString();//最大正向跳变
            List<string> datali = new List<string>();
            datali.Add(AbsoluteAverage);
            datali.Add(Average);
            datali.Add(MaxNegative);
            datali.Add(MaxForward);
            string data = JsonConvert.SerializeObject(datali);//序列化
            resultInfo.flag = 1;
            resultInfo.msg = "";
            resultInfo.data = data;
            return JsonConvert.SerializeObject(resultInfo);
        }

        //返回结果集---第一个页面
        public List<JumpPoints> GadgetList(string geoFilePath, int numUDJumpValue)
        {
            List<JumpPoints> listPoints = new List<JumpPoints>();
            List<int[]> dataList = geoHelper.GetMileChannelData(geoFilePath);

            totalLength = (dataList[1].Length - 1) * 0.25 / 1000;


            double km_pre = 0;
            double meter_pre = 0;
            double km_currrent = 0;
            double meter_current = 0;
            double mileage_between = 0;
            double meter_between = 0;

            double mileage_pre = 0;
            double mileage_current = 0;

            double forwardValue = 0;
            double backValue = 0;

            try
            {
                if (dataList != null && dataList.Count > 0)
                {
                    listPoints.Clear();
                    int index = 0;
                    for (int i = 0; i < dataList[0].Length; i++)
                    {
                        if (i == 0)
                        {
                            km_pre = dataList[0][i];
                            meter_pre = dataList[1][i];
                        }
                        else
                        {
                            km_currrent = dataList[0][i];
                            meter_current = dataList[1][i];

                            mileage_pre = km_pre * 1000 + meter_pre * 0.25;
                            mileage_current = km_currrent * 1000 + meter_current * 0.25;

                            mileage_between = mileage_current - mileage_pre;
                            meter_between = meter_current - meter_pre;

                            if (Math.Abs(mileage_between) > Convert.ToDouble(numUDJumpValue))//与跳变容许值进行比较
                            {
                                if (mileage_between > forwardValue)
                                {
                                    forwardValue = mileage_between;
                                }
                                if (mileage_between < 0 && Math.Abs(mileage_between) > backValue)
                                {
                                    backValue = Math.Abs(mileage_between);
                                }

                                JumpPoints point = new JumpPoints();
                                point.ID = ++index;
                                point.CurrentMileage = km_currrent;
                                point.CurrentSample = meter_current;
                                point.LastMileage = km_pre;
                                point.LastSample = meter_pre;
                                point.DiffMileage = mileage_between;
                                point.DiffSample = meter_between;

                                listPoints.Add(point);
                            }

                            km_pre = km_currrent;
                            meter_pre = meter_current;
                            int value = i * 80 / dataList[0].Length;
                            if (value != 0 && (value % 20 == 0) || i == dataList[0].Length - 1)
                            {
                               

                            }
                        }

                    }

                }
                else
                {
                    //MessageBox.Show("没有获取到Geo通道数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                   // return;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return listPoints;

        }

    }
}
