using CitIndexFileSDK.IntelligentMileageFix;
using IntegratedDisplayCommon.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MileageFix
{
    public class CorrectionIdf
    {
        /// <summary>
        /// 里程相关行修正
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(86)]
        public string Correction(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                //修正后的cit文件路径
                string citFile = Convert.ToString(obj["citFile"]);
                //修正后的idf文件路径
                string idfFile = Convert.ToString(obj["idfFile"]);

                //要处理的cit文件路径的集合
                string[] lstCitFiles = JsonConvert.DeserializeObject<string[]>(obj["lstCitFiles"].ToString());
                //超高门阚值
                float superelevation = Convert.ToSingle(obj["superelevation"].ToString());
                //轨距门阚值
                float gage = Convert.ToSingle(obj["gage"].ToString());
                //左高低门阚值
                float LProf = Convert.ToSingle(obj["LProf"].ToString());
                //右高低门阚值
                float RProf = Convert.ToSingle(obj["RProf"].ToString());
                //原始数据点
                int fixedCount = Convert.ToInt32(obj["fixedCount"].ToString());
                //目标数据点
                int targetCount = Convert.ToInt32(obj["targetCount"].ToString());

                IntelligentMilestoneFix fix = new IntelligentMilestoneFix();

                fix.FixedSamplingCount = fixedCount;
                fix.TargetSamplingCount = targetCount;

                fix.FixParams.Add(new FixParam() { ChannelName = "Gage", ThreShold = gage, Priority = 1 });
                fix.FixParams.Add(new FixParam() { ChannelName = "Superelevation", ThreShold = superelevation, Priority = 0 });
                fix.FixParams.Add(new FixParam() { ChannelName = "L_Prof_SC", ThreShold = LProf, Priority = 2 });
                fix.FixParams.Add(new FixParam() { ChannelName = "R_Prof_SC", ThreShold = RProf, Priority = 3 });

                fix.InitFixData(citFile, idfFile, true);
                
                try
                {
                    /*
                    foreach (string path in lstCitFiles)
                    {
                        bool exec=fix.RunMilestoneFix(path, true);
                        break;
                    }
                    */
                    List<string> successlist = new List<string>();
                    List<string> faillist = new List<string>();
                    foreach (string path in lstCitFiles)
                    {
                        //string path = lstCitFiles[0];
                        bool exec = fix.RunMilestoneFix(path, true);
                        string targetIdf = path.Replace(".cit", "_MileageFix.idf");
                        if (exec)
                        {
                            successlist.Add(targetIdf);
                        }
                        else
                        {
                            faillist.Add(path);
                        }
                    }

                    if (faillist.Count>0)
                    {
                        resultInfo.flag = 1;
                        resultInfo.msg ="修正失败："+JsonConvert.SerializeObject(faillist);
                        resultInfo.data = JsonConvert.SerializeObject(successlist);
                    }
                    else
                    {
                        resultInfo.flag = 1;
                        resultInfo.msg = "";
                        resultInfo.data = JsonConvert.SerializeObject(successlist);
                    }
                    
                }
                catch (Exception ex)
                {
                    resultInfo.flag = 0;
                    resultInfo.msg = ex.ToString();
                    return JsonConvert.SerializeObject(resultInfo);
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

    }
}
