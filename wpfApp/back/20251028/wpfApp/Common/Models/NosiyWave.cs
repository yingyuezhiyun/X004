using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfApp.Common.Models
{
    public class NosiyWave
    {
        public bool IsEnable { get; set; }

        public bool IsVisible { get; set; }

        /// <summary>
        /// 标准偏差
        /// </summary>
        public double StdDev { get; set; }
    }
}
