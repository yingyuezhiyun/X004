using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace wpfApp.Common.CtrlProtocol
{
   public interface ICommunicationProtocol
    {
       // List<byte> CreateFrame(object paramsSet, params byte[] data);
        List<byte> CreateQueryFrame(object paramsQuery);
        void AddData(byte[] data);
        void SetCallback(Action<object, object> callback);

        List<byte> CreateFrame(object paramsSet, params object[] data);
    }
}
