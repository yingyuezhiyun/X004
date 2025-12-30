using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfApp.Common.Models
{
    public class Wave
    {

        public bool IsEnable { get; set; }

        public bool IsVisible { get; set; }
        /// <summary>
        /// 幅值
        /// </summary>
        public double Maginitude { get; set; }

        /// <summary>
        /// 相位
        /// </summary>
        public int Phase { get; set; }

        /// <summary>
        /// 频率
        /// </summary>
        public double Frequency { get; set; }

    }
}
