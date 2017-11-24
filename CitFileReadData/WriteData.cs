using CitFileSDK;
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
/// 说    明：17  数据写模块(数据源可能是CIT, 也可能是别的文件类型，如txt, bny等) 目前只考虑CIT一种情况
/// Version ：2.0
/// Date    ：2017/8/29
/// Author  ：Qinh
/// -------------------------------------------------------------------------------------------


namespace CitFileReadData
{
    public class WriteData
    {
        // CIT文件相关操作类
        CITFileProcess cfprocess = new CITFileProcess();

        #region  ChannelDefinition 

        /// <summary>
        /// 将通道定义信息写入文件中
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(50)]
        public string WriteChannelDefinitionList(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                List<ChannelDefinition> channellist = JsonConvert.DeserializeObject < List <ChannelDefinition>>(obj["channelList"].ToString());

                bool result = cfprocess.WriteChannelDefinitionList(citFile, channellist);
                resultInfo.flag = 1;
                resultInfo.msg = "";
                resultInfo.data = result.ToString();
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(resultInfo);
        }

        #endregion


        #region 写入附加信息

        /// <summary>
        /// 向文件中写入文件头补充信息
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(51)]
        public string WriteExtraInfo(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);
                string info = Convert.ToString(obj["info"]);

                bool data = cfprocess.WriteExtraInfo(citFile, info);
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


        #region citfilemodify

        /// <summary>
        /// 把单行线都统一为增里程(包括正方向和反方向)
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(52)]
        public string ModifyCitMergeKmInc(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);

                cfprocess.ModifyCitMergeKmInc(citFile);
                resultInfo.flag = 1;
                resultInfo.msg = "";
                resultInfo.data = "";
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(resultInfo);
        }

        /// <summary>
        /// 把反方向检测转换为正方向检测
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(53)]
        public string ModifyCitReverseToForward(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);

                cfprocess.ModifyCitReverseToForward(citFile);
                resultInfo.flag = 1;
                resultInfo.msg = "";
                resultInfo.data = "";
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(resultInfo);
        }

        /// <summary>
        /// 根据文件里的里程数据判断增减
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(54)]
        public string IsCitKmInc(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string citFile = Convert.ToString(obj["citFile"]);

                bool result=cfprocess.IsCitKmInc(citFile);
                resultInfo.flag = 1;
                resultInfo.msg = "";
                resultInfo.data = result.ToString();
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(resultInfo);
        }

        #endregion


        #region citfilewrite

        /// <summary>
        /// 向cit文件中写入文件头、数据块 包括参数：cit文件路径、文件信息、通道定义集合、补充信息、通道数据数组集合
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(55)]
        public string WriteCitFile(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);

                string citFile = Convert.ToString(obj["citFile"]);
                FileInformation fi = JsonConvert.DeserializeObject<FileInformation>(obj["fi"].ToString());
                List<ChannelDefinition> channelList = JsonConvert.DeserializeObject<List<ChannelDefinition>>(obj["channelList"].ToString());
                string extraInfo = Convert.ToString(obj["extraInfo"]);
                List<double[]> arrayDone = JsonConvert.DeserializeObject<List<double[]>>(obj["arrayDone"].ToString());

                bool data =cfprocess.WriteCitFile(citFile, fi, channelList, extraInfo, arrayDone);

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
        /// 向cit文件中写入文件头信息
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(56)]
        public string WriteCitFileHead(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);

                string citFile = Convert.ToString(obj["citFile"]);
                FileInformation fi = JsonConvert.DeserializeObject<FileInformation>(obj["fi"].ToString());

                bool data = cfprocess.WriteCitFileHead(citFile, fi);

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
        /// 写入通道定义信息
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(57)]
        public string WriteCitChannelDefintion(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);

                string citFile = Convert.ToString(obj["citFile"]);
                List<ChannelDefinition> channelList = JsonConvert.DeserializeObject<List<ChannelDefinition>>(obj["channelList"].ToString());

                bool data = cfprocess.WriteCitChannelDefintion(citFile,channelList);

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
        /// 写入cit文件附加信息
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(58)]
        public string WriteCitExtraInfo(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);

                string citFile = Convert.ToString(obj["citFile"]);
                string extraInfo = Convert.ToString(obj["extraInfo"]);

                bool data = cfprocess.WriteCitExtraInfo(citFile, extraInfo);

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
        /// 写入通道数据
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [DispId(59)]
        public string WriteCitChannelData(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();

            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);

                string citFile = Convert.ToString(obj["citFile"]);
                List<double[]> channelData = JsonConvert.DeserializeObject<List<double[]>>(obj["channelData"].ToString());

                bool data = cfprocess.WriteCitChannelData(citFile, channelData);

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

        #region 测试调用结果
        /// <summary>
        /// 链接测试使用
        /// </summary>
        /// <returns></returns>
        [DispId(60)]
        public string testConnect()
        {
            return "ConnectSuccess";
        }

        #endregion
    }
}
