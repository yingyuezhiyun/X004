using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Windows.Networking;
using wpfApp.Common.CtrlProtocol;
using wpfApp.Common.Dialog;
using wpfApp.Common.PLDProtocol;
using wpfApp.Extensions;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;
using static wpfApp.ViewModels.PLDMainViewModel;
using static wpfApp.ViewModels.TabDevViewModel;

namespace wpfApp.ViewModels
{
    public partial class PLDMainViewModel
    {

        private List<string> portName;
        public List<string> PortName
        {
            get => SerialPort.GetPortNames().ToList();
            set { portName = value; RaisePropertyChanged(); }
        }
        private SerialPort deviceSerialPort = new SerialPort();
        /// <summary>
        /// 串口
        /// </summary>
        public SerialPort DeviceSerialPort
        {
            get { return deviceSerialPort; }
            set { deviceSerialPort = value; RaisePropertyChanged(); }
        }

        void UpdataSerialPort()
        {
            PortName = SerialPort.GetPortNames().ToList();
        }

        private string _devPortName;

        public string DevPortName
        {
            get { return _devPortName; }
            set { _devPortName = value; RaisePropertyChanged(); }
        }


        private bool _deviceIsOpen;
        public bool DeviceIsOpen
        {
            get => DeviceSerialPort.IsOpen;
            set { _deviceIsOpen = value; RaisePropertyChanged(); }
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
                    DeviceSerialPort.Parity = Parity.Even;
                    DeviceSerialPort.BaudRate = 921600;
                    DeviceSerialPort.StopBits = StopBits.Two;
                    DeviceSerialPort.DataBits = 8;
                    DeviceSerialPort.PortName = DevPortName;
                    DeviceSerialPort.Open();
                    connectTimer.Start();
                    bitTimer.Start();
                    DevParamUpdata();
                    MeasureParams.StatusInfo.ConnectStatus.Value = "连接中...";
                }
                catch (Exception ex)
                {
                    aggregator.SendMessage(ex.Message, "Main");
                }

            }
            DeviceIsOpen = DeviceSerialPort.IsOpen;
        }

        private void serialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int bytesToRead = DeviceSerialPort.BytesToRead;      //获取接收缓冲区中的字节数
            if (bytesToRead > 0)
            {
                byte[] tempBuffer = new byte[bytesToRead];
                DeviceSerialPort.Read(tempBuffer, 0, bytesToRead);   //读取
                protocolManager.HandleData(tempBuffer);
            }
        }
        public string FormatNullableFloat(object value, int decimalPlaces)
        {
            var t = value as float?;
            if (t.HasValue)
            {
                return t.Value.ToString($"F{decimalPlaces}");
            }
            else
            {
                return "N/A"; // 或者返回其他默认值，例如：return ""; 
            }
        }

        bool isIgnoreBitErr = false;
        async void  UpdateStatus(UInt32 p)
        {
            string status = "";
            foreach (var item in PLDParams.ErrStatus)
            {
                if ((p & (UInt32)item.statusFlags) != 0)
                {
                    status += item.Name;
                }
            }
            if (status == "")
            {
                status = "正常";
            }
            else if (!isIgnoreBitErr)
            {
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    var dialogResult = await dialogHostService.Question("运行错误", "\r\n设备状态异常，已经停止运行!\r\n" + "错误内容：" + status, msgType: MsgType.YesIgnoreRetry);
                    //if (dialogResult.Result != Prism.Services.Dialogs.ButtonResult.OK) return;
                    if (dialogResult.Result == Prism.Dialogs.ButtonResult.Retry)
                    {
                        DevClearErr();
                        return;
                    }
                    else if (dialogResult.Result == Prism.Dialogs.ButtonResult.Yes)
                    {
                        DevClose();
                    }
                    else if (dialogResult.Result == Prism.Dialogs.ButtonResult.Ignore)
                    {
                        isIgnoreBitErr = true;
                    }
                });
            }
            MeasureParams.StatusInfo.BITStatus.Value = status;

            bool sw_on = false;
            sw_on = (p & (UInt32)PLDParams.StatusFlags.TEC1_SW) != 0;
            UpdateTEC_SW(0, sw_on);
            sw_on = (p & (UInt32)PLDParams.StatusFlags.TEC2_SW) != 0;
            UpdateTEC_SW(1, sw_on);
            sw_on = (p & (UInt32)PLDParams.StatusFlags.TEC3_SW) != 0;
            UpdateTEC_SW(2, sw_on);
            sw_on = (p & (UInt32)PLDParams.StatusFlags.TEC4_SW) != 0;
            UpdateTEC_SW(3, sw_on);

            sw_on = (p & (UInt32)PLDParams.StatusFlags.LD1_SW) != 0;
            UpdateLD_SW(0, sw_on);
            sw_on = (p & (UInt32)PLDParams.StatusFlags.LD2_SW) != 0;
            UpdateLD_SW(1, sw_on);

            sw_on = (p & (UInt32)PLDParams.StatusFlags.Q_SW) != 0;
            UpdateTQ_SW(sw_on);

            sw_on = (p & (UInt32)PLDParams.StatusFlags.TRQ_Type) != 0;
            UpdateTrigType(!sw_on);

            sw_on = (p & (UInt32)PLDParams.StatusFlags.Pulse_Type) != 0;
            UpdatePulseType(!sw_on);
        }

        void UpdateTEC_SW(int idx, bool sw_on)
        {
            if (sw_on)
            {
                SettingParamsToShow.TECParams[idx].IsWork = true;
                MeasureParams.StatusInfo.TECStatus[idx].Value = "开";
            }
            else
            {
                SettingParamsToShow.TECParams[idx].IsWork = false;
                MeasureParams.StatusInfo.TECStatus[idx].Value = "关";
            }
        }

        void UpdateLD_SW(int idx, bool sw_on)
        {
            if (sw_on)
            {
                SettingParamsToShow.LDParams[idx].IsWork = true;
                MeasureParams.StatusInfo.LDStatus[idx].Value = "开";
            }
            else
            {
                SettingParamsToShow.LDParams[idx].IsWork = false;
                MeasureParams.StatusInfo.LDStatus[idx].Value = "关";
            }
        }

        void UpdateTQ_SW(bool sw_on)
        {
            if (sw_on)
            {
                SettingParamsToShow.TQParams.IsWork = "开";
                MeasureParams.StatusInfo.TQStatus.Value = "开";
            }
            else
            {
                SettingParamsToShow.TQParams.IsWork = "关";
                MeasureParams.StatusInfo.TQStatus.Value = "关";
            }
        }

        void UpdateTrigType(bool isInter)
        {
            if (isInter)
            {
                SettingParamsToShow.Mode = "内调制";
                MeasureParams.StatusInfo.Mode.Value = "内调制";
            }
            else
            {
                SettingParamsToShow.Mode = "外调制";
                MeasureParams.StatusInfo.Mode.Value = "外调制";
            }
        }

        void UpdatePulseType(bool isSPWM)
        {
            if (isSPWM)
            {
                SettingParamsToShow.PulseType = "变频";
                MeasureParams.StatusInfo.PulseType.Value = "变频";
            }
            else
            {
                SettingParamsToShow.PulseType = "定频";
                MeasureParams.StatusInfo.PulseType.Value = "定频";
            }
        }

        void serialPortDataCallBack(object paramsSet, object data)
        {
            if (paramsSet as PLDParams.PLDParamsFromGet? == null)
            {
                return;
            }
            connectTimer.Stop();
            connectTimer.Start();
            if (MeasureParams.StatusInfo.ConnectStatus.Value != "已连接")
            {
                MeasureParams.StatusInfo.ConnectStatus.Value = "已连接";
            }            
            var a = (PLDParams.PLDParamsFromGet)paramsSet;
            switch (a)
            {
                case PLDParams.PLDParamsFromGet.None:
                    break;
                case PLDParams.PLDParamsFromGet.LD1_S_Cur:
                    SettingParamsToShow.LDParams[0].Curr = FormatNullableFloat(data,1);
                    break;
                case PLDParams.PLDParamsFromGet.LD2_S_Cur:
                    SettingParamsToShow.LDParams[1].Curr = FormatNullableFloat(data, 1);
                    break;
                case PLDParams.PLDParamsFromGet.DFLT_V:
                    break;
                case PLDParams.PLDParamsFromGet.INTER_TRG_FREQ:
                    SettingParamsToShow.PulseParams.Freq = (data as UInt16?).ToString();
                    break;
                case PLDParams.PLDParamsFromGet.PULSE_WIDTH:
                    SettingParamsToShow.PulseParams.Width = (data as UInt16?).ToString();
                    break;
                case PLDParams.PLDParamsFromGet.Q_DELAY:
                    SettingParamsToShow.TQParams.Delay = (data as UInt16?).ToString();
                    break;
                case PLDParams.PLDParamsFromGet.TRG_TYPE:
                    UpdateTrigType(data as PLDParams.TRIGTYPE? == PLDParams.TRIGTYPE.INTER);
                    
                    break;
                case PLDParams.PLDParamsFromGet.PulseType:
                    UpdatePulseType(data as PLDParams.PulseType? == PLDParams.PulseType.SPWM);
                 
                    break;
                case PLDParams.PLDParamsFromGet.LD1_SW:
                    UpdateLD_SW(0, data as byte? == 1);
                    break;
                case PLDParams.PLDParamsFromGet.LD2_SW:
                    UpdateLD_SW(1, data as byte? == 1);
                    break;
                case PLDParams.PLDParamsFromGet.Q_SW:
                    UpdateTQ_SW(data as byte? == 1);                   
                    break;
                case PLDParams.PLDParamsFromGet.TEC1_SW:
                        UpdateTEC_SW(0, data as byte? == 1);
                    break;
                case PLDParams.PLDParamsFromGet.TEC2_SW:
                    UpdateTEC_SW(1, data as byte? == 1);
                    break;
                case PLDParams.PLDParamsFromGet.TEC3_SW:
                    UpdateTEC_SW(2, data as byte? == 1);
                    break;
                case PLDParams.PLDParamsFromGet.TEC4_SW:
                    UpdateTEC_SW(3, data as byte? == 1);
                    break;
                case PLDParams.PLDParamsFromGet.TEC1_PARA:
                    {
                        var p = data as PLDParams;
                        if (p != null)
                        {
                            SettingParamsToShow.TECParams[0].Vol = p.tECParams.Vol.ToString("F1");
                            SettingParamsToShow.TECParams[0].Temp = p.tECParams.Temp.ToString("F1");
                            SettingParamsToShow.LDParams[0].Temp = p.tECParams.Temp.ToString("F1");
                        }
                    }
                    break;
                case PLDParams.PLDParamsFromGet.TEC2_PARA:
                    {
                        var p = data as PLDParams;
                        if (p != null)
                        {
                            SettingParamsToShow.TECParams[1].Vol = p.tECParams.Vol.ToString("F1");
                            SettingParamsToShow.TECParams[1].Temp = p.tECParams.Temp.ToString("F1");
                            SettingParamsToShow.LDParams[1].Temp = p.tECParams.Temp.ToString("F1");
                        }
                    }
                    break;
                case PLDParams.PLDParamsFromGet.TEC3_PARA:
                    {
                        var p = data as PLDParams;
                        if (p != null)
                        {
                            SettingParamsToShow.TECParams[2].Vol = p.tECParams.Vol.ToString("F1");
                            SettingParamsToShow.TECParams[2].Temp = p.tECParams.Temp.ToString("F1");
                        }
                    }
                    break;
                case PLDParams.PLDParamsFromGet.TEC4_PARA:
                    {
                        var p = data as PLDParams;
                        if (p != null)
                        {
                            SettingParamsToShow.TECParams[3].Vol = p.tECParams.Vol.ToString("F1");
                            SettingParamsToShow.TECParams[3].Temp = p.tECParams.Temp.ToString("F1");
                        }
                    }
                    break;
                case PLDParams.PLDParamsFromGet.PULSE_PARA:
                    {
                        var p = data as PLDParams;
                        if (p != null)
                        {
                            SettingParamsToShow.PulseParams.Num = p.pulseParams.Num.ToString();
                            for (int i = 0; i < 11; i++)
                            {
                                SettingParamsToShow.PulseParams.Num =p.pulseParams.Interval[i].ToString();
                            }
                        }
                    }
                    break;
                case PLDParams.PLDParamsFromGet.LD1_M_Cur:
                    MeasureParams.LDParams[0].Curr.Value = FormatNullableFloat(data, 1); 
                    break;
                case PLDParams.PLDParamsFromGet.LD2_M_Cur:
                    MeasureParams.LDParams[1].Curr.Value = FormatNullableFloat(data, 1);
                    break;
                case PLDParams.PLDParamsFromGet.L1_M_V:
                    MeasureParams.LDParams[0].Vol.Value = FormatNullableFloat(data, 1);
                    break;
                case PLDParams.PLDParamsFromGet.L2_M_V:
                    MeasureParams.LDParams[1].Vol.Value = FormatNullableFloat(data, 1);
                    break;
                case PLDParams.PLDParamsFromGet.PWR_TEMP:
                    MeasureParams.OtherInfos.PwrTemp.Value = FormatNullableFloat(data, 1);
                  
                    break;
                case PLDParams.PLDParamsFromGet.OUT_PD:
                    MeasureParams.PDParams.Power.Value = FormatNullableFloat(data, 1);
                    break;
                case PLDParams.PLDParamsFromGet.OUT_TEMP:
                    MeasureParams.PDParams.Temp.Value = FormatNullableFloat(data, 1);
                    break;
                case PLDParams.PLDParamsFromGet.TEC1_M_TEMP:
                    MeasureParams.LDParams[0].Temp.Value = FormatNullableFloat(data, 1);
                    break;
                case PLDParams.PLDParamsFromGet.TEC1_M_PW:
                    MeasureParams.TECParams[0].Power.Value = FormatNullableFloat(data, 1);
                    break;
                case PLDParams.PLDParamsFromGet.TEC2_M_TEMP:
                    MeasureParams.LDParams[1].Temp.Value = FormatNullableFloat(data, 1);
                    break;
                case PLDParams.PLDParamsFromGet.TEC2_M_PW:
                    MeasureParams.TECParams[1].Power.Value = FormatNullableFloat(data, 1);
                    break;
                case PLDParams.PLDParamsFromGet.TEC3_M_TEMP:
                    MeasureParams.TECParams[2].Temp.Value = FormatNullableFloat(data, 1);
                    break;
                case PLDParams.PLDParamsFromGet.TEC3_M_PW:
                    MeasureParams.TECParams[2].Power.Value = FormatNullableFloat(data, 1);
                    break;
                case PLDParams.PLDParamsFromGet.TEC4_M_TEMP:
                    MeasureParams.TECParams[3].Temp.Value = FormatNullableFloat(data, 1);
                    break;
                case PLDParams.PLDParamsFromGet.TEC4_M_PW:
                    MeasureParams.TECParams[3].Power.Value = FormatNullableFloat(data, 1);
                    break;
                case PLDParams.PLDParamsFromGet.WRK_STA:
                    UpdateStatus((UInt32)data);
                    break;
                case PLDParams.PLDParamsFromGet.ALL_SET:
                    
                    break;
                case PLDParams.PLDParamsFromGet.ALL_M:
                    {
                        var p = data as PLDParams;
                        if (p != null)
                        {
                            MeasureParams.LDParams[0].Curr.Value = p.AllMeasureParams.LDCurr[0].ToString("F1");
                            MeasureParams.LDParams[1].Curr.Value = p.AllMeasureParams.LDCurr[1].ToString("F1");
                            MeasureParams.LDParams[0].Vol.Value = p.AllMeasureParams.LDVol[0].ToString("F1");
                            MeasureParams.LDParams[1].Vol.Value = p.AllMeasureParams.LDVol[1].ToString("F1");
                            MeasureParams.OtherInfos.PwrTemp.Value = p.AllMeasureParams.PwrTemp.ToString("F1");
                            MeasureParams.PDParams.Power.Value = p.AllMeasureParams.PD.ToString("F1");
                            MeasureParams.PDParams.Temp.Value = p.AllMeasureParams.PDTemp.ToString("F1");
                            for (int i = 0; i < 4; i++)
                            {
                                MeasureParams.TECParams[i].Temp.Value = p.AllMeasureParams.TECTemp[i].ToString("F1");
                                MeasureParams.TECParams[i].Power.Value = p.AllMeasureParams.TECPower[i].ToString("F1");
                                MeasureParams.TECParams[i].Curr.Value = p.AllMeasureParams.TECCur[i].ToString("F1");
                            }
                            MeasureParams.LDParams[0].Temp.Value = MeasureParams.TECParams[0].Temp.Value;
                            MeasureParams.LDParams[1].Temp.Value = MeasureParams.TECParams[1].Temp.Value;                           
                            UpdateStatus(p.AllMeasureParams.Status);
                            Update.Version = p.AllMeasureParams.Version;


                            bool IsSave =false;
                            for (int i = 0; i < 2; i++)
                            {
                                if (SettingParams.LDParams[i].IsWork)
                                {
                                    IsSave = true;
                                }
                            }
                            for (int i = 0; i < 4; i++)
                            {
                                if (SettingParams.TECParams[i].IsWork)
                                {
                                    IsSave = true;
                                }
                            }
                            if (IsSave)
                            {
                                var d = new MeasureDataToSave();
                                saveDataFile(d);
                            }
                        }
                    }
                    break;
                case PLDParams.PLDParamsFromGet.UPGRADE:
                    break;
                case PLDParams.PLDParamsFromGet.LD1_Vol:
                    SettingParamsToShow.LDParams[0].Vol = FormatNullableFloat(data, 1);
                    break;
                case PLDParams.PLDParamsFromGet.LD2_Vol:
                    SettingParamsToShow.LDParams[1].Vol = FormatNullableFloat(data, 1);
                    break;
                case PLDParams.PLDParamsFromGet.LD1_HOC:
                    SettingParamsToShow.LDParams[0].HOC = FormatNullableFloat(data, 1);
                    break;
                case PLDParams.PLDParamsFromGet.LD2_HOC:
                    SettingParamsToShow.LDParams[1].HOC = FormatNullableFloat(data, 1);
                    break;
                case PLDParams.PLDParamsFromGet.LD1_SKB:
                    {
                        var p = data as PLDParams;
                        if (p != null)
                        {
                            SettingParamsToShow.LDParams[0].CalibSet.K= p.CalibCoef.k.ToString("F2");
                            SettingParamsToShow.LDParams[0].CalibSet.B = p.CalibCoef.b.ToString("F2");
                        }
                    }
                    break;
                case PLDParams.PLDParamsFromGet.LD2_SKB:
                    {
                        var p = data as PLDParams;
                        if (p != null)
                        {
                            SettingParamsToShow.LDParams[1].CalibSet.K = p.CalibCoef.k.ToString("F2");
                            SettingParamsToShow.LDParams[1].CalibSet.B = p.CalibCoef.b.ToString("F2");
                        }
                    }
                    break;
                case PLDParams.PLDParamsFromGet.LD1_MKB:
                    {
                        var p = data as PLDParams;
                        if (p != null)
                        {
                            SettingParamsToShow.LDParams[0].CalibMeasure.K = p.CalibCoef.k.ToString("F2");
                            SettingParamsToShow.LDParams[0].CalibMeasure.B = p.CalibCoef.b.ToString("F2");
                        }
                    }
                    break;
                case PLDParams.PLDParamsFromGet.LD2_MKB:
                    {
                        var p = data as PLDParams;
                        if (p != null)
                        {
                            SettingParamsToShow.LDParams[1].CalibMeasure.K = p.CalibCoef.k.ToString("F2");
                            SettingParamsToShow.LDParams[1].CalibMeasure.B = p.CalibCoef.b.ToString("F2");
                        }
                    }
                    break;
                case PLDParams.PLDParamsFromGet.PD_MKB:
                    {
                        var p = data as PLDParams;
                        if (p != null)
                        {
                            SettingParamsToShow.CalibPD.K = p.CalibCoef.k.ToString("F2");
                            SettingParamsToShow.CalibPD.B = p.CalibCoef.b.ToString("F2");
                        }
                    }
                    break;
                case PLDParams.PLDParamsFromGet.TEC1_PID:
                    {
                        var p = data as PLDParams;
                        if (p != null)
                        {
                            SettingParamsToShow.TECParams[0].PID.P = p.PIDCoef.p.ToString("F2");
                            SettingParamsToShow.TECParams[0].PID.I = p.PIDCoef.i.ToString("F2");
                            SettingParamsToShow.TECParams[0].PID.D = p.PIDCoef.d.ToString("F2");
                        }
                    }
                    break;
                case PLDParams.PLDParamsFromGet.TEC2_PID:
                    {
                        var p = data as PLDParams;
                        if (p != null)
                        {
                            SettingParamsToShow.TECParams[1].PID.P = p.PIDCoef.p.ToString("F2");
                            SettingParamsToShow.TECParams[1].PID.I = p.PIDCoef.i.ToString("F2");
                            SettingParamsToShow.TECParams[1].PID.D = p.PIDCoef.d.ToString("F2");
                        }
                    }
                    break;
                case PLDParams.PLDParamsFromGet.TEC3_PID:
                    {
                        var p = data as PLDParams;
                        if (p != null)
                        {
                            SettingParamsToShow.TECParams[2].PID.P = p.PIDCoef.p.ToString("F2");
                            SettingParamsToShow.TECParams[2].PID.I = p.PIDCoef.i.ToString("F2");
                            SettingParamsToShow.TECParams[2].PID.D = p.PIDCoef.d.ToString("F2");
                        }
                    }
                    break;
                case PLDParams.PLDParamsFromGet.TEC4_PID:
                    {
                        var p = data as PLDParams;
                        if (p != null)
                        {
                            SettingParamsToShow.TECParams[3].PID.P = p.PIDCoef.p.ToString("F2");
                            SettingParamsToShow.TECParams[3].PID.I = p.PIDCoef.i.ToString("F2");
                            SettingParamsToShow.TECParams[3].PID.D = p.PIDCoef.d.ToString("F2");
                        }
                    }
                    break;
                case PLDParams.PLDParamsFromGet.S_LCM:
                    {
                        var p = data as PLDParams;
                        if (p != null)
                        {
                            SettingParamsToShow.LCMParams.IsPowerOn = p.LCMSet.PwrEn == 1 ? "开" : "关";
                            SettingParamsToShow.LCMParams.IsIsMotorWork = p.LCMSet.MotorEn == 1 ? "开" : "关";
                            SettingParamsToShow.LCMParams.MotorSpeed = p.LCMSet.MotorSpeed.ToString();
                            SettingParamsToShow.LCMParams.FanSpeed = p.LCMSet.Fan.ToString();
                            SettingParamsToShow.LCMParams.PwrLimit = p.LCMSet.PwrLimit.ToString();
                            MeasureParams.StatusInfo.PumpStatus.Value = p.LCMSet.PwrEn == 1 ? "开" : "关";
                            MeasureParams.StatusInfo.FanStatus.Value = p.LCMSet.Fan > 0 ? "开" : "关";
                        }
                    }
                    break;
                case PLDParams.PLDParamsFromGet.M_LCM:
                    {
                        var p = data as PLDParams;
                        if (p != null)
                        {
                            MeasureParams.LCMParams.Vol.Value = p.LCMMeasure.DCVoltage.ToString();
                            MeasureParams.LCMParams.Curr.Value = p.LCMMeasure.DCCurrent.ToString();
                            MeasureParams.LCMParams.Power.Value = p.LCMMeasure.PwrLimit.ToString();
                            MeasureParams.LCMParams.MotorSpeed.Value = p.LCMMeasure.MotorSpeed.ToString();
                            MeasureParams.LCMParams.Temp.Value = p.LCMMeasure.Temp.ToString("F1");
                        }
                    }
                    break;
                case PLDParams.PLDParamsFromGet.SaveParam:
                    if (data as byte? == 1)
                    {
                        aggregator.SendMessage("参数保存成功!", "Main");
                    }
                    else
                    {
                        aggregator.SendMessage("参数保存失败!", "Main");
                    }
                    break;
                default:
                    break;
            }


        }

        void DevSetParam(PLDParams.PLDParamsToSet cmd, params byte[] param)
        {
            List<byte> data = protocolManager.GetSendCommand(cmd, param);
            SendData(data);
        }

        void DevQueryParam(PLDParams.PLDParamsToQuery cmd)
        {
            List<byte> data = protocolManager.GetQueryCommand(cmd);
            SendData(data);
        }

        object sendlocker = new object();   
        void SendData(List<byte> data)
        {
            if (data.Count == 0)
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
   
    
    
    }
}
