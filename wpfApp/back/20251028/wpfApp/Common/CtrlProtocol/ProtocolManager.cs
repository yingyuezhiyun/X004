using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfApp.Common.CtrlProtocol
{
    public class ProtocolManager
    {
        private ICommunicationProtocol _currentProtocol;
        public void SetProtocol(ICommunicationProtocol protocol)
        { 
            _currentProtocol = protocol;
        }
        public List<byte> SendCommand(DevParamsToSet paramsSet)
        {
            return _currentProtocol.CreateFrame(paramsSet);
        }
        public List<byte> SendCommand(DevParamsToSet paramsSet, byte data)
        {
            return _currentProtocol.CreateFrame(paramsSet, data);
        }
        public List<byte> SendCommand(DevParamsToSet paramsSet, float data)
        {
            return _currentProtocol.CreateFrame(paramsSet, BitConverter.GetBytes(data));
        }
        //public List<byte> SendCommand(DevParamsToSet paramsSet, string data)
        //{
        //    try
        //    {
        //        byte[] datas = BitConverter.GetBytes(Convert.ToSingle(data));
        //        return _currentProtocol.CreateFrame(paramsSet, BitConverter.GetBytes(Convert.ToSingle(data)));
        //    }
        //    catch (Exception ex)
        //    {
        //        //aggregator.SendMessage(ex.Message, "Main");
        //        return new List<byte>();
        //    }
        //}
        public List<byte> Query(DevParamsFromGet paramsSet)
        {
            return _currentProtocol.CreateQueryFrame(paramsSet);
        }


        public (DevParamsFromGet ParamsGet, object result) Parse(RevData data)
        {
            return _currentProtocol.ParseFrame(data);
        }




    }
}
