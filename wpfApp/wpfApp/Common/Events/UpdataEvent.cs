using wpf_app.Common.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpf_app.Common.Events
{
    public class UpdataEvent : PubSubEvent<UpdataModel>
    {

    }
    public class UpdataModel
    {
        public string Filter { get; set; }
        public List<DetData> Data { get; set; } = new List<DetData>();
    }
}
