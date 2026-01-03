using Prism.Events;
using System;
using System.Collections.Generic;

namespace wpfApp.Common.Events
{
    public enum PLDUpdateType
    {
        MeasureData
    }

    public class PLDMeasureData
    {
        public DateTime time { get; set; }
        public List<double> data_list { get; set; } = new List<double>();
    }

    public class PLDUpdateModel
    {
        public PLDUpdateType Type { get; set; }
        public PLDMeasureData plData { get; set; } = new PLDMeasureData();
    }

    public class PLDUpdateEvent : PubSubEvent<PLDUpdateModel>
    {

    }
}
