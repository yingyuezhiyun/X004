using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfApp.Common.CtrlProtocol;

namespace wpfApp.Common.PLDProtocol
{
   public interface ICommunicationProtocol2
    {
        List<byte> CreateFrame(object paramsSet, params byte[] data);
        List<byte> CreateQueryFrame(object paramsQuery);
        void AddData(byte[] data);
        void SetCallback(Action<object, object> callback);
    }
}
