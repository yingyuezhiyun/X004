using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using wpfApp.Common.CtrlProtocol;
using wpfApp.Extensions;

namespace wpfApp.ViewModels
{
    public partial class TabDevViewModel
    {
        object sendlocker = new object();
        void SendData(List<byte> data)
        {
            data.InsertRange(0, new List<byte> { 0x7e, 0xe7 });//帧头
            
            data.Insert(2, (byte)(data.Count + 3));//长度
            data.AddRange(new List<byte> { 0xaa, 0x55 });//帧尾
            lock (sendlocker)
            {
                if (DeviceSerialPort.IsOpen)
                    try
                    {
                        DeviceSerialPort.Write(data.ToArray(), 0, data.Count);

                    }
                    catch (Exception e)
                    {
                        //MessageBox.Show(e.Message);
                    }
            }
        }

        void SendData2(List<byte> data)
        {
            if (data.Count==0)
            {
                return;
            }
            lock (sendlocker)
            {
                if (DeviceSerialPort.IsOpen)
                    try
                    {
                        DeviceSerialPort.Write(data.ToArray(), 0, data.Count);
                    }
                    catch (Exception e)
                    {
                        //MessageBox.Show(e.Message);
                    }
            }
        }


        void DevSetParam(DevParamsToSet cmd)
        {
            List<byte> data = protocolManager.SendCommand(cmd);
            SendData2(data);

        }
        void DevSetParamFloatString(DevParamsToSet cmd, string param)
        {
            try
            {
                float value = Convert.ToSingle(param);
                List<byte> data = protocolManager.SendCommand(cmd, value);
                SendData2(data);
            }
            catch (Exception ex)
            {
                aggregator.SendMessage(ex.Message, "Main");
            }
        }
        void DevSetParam(DevParamsToSet cmd, byte param)
        {
            List<byte> data = protocolManager.SendCommand(cmd, param);
            SendData2(data);
        }

        void DevGetParam(DevParamsFromGet cmd)
        {
            List<byte> data = protocolManager.Query(cmd);
            SendData2(data); 

        }
        private void DeviceConnect()
        {
            if (DeviceSerialPort.IsOpen)
            {
                try
                {
                    DeviceSerialPort.Close();
                    DevClose();
                }
                catch (Exception ex)
                {
                    aggregator.SendMessage(ex.Message, "Main");
                }

            }
            else
            {
                try
                {
                    DeviceSerialPort.Parity = Parity.None;
                    DeviceSerialPort.BaudRate = 115200;
                    DeviceSerialPort.StopBits = StopBits.One;
                    DeviceSerialPort.DataBits = 8;
                    DeviceSerialPort.PortName = DevPortName;
                    DeviceSerialPort.Open();
                    connectTimer.Start();
                    bitTimer.Start();                    
                    DevParamUpdata();
                }
                catch (Exception ex)
                {
                    aggregator.SendMessage(ex.Message, "Main");
                }

            }
            DeviceIsOpen = DeviceSerialPort.IsOpen;
        }

        void UpdataSerialPort()
        {
            PortName = SerialPort.GetPortNames().ToList();
        }

        object locker = new object();
        private const UInt16 CMD_HEAD = 0xE77E;
        private const UInt16 CMD_TAIL = 0x55AA;
        public List<RevData> revDatas { get; set; } = new List<RevData>();

        private List<byte> remainData = new List<byte>();
        private void serialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int bytesToRead = DeviceSerialPort.BytesToRead;      //获取接收缓冲区中的字节数
            byte[] tempBuffer = new byte[bytesToRead];
            DeviceSerialPort.Read(tempBuffer, 0, bytesToRead);   //读取
            List<byte> receiveBufferTemp = new List<byte>();
        
            receiveBufferTemp.AddRange(remainData);
            receiveBufferTemp.AddRange(tempBuffer);
            remainData.Clear();
            int rev_cnt = receiveBufferTemp.Count;
            if (rev_cnt < 7)
            {
                remainData.AddRange(receiveBufferTemp);
                return;
            }
            int take_data_cnt = 0;
            for (int i = 0; i < rev_cnt - 3; i++)
            {

                int remain_cnt = rev_cnt - i;
                int data_len = receiveBufferTemp[i + 2];
                if (BitConverter.ToUInt16(receiveBufferTemp.ToArray(), i) == CMD_HEAD
                    && data_len <= remain_cnt && data_len>0)
                {
                    if (BitConverter.ToUInt16(receiveBufferTemp.ToArray(), i + data_len - 2) == CMD_TAIL)
                    {
                        RevData revData = new RevData();              
                        revData.rawData.AddRange(receiveBufferTemp.Skip(i).Take(data_len));
                        revData.time = DateTime.Now;
                        lock (locker)
                        {
                            revDatas.Add(revData);
                        }
                        i += data_len - 1;
                        take_data_cnt = i;
                        connectTimer.Stop();
                        connectTimer.Start();
                    }
                }

            }
            remainData.AddRange(receiveBufferTemp.Skip(take_data_cnt));

        }


     

    }
}
