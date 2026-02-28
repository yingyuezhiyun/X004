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
       
        public void SetCallback(Action<object, object> callback)
        {
            if (_currentProtocol == null)
            {
                //return;
                throw new InvalidOperationException("No protocol set.");
            }
            _currentProtocol.SetCallback(callback);
        }

        public void HandleData(byte[] data)
        {
            if (_currentProtocol == null)
            {
                //return;
                throw new InvalidOperationException("No protocol set.");
            }
            _currentProtocol.AddData(data);
        }


        //public List<byte> GetSendCommand(object paramsSet)
        //{
        //    return _currentProtocol.CreateFrame(paramsSet);
        //}
        //public List<byte> GetSendCommand(object paramsSet, params byte[] data)
        //{
        //    return _currentProtocol.CreateFrame(paramsSet, data);
        //}

        public List<byte> GetSendCommand(object paramsSet, params object[] data)
        {
            return _currentProtocol.CreateFrame(paramsSet, data);
        }
        public List<byte> GetSendCommand(object paramsSet, float data)
        {
            return _currentProtocol.CreateFrame(paramsSet, BitConverter.GetBytes(data));
        }
        public List<byte> GetQueryCommand(object paramsSet)
        {
            return _currentProtocol.CreateQueryFrame(paramsSet);
        }
    }
}
