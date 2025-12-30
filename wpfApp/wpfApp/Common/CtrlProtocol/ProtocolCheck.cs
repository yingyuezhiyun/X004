using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfApp.ViewModels;

namespace wpfApp.Common.CtrlProtocol
{
    public class ProtocolCheck
    {
        private const UInt16 CMD_HEAD = 0xE77E;
        private const UInt16 CMD_TAIL = 0x55AA;


        private RevData GetDevData(byte[] data)
        {
            if (data.Length >= 7)
            {
                int frameLen = data[2];
                if (BitConverter.ToUInt16(data) == CMD_HEAD
                    && frameLen > 0 && frameLen <= data.Length
                    && BitConverter.ToUInt16(data, frameLen - 2) == CMD_TAIL)
                {
                    RevData revData = new RevData();
                    revData.rawData.AddRange(data.Take(frameLen));
                    revData.time = DateTime.Now;
                    return revData;
                }
            }
            return null;
        }

        async Task<bool> DevBitCheck(SerialPort serialPort)
        {
            int RetryCnt = 5;
            Protocol_X002_002 protocol = new Protocol_X002_002();
            var data = protocol.CreateQueryFrame(DevParamsFromGet.BIT);
            while (RetryCnt > 0)
            {
                serialPort.Write(data.ToArray(), 0, data.Count);
                await Task.Delay(200);
                int serialLen = serialPort.BytesToRead;
                if (serialLen > 0)
                {
                    byte[] bytes = new byte[serialLen];
                    serialPort.Read(bytes, 0, serialLen);
                    var frame = GetDevData(bytes);
                    if (frame != null && protocol.ParseFrame(frame).ParamsGet == DevParamsFromGet.BIT)
                    {
                        return true;
                    }
                }
                RetryCnt--;
            }
            return false;
        }
        async Task<DevType> GetDevVerison(SerialPort serialPort)
        {
            int RetryCnt = 5;
            Protocol_X002_002 protocol = new Protocol_X002_002();
            var data = protocol.CreateQueryFrame(DevParamsFromGet.VERSION);

            //版本获取
            while (RetryCnt > 0)
            {
                serialPort.Write(data.ToArray(), 0, data.Count);
                await Task.Delay(200);
                int serialLen = serialPort.BytesToRead;
                if (serialLen > 0)
                {
                    byte[] bytes = new byte[serialLen];
                    serialPort.Read(bytes, 0, serialLen);
                    var frame = GetDevData(bytes);
                    
                    if (frame != null)
                    {
                        var d = protocol.ParseFrame(frame);
                        if (d.ParamsGet == DevParamsFromGet.VERSION)
                        {
                            return (DevType)d.result;
                        }
                       
                    }
                }
                RetryCnt--;
            }
            return DevType.X002_001;
        }

        public async Task<DevType> DevCheck(string portName)
        {
            int baudRate = 115200;
            DevType reslut = DevType.None;
            using (SerialPort serialPort = new SerialPort(portName, baudRate))
            {
                try
                {
                    serialPort.Open();
                    if (await DevBitCheck(serialPort))//自检信息检测
                    {
                        reslut = await GetDevVerison(serialPort);

                    }
                    serialPort.Close();
                }
                catch (Exception)
                {
                }
            }
            return reslut;
        }
    }
}
