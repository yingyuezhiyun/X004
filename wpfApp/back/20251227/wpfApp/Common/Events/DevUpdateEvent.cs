using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using wpfApp.ViewModels;

namespace wpfApp.Common.Events
{
    public enum DevUpdateType
    {
        Status,
        MeasureData
    }
    public class DevUpdateModel
    {
        public string DevName { get; set; }

        public int CH { get; set; }

        public DevUpdateType Type { get; set; }

        public DevMeasureData devMeasureData { get; set; } = new DevMeasureData();

        public DevStatusData devStatusData { get; set; } = new DevStatusData();

    }

    public class DevUpdateEvent : PubSubEvent<DevUpdateModel>
    {

    }
}
