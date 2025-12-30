using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfApp.Common.CtrlProtocol;

namespace wpfApp.Common.Events
{
    public enum DevManageUpdataType
    {
        Query,//查询
        Answer,//回复
        ParamSet,//设置
        Updata,//更新设备
        ClearErr,//清除错误
    }

    public class DevManageUpdataModel
    {
        public string DevName { get; set; }

        public int CH { get; set; }

        public DevManageUpdataType Type { get; set; }

        public DevType devType { get; set; }

        public string DevPort {  get; set; }

        public bool IsOpen { get; set; }

        public bool IsIDE {  get; set; }

        public List<string> DevPorts { get; set; } = new List<string>();

    }


    public class DevManageEvent: PubSubEvent<DevManageUpdataModel>
    {

    }
}
