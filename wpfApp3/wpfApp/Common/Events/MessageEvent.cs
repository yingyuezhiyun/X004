using Prism.Events;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfApp.Common.Events
{

    public class MessageModel
    {
        public string Filter { get; set; }
        public string Message { get; set; }

        public double TimeSpan { get; set; }

        private string? _color;
        public string Color
        {
            get => _color ?? GetDefaultColor(Type);
            set => _color = value;
        }
        private static readonly IReadOnlyDictionary<MessageType, string> DefaultColors
           = new Dictionary<MessageType, string>
           {
                //{ MessageType.Default,     "#FF9E9E9E" }, // Grey
                //{ MessageType.Success,     "#FF4CAF50" }, // Green
                //{ MessageType.Warning,     "#FFFFC107" }, // Amber/Orange
                //{ MessageType.Error,       "#FFF44336" }, // Red
                //{ MessageType.Information, "#FF2196F3" }, // Blue


                { MessageType.Default,     "#E6E6E6" }, // Grey
                { MessageType.Success,     "#3DB5EA" }, // Green
                { MessageType.Warning,     "#E6B205" }, // Amber/Orange
                { MessageType.Error,       "#E64717" }, // Red
                { MessageType.Information, "#2196F3" }, // Blue
           };

        private static string GetDefaultColor(MessageType type)
        {
            if (DefaultColors.TryGetValue(type, out var color))
            {
                return color;
            }

            return DefaultColors[MessageType.Default];
        }
        public MessageType Type { get; set; } = MessageType.Default;

        public enum MessageType
        {
            Default,
            Success,
            Warning,
            Error,
            Information,           
        };
    }

    public class MessageEvent : PubSubEvent<MessageModel>
    {

    }

   
}