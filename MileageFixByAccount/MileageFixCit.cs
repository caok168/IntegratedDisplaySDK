using CitFileProcess;
using IntegratedDisplayCommon.Model;
using InvalidDataProcessing;
using MathWorks.MATLAB.NET.Arrays;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MileageFixByAccount
{
    public class MileageFixCit
    {
        CitFileHelper citHelper = new CitFileHelper();

        /// <summary>
        /// 根据相应台账修改cit文件
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(101)]
        public string FixCit(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                //cit文件路径
                string citFile = Convert.ToString(obj["citFile"]);
                //曲线台账模板
                string curveFile = Convert.ToString(obj["curveFile"]);
                //长短链模板
                string abruptMileFile = Convert.ToString(obj["abruptMileFile"]);
                //采样频率
                int fs = Convert.ToInt32(obj["fs"].ToString());
                //超高控制阈值
                double thresh_curve = Convert.ToDouble(obj["thresh_curve"].ToString());
                //将文件按传入点位数分段读取
                int pointCount = Convert.ToInt32(obj["pointCount"].ToString());

                string data = _fixCit(citFile, curveFile, abruptMileFile, fs, thresh_curve, pointCount);
                
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

        private string _fixCit(string citFilePath, string curveFilePath, string abruptMileFilePath, int fs, double thresh_curve, int pointCount)
        {
            CalculateCorrugationClass calculateCorrugation = new CalculateCorrugationClass();

            var header = citHelper.GetDataInfoHead(citFilePath);
            var channelList = citHelper.GetDataChannelInfoHead(citFilePath);

            string correctMileFilePath = citFilePath.Substring(0, citFilePath.Length - 4) + "correctMileStone.cit";
            CreateCitHeader(correctMileFilePath, header, channelList);

            long startPos = citHelper.GetSamplePointStartOffset(header.iChannelNumber);
            long endPos = 0;
            
            //点位数
            int sampleNum = Convert.ToInt32((citHelper.GetFileLength(citFilePath) - startPos) / (header.iChannelNumber * 2));

            //循环次数
            int count = Convert.ToInt32(sampleNum / pointCount);
            //是否有余点
            int residue = Convert.ToInt32(sampleNum % pointCount);

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

            double[,] wcCurveData; //台账曲率
            double[,] abruptMileData;//长短链

            wcCurveData = GetData(curveFilePath);
            abruptMileData = GetData(abruptMileFilePath);

            //if (count == 0) count = 1;
            for (int k = 0; k < count; k++)
            {
                List<double[]> dataList = null;

                if (iszero)
                {
                    dataList = citHelper.GetAllChannelDataInRange(citFilePath, startPos, residue, ref endPos);
                }
                else
                {
                    if (residue == 0)
                    {
                        dataList = citHelper.GetAllChannelDataInRange(citFilePath, startPos, pointCount, ref endPos);
                    }
                    else
                    {
                        if (k == (count - 1))
                        {
                            dataList = citHelper.GetAllChannelDataInRange(citFilePath, startPos, residue, ref endPos);
                        }
                        else
                        {
                            dataList = citHelper.GetAllChannelDataInRange(citFilePath, startPos, pointCount, ref endPos);
                        }
                    }
                }

                List<double[]> dataList_input = new List<double[]>();
                dataList_input.Add(dataList[0]);
                dataList_input.Add(dataList[1]);
                dataList_input.Add(dataList[7]);

                MWNumericArray array = calculateCorrugation.GetProcessAbnormalDispResultProcess(dataList_input);

                List<double[]> listData = calculateCorrugation.GetProcessAbnormalDispResult(array);


                double[] mileData;
                double[] curveData;//超高

                mileData = listData[0];
                curveData = listData[1];


                MWNumericArray array2 = calculateCorrugation.GetVerifyKilometerResultProcess(mileData, curveData, wcCurveData, abruptMileData, fs, thresh_curve);

                var correctMileData = calculateCorrugation.GetVerifyKilometerResult(array2);

                double[] kmData = new double[correctMileData.Length];
                double[] mData = new double[correctMileData.Length];

                for (int i = 0; i < correctMileData.Length; i++)
                {
                    kmData[i] = correctMileData[i] / 1000;
                    mData[i] = correctMileData[i] % 1000;
                }

                dataList[0] = kmData;
                dataList[1] = mData;

                CreateCitData(correctMileFilePath, dataList);

                startPos = endPos;
            }
            
            return correctMileFilePath;
        }


        /// <summary>
        /// 创建cit文件头
        /// </summary>
        /// <param name="citFileName"></param>
        /// <param name="dataHeadInfo"></param>
        /// <param name="channelList"></param>
        private void CreateCitHeader(string citFileName, DataHeadInfo dataHeadInfo, List<DataChannelInfo> channelList)
        {
            citHelper.WriteCitFileHeadInfo(citFileName, dataHeadInfo, channelList);

            citHelper.WriteDataExtraInfo(citFileName, "");
        }

        /// <summary>
        /// 写入cit数据
        /// </summary>
        /// <param name="citFileName"></param>
        /// <param name="dataList"></param>
        private void CreateCitData(string citFileName, List<double[]> dataList)
        {
            citHelper.WriteChannelData(citFileName, dataList);
        }

        /// <summary>
        /// 获取模板数据
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private double[,] GetData(string filePath)
        {
            List<double[]> list = new List<double[]>();

            StreamReader sr = new StreamReader(filePath);

            while (true)
            {
                string str = sr.ReadLine();
                if (!string.IsNullOrEmpty(str))
                {
                    str = str.TrimStart();

                    str = str.Replace("\t", " ");
                    str = str.Replace("  ", ",");
                    str = str.Replace(" ", ",");

                    string[] datas = str.Split(',');
                    int length = datas.Length;
                    if (length > 0)
                    {
                        double[] darray = new double[length];
                        for (int i = 0; i < darray.Length; i++)
                        {
                            darray[i] = Convert.ToDouble(datas[i]);
                        }
                        list.Add(darray);
                    }
                }
                else
                {
                    break;
                }
            }
            int count = 0;
            int listLength = list.Count;
            if (list.Count > 0)
            {
                count = list[0].Length;
            }
            double[,] data = new double[listLength, count];

            for (int i = 0; i < listLength; i++)
            {
                for (int j = 0; j < count; j++)
                {
                    data[i, j] = list[i][j];
                }
            }

            return data;
        }

    }
}
