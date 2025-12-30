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
using wpfApp.Common.Events;
using wpfApp.Common.PLDProtocol;
using wpfApp.Extensions;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;
using static wpfApp.Common.PLDProtocol.PLDParams;
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
                    //DeviceSerialPort.BaudRate = 115200;
                    DeviceSerialPort.StopBits = StopBits.Two;
                    DeviceSerialPort.DataBits = 8;
                    DeviceSerialPort.PortName = DevPortName;
                    DeviceSerialPort.Open();
                    connectTimer.Start();
                    bitTimer.Start();
                    DevParamUpdata();
                    MeasureParams.StatusInfo.ConnectStatus.Value = "连接中...";
                    isBootModeDetect = true;
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

            bool LDWork = false;
            sw_on = (p & (UInt32)PLDParams.StatusFlags.LD1_SW) != 0;
            UpdateLD_SW(0, sw_on);
            LDWork |= sw_on;
            sw_on = (p & (UInt32)PLDParams.StatusFlags.LD2_SW) != 0;
            UpdateLD_SW(1, sw_on);
            LDWork |= sw_on;
            LDCtrlEn = !LDWork;

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

        private bool isBootModeDetect = true;
        async void BootMode()
        {
            MeasureParams.StatusInfo.BITStatus.Value = "BOOT模式";

            if (isBootModeDetect)
            {
                isBootModeDetect = false;

               await Application.Current.Dispatcher.Invoke(() =>  dialogHostService.Question("警告", "\r\n当前处于BOOT模式，仅可用于程序升级！", msgType: MsgType.Yes));
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

            var p1 = data as PLDParams;
            int idx = 0;
            switch (a)
            {
                case PLDParams.PLDParamsFromGet.None:
                    break;
                case PLDParams.PLDParamsFromGet.BootMode:
                    BootMode();
                    break;
                case PLDParams.PLDParamsFromGet.LD1_S_Cur:
                    SettingParamsToShow.LDParams[0].Curr = p1.SetParams.LDParams[0].Curr.ToString("F1");
                    break;
                case PLDParams.PLDParamsFromGet.LD2_S_Cur:
                    SettingParamsToShow.LDParams[1].Curr = p1.SetParams.LDParams[1].Curr.ToString("F1");
                    break;
                case PLDParams.PLDParamsFromGet.DFLT_V:
                    break;
                case PLDParams.PLDParamsFromGet.INTER_TRG_FREQ:
                    SettingParamsToShow.PulseParams.Freq = p1.SetParams.PulseParams.Freq.ToString();
                    break;
                case PLDParams.PLDParamsFromGet.PULSE_WIDTH:
                    SettingParamsToShow.PulseParams.Width = p1.SetParams.PulseParams.Width.ToString();
                    break;
                case PLDParams.PLDParamsFromGet.Q_DELAY:
                    SettingParamsToShow.TQParams.Delay = p1.SetParams.TQParams.Delay.ToString();
                    break;
                case PLDParams.PLDParamsFromGet.TRG_TYPE:
                    UpdateTrigType(p1.SetParams.TrigType == PLDParams.TrigType.INTER);
                    break;
                case PLDParams.PLDParamsFromGet.PulseType:
                    UpdatePulseType(p1.SetParams.PulseType == PLDParams.PulseType.SPWM);
                 
                    break;
                case PLDParams.PLDParamsFromGet.LD1_SW:
                    UpdateLD_SW(0, p1.SetParams.LDParams[0].WorkType == PLDParams.WorkType.ON);
                    break;
                case PLDParams.PLDParamsFromGet.LD2_SW:
                    UpdateLD_SW(1, p1.SetParams.LDParams[1].WorkType == PLDParams.WorkType.ON);
                    break;
                case PLDParams.PLDParamsFromGet.Q_SW:
                    UpdateTQ_SW(p1.SetParams.TQParams.WorkType == PLDParams.WorkType.ON);                   
                    break;
                case PLDParams.PLDParamsFromGet.TEC1_SW:
                        UpdateTEC_SW(0, p1.SetParams.TECParams[0].WorkType == PLDParams.WorkType.ON);
                    break;
                case PLDParams.PLDParamsFromGet.TEC2_SW:
                    UpdateTEC_SW(1, p1.SetParams.TECParams[1].WorkType == PLDParams.WorkType.ON);
                    break;
                case PLDParams.PLDParamsFromGet.TEC3_SW:
                    UpdateTEC_SW(2, p1.SetParams.TECParams[2].WorkType == PLDParams.WorkType.ON);
                    break;
                case PLDParams.PLDParamsFromGet.TEC4_SW:
                    UpdateTEC_SW(3, p1.SetParams.TECParams[3].WorkType == PLDParams.WorkType.ON);
                    break;
                case PLDParams.PLDParamsFromGet.TEC1_PARA:
                    {
                        
                            SettingParamsToShow.TECParams[0].Vol = p1.SetParams.TECParams[0].Vol.ToString("F1");
                            SettingParamsToShow.TECParams[0].Temp = p1.SetParams.TECParams[0].Temp.ToString("F1");
                            SettingParamsToShow.LDParams[0].Temp = p1.SetParams.TECParams[0].Temp.ToString("F1");
                        setParam.TECParams[0].Vol = p1.SetParams.TECParams[0].Vol;
                    }
                    break;
                case PLDParams.PLDParamsFromGet.TEC2_PARA:
                    {
                        
                            SettingParamsToShow.TECParams[1].Vol = p1.SetParams.TECParams[1].Vol.ToString("F1");
                            SettingParamsToShow.TECParams[1].Temp = p1.SetParams.TECParams[1].Temp.ToString("F1");
                            SettingParamsToShow.LDParams[1].Temp = p1.SetParams.TECParams[1].Temp.ToString("F1");
                        setParam.TECParams[1].Vol = p1.SetParams.TECParams[1].Vol;
                    }
                    break;
                case PLDParams.PLDParamsFromGet.TEC3_PARA:
                    {

                        SettingParamsToShow.TECParams[2].Vol = p1.SetParams.TECParams[2].Vol.ToString("F1");
                        SettingParamsToShow.TECParams[2].Temp = p1.SetParams.TECParams[2].Temp.ToString("F1");
                        setParam.TECParams[2].Vol = p1.SetParams.TECParams[2].Vol;
                    }
                    break;
                case PLDParams.PLDParamsFromGet.TEC4_PARA:
                    {

                        SettingParamsToShow.TECParams[3].Vol = p1.SetParams.TECParams[3].Vol.ToString("F1");
                        SettingParamsToShow.TECParams[3].Temp = p1.SetParams.TECParams[3].Temp.ToString("F1");
                        setParam.TECParams[3].Vol = p1.SetParams.TECParams[3].Vol;
                    }
                    break;
                case PLDParams.PLDParamsFromGet.PULSE_PARA:
                    {
                        SettingParamsToShow.PulseParams.Num = p1.SetParams.PulseParams.Num.ToString();
                        for (int i = 0; i < 11; i++)
                        {
                            SettingParamsToShow.PulseParams.Interval[i].Value = p1.SetParams.PulseParams.Interval[i].ToString();
                        }
                    }
                    break;
                case PLDParams.PLDParamsFromGet.LD1_M_Cur:
                    MeasureParams.LDParams[0].Curr.Value = p1.MeasureParams.LDParams[0].Curr.ToString("F1");
                    break;
                case PLDParams.PLDParamsFromGet.LD2_M_Cur:
                    MeasureParams.LDParams[1].Curr.Value = p1.MeasureParams.LDParams[1].Curr.ToString("F1");
                    break;
                case PLDParams.PLDParamsFromGet.L1_M_V:
                    MeasureParams.LDParams[0].Vol.Value = p1.MeasureParams.LDParams[0].Vol.ToString("F1");
                    break;
                case PLDParams.PLDParamsFromGet.L2_M_V:
                    MeasureParams.LDParams[1].Vol.Value = p1.MeasureParams.LDParams[1].Vol.ToString("F1");
                    break;
                case PLDParams.PLDParamsFromGet.PWR_TEMP:
                    MeasureParams.OtherInfos.PwrTemp.Value = p1.MeasureParams.OtherInfos.PwrTemp.ToString("F1");

                    break;
                case PLDParams.PLDParamsFromGet.OUT_PD:
                    MeasureParams.PDParams.Power.Value = p1.MeasureParams.PDParams.Power.ToString("F1");
                    break;
                case PLDParams.PLDParamsFromGet.OUT_TEMP:
                    MeasureParams.PDParams.Temp.Value = p1.MeasureParams.PDParams.Temp.ToString("F1");
                    break;
                case PLDParams.PLDParamsFromGet.TEC1_M_TEMP:
                    MeasureParams.LDParams[0].Temp.Value = p1.MeasureParams.TECParams[0].Temp.ToString("F1");
                    break;
                case PLDParams.PLDParamsFromGet.TEC1_M_PW:
                    MeasureParams.TECParams[0].Power.Value = p1.MeasureParams.TECParams[0].Power.ToString("F1");
                    break;
                case PLDParams.PLDParamsFromGet.TEC2_M_TEMP:
                    MeasureParams.LDParams[1].Temp.Value = p1.MeasureParams.TECParams[1].Temp.ToString("F1");
                    break;
                case PLDParams.PLDParamsFromGet.TEC2_M_PW:
                    MeasureParams.TECParams[1].Power.Value = p1.MeasureParams.TECParams[1].Power.ToString("F1");
                    break;
                case PLDParams.PLDParamsFromGet.TEC3_M_TEMP:
                    MeasureParams.TECParams[2].Temp.Value = p1.MeasureParams.TECParams[2].Temp.ToString("F1");
                    break;
                case PLDParams.PLDParamsFromGet.TEC3_M_PW:
                    MeasureParams.TECParams[2].Power.Value = p1.MeasureParams.TECParams[2].Power.ToString("F1");
                    break;
                case PLDParams.PLDParamsFromGet.TEC4_M_TEMP:
                    MeasureParams.TECParams[3].Temp.Value = p1.MeasureParams.TECParams[3].Temp.ToString("F1");
                    break;
                case PLDParams.PLDParamsFromGet.TEC4_M_PW:
                    MeasureParams.TECParams[3].Power.Value = p1.MeasureParams.TECParams[3].Power.ToString("F1");
                    break;
                case PLDParams.PLDParamsFromGet.WRK_STA:
                    UpdateStatus(p1.MeasureParams.Status);
                    break;
                case PLDParams.PLDParamsFromGet.ALL_SET:
                    
                    break;
                case PLDParams.PLDParamsFromGet.ALL_M:
                    {

                        MeasureParams.LDParams[0].Curr.Value = p1.MeasureParams.LDParams[0].Curr.ToString("F1");
                        MeasureParams.LDParams[1].Curr.Value = p1.MeasureParams.LDParams[1].Curr.ToString("F1");
                        MeasureParams.LDParams[0].Vol.Value = p1.MeasureParams.LDParams[0].Vol.ToString("F1");
                        MeasureParams.LDParams[1].Vol.Value = p1.MeasureParams.LDParams[1].Vol.ToString("F1");
                        MeasureParams.OtherInfos.PwrTemp.Value = p1.MeasureParams.OtherInfos.PwrTemp.ToString("F1");
                        MeasureParams.OtherInfos.SysCurr.Value = p1.MeasureParams.OtherInfos.SysCurr.ToString("F1");
                        MeasureParams.OtherInfos.SysVol.Value = p1.MeasureParams.OtherInfos.SysVol.ToString("F1");
                        MeasureParams.PDParams.Power.Value = p1.MeasureParams.PDParams.Power.ToString("F1");
                        MeasureParams.PDParams.Temp.Value = p1.MeasureParams.PDParams.Temp.ToString("F1");
                        for (int i = 0; i < 4; i++)
                        {
                            MeasureParams.TECParams[i].Temp.Value = p1.MeasureParams.TECParams[i].Temp.ToString("F1");
                            MeasureParams.TECParams[i].Power.Value = p1.MeasureParams.TECParams[i].Power.ToString("F1");
                            MeasureParams.TECParams[i].Curr.Value = p1.MeasureParams.TECParams[i].Curr.ToString("F1");
                        }
                        MeasureParams.LDParams[0].Temp.Value = MeasureParams.TECParams[0].Temp.Value;
                        MeasureParams.LDParams[1].Temp.Value = MeasureParams.TECParams[1].Temp.Value;
                        UpdateStatus(p1.MeasureParams.Status);
                        Update.Version = p1.MeasureParams.Version;
                        bool IsSave = false;
                        for (int i = 0; i < 2; i++)
                        {
                            
                            if (SettingParamsToShow.LDParams[i].IsWork)
                            {
                                IsSave = true;
                            }
                        }
                        for (int i = 0; i < 4; i++)
                        {
                            if (SettingParamsToShow.TECParams[i].IsWork)
                            {
                                IsSave = true;
                            }
                        }
                        if (IsSave)
                        {
                            saveDataFile(p1);
                        }
                    }
                    break;
                case PLDParams.PLDParamsFromGet.Upgrade:
                    switch (p1.MeasureParams.UpgradeParams.Status)
                    {
                        case PLDParams.UpgradeStatus.IDLE:                            
                   
                        case PLDParams.UpgradeStatus.CUR_DONE:
                            if (p1.MeasureParams.UpgradeParams.CurrIdx == setParam.UpgradeParams.CurrIdx)
                            {
                                setParam.UpgradeParams.CurrIdx++;
                                Update.IsAccess = true;
                            }
                            break;
                        case PLDParams.UpgradeStatus.LAST_DONE:
                            if (!Update.IsUpdate)
                            {
                                Application.Current.Dispatcher.Invoke(() => aggregator.SendMessage("程序烧写完成，请断电重启完成升级!", "Main",Type:MessageModel.MessageType.Success,TimeSpan:4.0));
                            }
                            Update.IsAccess = true;
                            break;
                        case PLDParams.UpgradeStatus.LAST_CHECK_ERR:                        
                        case PLDParams.UpgradeStatus.LAST_FAILED:
                            Application.Current.Dispatcher.Invoke(() => aggregator.SendMessage("程序升级失败,CRC校验失败!", "Main", Type: MessageModel.MessageType.Error, TimeSpan: 4.0));
                            Update.IsAccess = false;
                            Update.IsUpdate = false;
                            break;
                        case PLDParams.UpgradeStatus.RUNNING: Update.IsAccess = false;break;
                        case PLDParams.UpgradeStatus.CUR_CHECK_ERR:                         
                        case PLDParams.UpgradeStatus.CUR_FALIED:
                            Application.Current.Dispatcher.Invoke(() => aggregator.SendMessage($"程序升级失败,第{setParam.UpgradeParams.CurrIdx + 1}包校验错误!", "Main", Type: MessageModel.MessageType.Error, TimeSpan: 4.0));
                            Update.IsAccess = false;
                            Update.IsUpdate = false;
                            break;
                        default:
                            break;
                    }
                    
                    break;
                case PLDParams.PLDParamsFromGet.LD1_Vol:
                    SettingParamsToShow.LDParams[0].Vol = p1.SetParams.LDParams[0].Vol.ToString("F1");
                    break;
                case PLDParams.PLDParamsFromGet.LD2_Vol:
                    SettingParamsToShow.LDParams[1].Vol = p1.SetParams.LDParams[1].Vol.ToString("F1");
                    break;
                case PLDParams.PLDParamsFromGet.LD1_HOC:
                    SettingParamsToShow.LDParams[0].HOC = p1.SetParams.LDParams[0].HOC.ToString("F1");
                    break;
                case PLDParams.PLDParamsFromGet.LD2_HOC:
                    SettingParamsToShow.LDParams[1].HOC = p1.SetParams.LDParams[1].HOC.ToString("F1");
                    break;
                case PLDParams.PLDParamsFromGet.LD1_SKB:
                    {

                        SettingParamsToShow.LDParams[0].CalibSet.K = p1.SetParams.LDParams[0].CalibSet.K.ToString("F2");
                        SettingParamsToShow.LDParams[0].CalibSet.B = p1.SetParams.LDParams[0].CalibSet.B.ToString("F2");

                    }
                    break;
                case PLDParams.PLDParamsFromGet.LD2_SKB:
                    {

                        SettingParamsToShow.LDParams[1].CalibSet.K = p1.SetParams.LDParams[1].CalibSet.K.ToString("F2");
                        SettingParamsToShow.LDParams[1].CalibSet.B = p1.SetParams.LDParams[1].CalibSet.B.ToString("F2");

                    }
                    break;
                case PLDParams.PLDParamsFromGet.LD1_MKB:
                    {

                        SettingParamsToShow.LDParams[0].CalibMeasure.K = p1.SetParams.LDParams[0].CalibMeasure.K.ToString("F2");
                        SettingParamsToShow.LDParams[0].CalibMeasure.B = p1.SetParams.LDParams[0].CalibMeasure.B.ToString("F2");

                    }
                    break;
                case PLDParams.PLDParamsFromGet.LD2_MKB:
                    {
                        SettingParamsToShow.LDParams[1].CalibMeasure.K = p1.SetParams.LDParams[1].CalibMeasure.K.ToString("F2");
                        SettingParamsToShow.LDParams[1].CalibMeasure.B = p1.SetParams.LDParams[1].CalibMeasure.B.ToString("F2");
                    }
                    break;
                case PLDParams.PLDParamsFromGet.PD_MKB:
                    {
                        SettingParamsToShow.CalibPD.K = p1.SetParams.CalibPD.K.ToString("F2");
                        SettingParamsToShow.CalibPD.B = p1.SetParams.CalibPD.B.ToString("F2");
                    }
                    break;
                case PLDParams.PLDParamsFromGet.TEC1_PID: idx = 0; goto SetPID;
                case PLDParams.PLDParamsFromGet.TEC2_PID: idx = 1; goto SetPID;
                case PLDParams.PLDParamsFromGet.TEC3_PID: idx = 2; goto SetPID;
                case PLDParams.PLDParamsFromGet.TEC4_PID:
                    idx = 3; goto SetPID;
                SetPID:
                    {
                        SettingParamsToShow.TECParams[idx].PID.P = p1.SetParams.TECParams[idx].PID.P.ToString("F2");
                        SettingParamsToShow.TECParams[idx].PID.I = p1.SetParams.TECParams[idx].PID.I.ToString("F2");
                        SettingParamsToShow.TECParams[idx].PID.D = p1.SetParams.TECParams[idx].PID.D.ToString("F2");
                    }
                    break;

                case PLDParams.PLDParamsFromGet.S_LCM:
                    {
                        SettingParamsToShow.LCMParams.IsPowerOn = p1.SetParams.LCMParams.IsPowerOn == PLDParams.WorkType.ON ? "开" : "关";
                        SettingParamsToShow.LCMParams.IsIsMotorWork = p1.SetParams.LCMParams.IsIsMotorWork == PLDParams.WorkType.ON ? "开" : "关";
                        SettingParamsToShow.LCMParams.MotorSpeed = p1.SetParams.LCMParams.MotorSpeed.ToString();
                        //SettingParamsToShow.LCMParams.FanSpeed = p1.SetParams.LCMParams.FanSpeed.ToString();
                        SettingParamsToShow.LCMParams.FanSpeed = p1.SetParams.LCMParams.FanSpeed == PLDParams.WorkType.ON ? "开" : "关";
                        SettingParamsToShow.LCMParams.PwrLimit = p1.SetParams.LCMParams.PwrLimit.ToString();
                        MeasureParams.StatusInfo.PumpStatus.Value = p1.SetParams.LCMParams.IsPowerOn == PLDParams.WorkType.ON ? "开" : "关";
                        //MeasureParams.StatusInfo.FanStatus.Value = p1.SetParams.LCMParams.FanSpeed > 0 ? "开" : "关";
                        MeasureParams.StatusInfo.FanStatus.Value = p1.SetParams.LCMParams.FanSpeed == PLDParams.WorkType.ON ? "开" : "关";
                    }
                    break;
                case PLDParams.PLDParamsFromGet.M_LCM:
                    {
                     
                            MeasureParams.LCMParams.Vol.Value = p1.MeasureParams.LCMParams.Vol.ToString();
                            MeasureParams.LCMParams.Curr.Value = p1.MeasureParams.LCMParams.Curr.ToString();
                            MeasureParams.LCMParams.Power.Value = p1.MeasureParams.LCMParams.Power.ToString();
                            MeasureParams.LCMParams.MotorSpeed.Value = p1.MeasureParams.LCMParams.MotorSpeed.ToString();
                            MeasureParams.LCMParams.Temp.Value = p1.MeasureParams.LCMParams.Temp.ToString("F1");
                        
                    }
                    break;
                case PLDParams.PLDParamsFromGet.SaveParam:
                    if (p1.MeasureParams.ParamSaveStatus)
                    {
                        Application.Current.Dispatcher.Invoke(() => aggregator.SendMessage("参数保存成功!", "Main",Type: MessageModel.MessageType.Success, TimeSpan: 4.0));                       
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() => aggregator.SendMessage("参数保存失败!", "Main", Type: MessageModel.MessageType.Error, TimeSpan: 4.0));
          
                    }
                    break;

                case PLDParams.PLDParamsFromGet.UpgradeResult:

                    switch (p1.MeasureParams.UpgradeResult)
                    {
                        case UpgradeResult.NONE:
                            Update.LastResult = "NA";
                            break;
                        case UpgradeResult.Pending:
                            Update.LastResult = "待升级";
                            break;
                        case UpgradeResult.Applied:
                            Update.LastResult = "升级成功";
                            break;
                        case UpgradeResult.Failed:
                            Update.LastResult = "升级失败";
                            break;
                        case UpgradeResult.BootMode:
                            Update.LastResult = "NA";
                            break;
                        default:
                            break;
                    }
                    break;


                default:
                    break;
            }


        }

        void DevSetParam(PLDParams.PLDParamsToSet cmd/*, params object[] param*/)
        {
            List<byte> data = protocolManager.GetSendCommand(cmd, pLDParams);
            SendData(data);
        }
        void DevSetParam(PLDParams.PLDParamsToSet cmd, params object[] param)
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
