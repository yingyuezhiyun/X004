using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfApp.Common.CtrlProtocol
{
    public class RevData
    {

        public List<byte> rawData { get; set; } = new List<byte>();

        public byte cmdType { get { return this.rawData[3]; } }

        public byte cmd { get { return this.rawData[4]; } }


        public byte[] data { get { return rawData.Skip(5).Take(this.dataLen).ToArray(); } }

        public int dataLen { get { return rawData.Count - 7; } }

        public float ToFloat { get { return BitConverter.ToSingle(this.data, 0); } }

        public UInt32 ToUInt32 { get { return BitConverter.ToUInt32(this.data, 0); } }

        public DateTime time { get; set; }
    }
}
