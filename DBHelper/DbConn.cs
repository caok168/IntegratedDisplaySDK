using IntegratedDisplayCommon.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace DBHelper
{
    public class DbConn
    {
        //判断是不是存在当前路径名称的数据库
        [DispId(151)]
        public string IsExistAccessDb(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string filePath = Convert.ToString(obj["filePath"]);
                bool y=DataAccess.AccessHelper.IsExistAccessDb(filePath);
                if (y)
                {
                    resultInfo.flag = 1;
                    resultInfo.msg = "数据库已存在";
                    resultInfo.data = filePath;
                }
                else
                {
                    resultInfo.flag = 0;
                    resultInfo.msg = "数据库不存在";
                    resultInfo.data = "";
                }
                
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(resultInfo);
        }
        //创建access数据库 （数据库文件名将以 Data+线路名+运行日期+NO+ID）

        [DispId(152)]
        public string CreateAccessDb(string json) {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string filePath = Convert.ToString(obj["filePath"]);
                bool y = DataAccess.AccessHelper.CreateAccessDb(filePath);
                if (y)
                {
                    resultInfo.flag = 1;
                    resultInfo.msg = "创建成功";
                    resultInfo.data = filePath;
                }
                else
                {
                    resultInfo.flag = 0;
                    resultInfo.msg = "创建失败";
                    resultInfo.data = "";
                }
                
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(resultInfo);
        }

        private ADOX.DataTypeEnum comm(string type) {
            ADOX.DataTypeEnum comm=new ADOX.DataTypeEnum();
            if (type.Equals("vachar"))
            {
                comm = ADOX.DataTypeEnum.adVarChar;
            }
            else if (type == "int" || type.Equals("Integer"))
            {
                comm = ADOX.DataTypeEnum.adInteger;
            }
            else if (type.Equals("double"))
            {
                comm = ADOX.DataTypeEnum.adDouble;
            }
            else {
                comm = ADOX.DataTypeEnum.adIUnknown;
            }
            return comm;
        }

        // 创建数据库表
        [DispId(153)]
        public string CreateAccessTable(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string filePath = Convert.ToString(obj["filePath"]);
                string tableName = Convert.ToString(obj["tableName"]);
                string colums = Convert.ToString(obj["colums"]);//bool
                JArray obj1 = (JArray)JsonConvert.DeserializeObject(colums);
                int sum = obj1.Count();
                ADOX.Column[] csf = new ADOX.Column[sum];
                for (int i = 0; i < sum; i++)
                {
                    string name = Convert.ToString(obj1[i]["Name"].ToString());
                    string type = Convert.ToString(obj1[i]["Type"].ToString());
                    int definedSize = Convert.ToInt32(obj1[i]["DefinedSize"].ToString());

                    csf[i]= new ADOX.Column() { Name = name, Type = comm(type), DefinedSize = definedSize };
                }
              
                bool isContainKeyID = Convert.ToBoolean(obj["isContainKeyID"].ToString());

                // ADOX.Column[] waveDataColumns;
                DataAccess.AccessHelper.CreateAccessTable(filePath, tableName, csf, isContainKeyID);

                resultInfo.flag = 1;
                resultInfo.msg = "";
                resultInfo.data = filePath;
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }
    
            return JsonConvert.SerializeObject(resultInfo);
        }

        //判断数据库中是否存在指定的表名
        [DispId(154)]
        public string IsExistAccessTable(string json) {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string filePath = Convert.ToString(obj["filePath"]);
                string tableName = Convert.ToString(obj["tableName"]);
                bool y = DataAccess.AccessHelper.IsExistAccessTable(filePath, tableName);
                if (y)
                {
                    resultInfo.flag = 1;
                    resultInfo.msg = "数据库中已存在当前库表";
                    resultInfo.data = filePath;
                }
                else
                {
                    resultInfo.flag = 0;
                    resultInfo.msg = "数据库中不存在当前库表";
                    resultInfo.data = "";
                }
                
            }
            catch (Exception ex)
            {
                resultInfo.flag = 0;
                resultInfo.msg = ex.Message;
            }

            return JsonConvert.SerializeObject(resultInfo);
        }
        //获取指定数据库的所有的表结构信息

        [DispId(155)]
        public string GetAccessTables(string json) {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string filePath = Convert.ToString(obj["filePath"]);
                DataTable table = DataAccess.AccessHelper.GetAccessTables(filePath);//数据库中所有表的信息
                string data = JsonConvert.SerializeObject(table);
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

        //运行OleDb语句
        [DispId(156)]
        public string Run_SQL(string json) {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string SQL = Convert.ToString(obj["SQL"]);
                string ConnStr = Convert.ToString(obj["ConnStr"]);
                int data = DataAccess.AccessHelper.Run_SQL(SQL, ConnStr);
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

        //获取自动增长列ID
        [DispId(157)]
        public string GetInsertID(string json) {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string SQL = Convert.ToString(obj["SQL"]);
                string ConnStr = Convert.ToString(obj["ConnStr"]);
                int data = DataAccess.AccessHelper.GetInsertID(SQL, ConnStr);
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


        //运行OleDb语句返回 DataTable
        [DispId(158)]
        public string Get_DataTable(string json) {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string SQL = Convert.ToString(obj["SQL"]);
                string ConnStr = Convert.ToString(obj["ConnStr"]);
                string Table_name = Convert.ToString(obj["Table_name"]);
                DataTable dd= DataAccess.AccessHelper.Get_DataTable(SQL, ConnStr, Table_name);
                string data = JsonConvert.SerializeObject(dd);
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

        ////运行OleDb语句,返回DataSet对象
        //public string Get_DataSet(string json) {
        //    //公共方法
        //    ResultInfo resultInfo = new ResultInfo();
        //    try
        //    {
        //        JObject obj = (JObject)JsonConvert.DeserializeObject(json);
        //        string SQL = Convert.ToString(obj["SQL"]);
        //        string ConnStr = Convert.ToString(obj["ConnStr"]);
        //        string Ds = Convert.ToString(obj["Ds"]);
        //        DataSet d = Ds;
        //        DataSet dd = DataAccess.AccessHelper.Get_DataSet(SQL, ConnStr, Ds);
        //        string data = JsonConvert.SerializeObject(dd);
        //        resultInfo.flag = 1;
        //        resultInfo.msg = "";
        //        resultInfo.data = data;
        //    }
        //    catch (Exception ex)
        //    {
        //        resultInfo.flag = 0;
        //        resultInfo.msg = ex.Message;
        //    }

        //    return JsonConvert.SerializeObject(resultInfo);
        //}
        //运行OleDb语句,返回DataSet对象
        //public string Get_DataSetName(string json)
        //{   
        //    //公共方法
        //    ResultInfo resultInfo = new ResultInfo();
        //    try
        //    {
        //        JObject obj = (JObject)JsonConvert.DeserializeObject(json);
        //        string SQL = Convert.ToString(obj["SQL"]);
        //        string ConnStr = Convert.ToString(obj["ConnStr"]);
        //        string Ds = Convert.ToString(obj["Ds"]);
        //        string tablename = Convert.ToString(obj["tablename"]);
        //        DataSet d = Ds;
        //        DataSet dd = DataAccess.AccessHelper.Get_DataSet(SQL, ConnStr, Ds,tablename);
        //        string data = JsonConvert.SerializeObject(dd);
        //        resultInfo.flag = 1;
        //        resultInfo.msg = "";
        //        resultInfo.data = data;
        //    }
        //    catch (Exception ex)
        //    {
        //        resultInfo.flag = 0;
        //        resultInfo.msg = ex.Message;
        //    }

        //    return JsonConvert.SerializeObject(resultInfo);
        //}
        //运行OleDb语句,返回DataSet对象，将数据进行了分页
        //public string Get_DataSetPaging(string json) {

        //    //公共方法
        //    ResultInfo resultInfo = new ResultInfo();
        //    try
        //    {
        //        JObject obj = (JObject)JsonConvert.DeserializeObject(json);
        //        string SQL = Convert.ToString(obj["SQL"]);
        //        string ConnStr = Convert.ToString(obj["ConnStr"]);
        //        string Ds = Convert.ToString(obj["Ds"]);
        //        string tablename = Convert.ToString(obj["tablename"]);
        //        int PageSize = Convert.ToInt32(obj["PageSize"].ToString());
        //        int StartIndex = Convert.ToInt32(obj["StartIndex"].ToString());
        //        DataSet ds = new DataSet();
        //        ds.Namespace = Ds;
        //        DataSet dd = DataAccess.AccessHelper.Get_DataSet(SQL, ConnStr, Ds, StartIndex, PageSize, tablename);
        //        string data = JsonConvert.SerializeObject(dd);
        //        resultInfo.flag = 1;
        //        resultInfo.msg = "";
        //        resultInfo.data = data;
        //    }
        //    catch (Exception ex)
        //    {
        //        resultInfo.flag = 0;
        //        resultInfo.msg = ex.Message;
        //    }

        //    return JsonConvert.SerializeObject(resultInfo);
        //}
        //返回OleDb语句执行结果的第一行第一列



        //返回OleDb语句执行结果的第一行第一列
        [DispId(159)]
        public string Get_Row1_Col1_Value(string json) {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string SQL = Convert.ToString(obj["SQL"]);
                string ConnStr = Convert.ToString(obj["ConnStr"]);
                string data = DataAccess.AccessHelper.Get_Row1_Col1_Value(SQL, ConnStr);
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
        
        //返回OleDb语句执行结果的第一行第一列
        private string Get_Adapter(string json)
        {
            //公共方法
            ResultInfo resultInfo = new ResultInfo();
            try
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(json);
                string SQL = Convert.ToString(obj["SQL"]);
                string ConnStr = Convert.ToString(obj["ConnStr"]);
                OleDbDataAdapter y = DataAccess.AccessHelper.Get_Adapter(SQL, ConnStr);
                string data = JsonConvert.SerializeObject(y);
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
