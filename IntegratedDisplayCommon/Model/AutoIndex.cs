using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegratedDisplayCommon.Model
{
    public class AutoIndex
    {
        public long milePos { get; set; }
        public int km_current { get; set; }
        public int meter_current { get; set; }
        public int km_pre { get; set; }
        public int meter_pre { get; set; }
        public int meter_between { get; set; }

        public float MileCurrent
        {
            get
            {
                return km_current + meter_current / 4;
            }
        }
    }
}
