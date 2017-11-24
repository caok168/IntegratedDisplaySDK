using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            SmallChangeTest();
        }

        /// <summary>
        /// 微小变化识别
        /// </summary>
        static void SmallChangeTest()
        {
            SmallChangeIdentify.ChangeDetection changeObj = new SmallChangeIdentify.ChangeDetection();

            Item item = new Item();
            item.citFile = "";
            item.citFile2 = "";
            item.idfFile = "";
            item.idfFile2 = "";
            item.innerdbpath = "";
            item.pointCount = 1;
            item.isCorrect = true;

            string json = JsonConvert.SerializeObject(item);

            json = "{\"baseFileId\":\"ITrVxjQoeG\",\"relaterepairType\":\"5\",\"citFile2\":\"H:/temp/smallchange/GJHS-SHANGHAI-BEIJING-05012017-181348-1.cit\",\"serviceID\":\"SmallChange\",\"baserepairType\":\"4\",\"citFile\":\"H:/temp/smallchange/GJHS-SHANGHAI-BEIJING-13012017-124459-1.cit\",\"isCorrect\":\"true\",\"innerdbpath\":\"H:/temp/smallchange/InnerDB.idf\",\"user\":\"王陆\",\"idfFile\":\"H:/temp/smallchange/GJHS-SHANGHAI-BEIJING-13012017-124459-1_MileageFix.idf\",\"pointCount\":\"100000\",\"idfFile2\":\"H:/temp/smallchange/GJHS-SHANGHAI-BEIJING-05012017-181348-1_MileageFix.idf\"}";

            string result = changeObj.SmallChange(json);
        }

        static void Test1()
        {
            Item item = new Item();
            item.citFile = @"H:\工作文件汇总\铁科院\程序\轨检\data\GNHS-HANGZHOU-NANJING-14052016-175302-1减变增.cit";
            //item.idfFile = @"H:\工作文件汇总\铁科院\程序\轨检\data\GNHS-HANGZHOU-NANJING-14052016-175302-1减变增.idf";
            //item.dbFile = @"H:\工作文件汇总\铁科院\程序\轨检\data\" + "InnerDB.idf";

            string json = JsonConvert.SerializeObject(item);

            CitFileReadData.ReadData obj = new CitFileReadData.ReadData();
            string result = obj.GetAllMileStone(json);
        }
    }

    public class Item
    {
        public string citFile { get; set; }

        public string citFile2 { get; set; }

        public string idfFile { get; set; }

        public string idfFile2 { get; set; }

        public string innerdbpath { get; set; }

        public int pointCount { get; set; }

        public string dbFile { get; set; }

        public bool isCorrect { get; set; }
    }
}
