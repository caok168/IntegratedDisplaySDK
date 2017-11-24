using CitFileSDK;
using GeoFileProcess;
using IntegratedDisplayCommon.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GeoToCit
{
    public class GeoToCit
    {
        GeoFileHelper gfprocess = new GeoFileHelper();
        //StreamWriter sw = new StreamWriter("D:/info.txt", true, Encoding.Default);

        /// <summary>
        /// geo转cit
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(71)]
        public string geotocit(string json)
        {

            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string lineCode = Convert.ToString(obj["lineCode"]);
                string lineName = Convert.ToString(obj["lineName"]);
                string train = Convert.ToString(obj["train"]);
                int kmInc = Convert.ToInt32(String.IsNullOrEmpty(obj["kmInc"].ToString()) ? "0" : obj["kmInc"].ToString());//增减里程
                int runDir = Convert.ToInt32(String.IsNullOrEmpty(obj["runDir"].ToString()) ? "0" : obj["runDir"].ToString());//检测方向
                int dir = Convert.ToInt32(String.IsNullOrEmpty(obj["dir"].ToString()) ? "0" : obj["dir"].ToString());//行别
                string geoFile = Convert.ToString(obj["geoFile"]);
                string citFile = Convert.ToString(obj["citFile"]);
                string csvFilePath = getCsvFilePath(Convert.ToString(obj["csvFilePath"]),train);
                string citFilePath = Path.GetFileNameWithoutExtension(geoFile) + ".cit";
                String geoFileNew = Path.Combine(Path.GetDirectoryName(geoFile), citFilePath);

                //sw.WriteLine("lineCode：" + lineCode);
                //sw.WriteLine("lineName：" + lineName);
                //sw.WriteLine("train：" + train);
                //sw.WriteLine("kmInc：" + kmInc);
                //sw.WriteLine("runDir：" + runDir);
                //sw.WriteLine("dir：" + dir);
                //sw.WriteLine("geoFile：" + geoFile);
                //sw.WriteLine("citFile：" + citFile);
                //sw.WriteLine("csvFilePath：" + csvFilePath);

                //这里获取头部部分的信息
                int kmIncNew = 0;
                int runDirNew = 0;
                int dirNew = 0;
                string[] sDHI = Path.GetFileNameWithoutExtension(geoFile).Split('-');
                string lineShortName = sDHI[0].ToUpper();
                gfprocess.QueryDataChannelInfoHead(geoFile);
                string mileageRange = gfprocess.GetExportDataMileageRange(geoFile);
                mileageRange = mileageRange.Substring(2);
                float startMileage = float.Parse(mileageRange.Substring(0, mileageRange.IndexOf("-")));//开始里程
                float endMileage = float.Parse(mileageRange.Substring(mileageRange.IndexOf("-") + 1));//结束里程
                if (startMileage < endMileage)
                {
                    kmIncNew = 0;
                    runDirNew = 0;
                }
                else
                {
                    kmIncNew = 1;
                    runDirNew = 1;
                }
                //行别 1：上行，2：下行，3：单线
                if (lineShortName.Substring(3, 1).Equals("X"))
                {//下
                    dirNew = 1;
                }
                else if (lineShortName.Substring(3, 1).Equals("S"))
                {//上
                    dirNew = 2;
                }
                else
                {//单
                    dirNew = 3;
                }
                if (String.IsNullOrEmpty(citFile))
                {//判断citfile路径是否为空
                    citFile = geoFileNew;
                }
                if (kmInc == 0)
                {
                    kmInc = kmIncNew;
                }
                if (runDir == 0)
                {
                    runDir = runDirNew;
                }
                if (dir == 0)
                {
                    dir = dirNew;
                }

                string time = null;
                string date = null;
                try
                {
                    time = String.Format("{0}:{1}:{2}", sDHI[4].Substring(0, 2), sDHI[4].Substring(2, 2), sDHI[4].Substring(4, 2));//4:31

                }
                catch (Exception)
                {
                    time = null;
                }
                try
                {
                    date = String.Format("{0}/{1}/{2}", sDHI[3].Substring(2, 2), sDHI[3].Substring(0, 2), sDHI[3].Substring(4, 4));//2106/5/21
                }
                catch (Exception)
                {
                    date = null;
                }

                FileInformation citFileHeader = new FileInformation();
                citFileHeader = gfprocess.GetFileHeadInfo(lineCode, lineName, train, kmInc, runDir, dir);

                //sw.WriteLine("写入时间前date：" + date);
                //sw.WriteLine("写入时间前time：" + time);

                if (time != null)
                {
                    citFileHeader.sTime = time;
                }
                if (date != null)
                {
                    citFileHeader.sDate = date;
                }

                //sw.WriteLine("写入时间后的citFileHeader：" + citFileHeader.sDate);
                //sw.WriteLine("写入时间后citFileHeader.sTime：" + citFileHeader.sTime);

                gfprocess.InitChannelMapping(csvFilePath);//初始化端口
                bool result = gfprocess.ConvertData(geoFile, citFile, citFileHeader);
                if (result)
                {
                    resultInfo.flag = 1;
                    resultInfo.msg = "";
                    resultInfo.data = citFile;
                }
                else
                {
                    resultInfo.flag = 0;
                    resultInfo.msg = "";
                    resultInfo.data = "转化失败";
                }
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            //sw.Close();

            return JsonConvert.SerializeObject(resultInfo);
        }


        /// <summary>
        /// 线路代码和配置文件集合
        /// </summary>
        private string getCsvFilePath(string configPath, string train)
        {
            Dictionary<string, string> dicTrainCodeAndConfigPath = new Dictionary<string, string>();
            dicTrainCodeAndConfigPath.Clear();
            //string runPath = System.Environment.CurrentDirectory;
            //string configPath = Path.Combine(runPath, "GEOConfig");
            string[] configFiles = Directory.GetFiles(configPath, "*.csv", SearchOption.AllDirectories);
            foreach (string configFile in configFiles)
            {
                dicTrainCodeAndConfigPath.Add(Path.GetFileNameWithoutExtension(configFile), configFile);
            }
            
            var query = from d in dicTrainCodeAndConfigPath
                        where d.Key == train
                        select d.Value;

            string defaultPath = Path.Combine(configPath, "GJ-6.csv");
            string dValue = query.FirstOrDefault() != null ? query.FirstOrDefault() : defaultPath;

            return dValue;
        }



    }
}

