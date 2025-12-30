using Microsoft.Win32;
using ScottPlot.Colormaps;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using wpfApp.Common.CtrlProtocol;
using wpfApp.Common.PLDProtocol;
using wpfApp.Extensions;
using static wpfApp.Common.PLDProtocol.PLDParams;

namespace wpfApp.ViewModels
{
    public partial class PLDMainViewModel
    {

        private ProtocolManager2 protocolManager = new ProtocolManager2();




        private List<PLDParams.TECParams> Tecparams { get; set; }
        void DevParamIni()
        {
            Tecparams = new List<PLDParams.TECParams>();
            for (int i = 0; i < 4; i++)//todo初始值转换
            {
                Tecparams.Add(new PLDParams.TECParams() { Temp=20});
            }
        }

        PLDParams.TrigType TransWorkMode(string mode)
        {
            if (mode == "外调制" )
                return PLDParams.TrigType.OUT;
            return PLDParams.TrigType.INTER;
        }

        void DevClose()
        {
            try
            {
                bitTimer.Stop();                
                connectTimer.Stop();
                MeasureParams.StatusInfo.ConnectStatus.Value = "未连接";
                SettingParamsToShow.LDParams[0].IsWork = false;
                SettingParamsToShow.LDParams[1].IsWork = false;
                SettingParamsToShow.TECParams[0].IsWork = false;
                SettingParamsToShow.TECParams[1].IsWork = false;
                SettingParamsToShow.TECParams[2].IsWork = false;
                SettingParamsToShow.TECParams[3].IsWork = false;

                DeviceSerialPort.Close();
                DeviceIsOpen = DeviceSerialPort.IsOpen;
                IsUpdatePara = false;
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => aggregator.SendMessage(ex.Message, "Main"));

            }
        }
        void DevSetMode()
        {
            DevSetParam(PLDParams.PLDParamsToSet.TRG_TYPE, (byte)TransWorkMode(SettingParams.Mode));
            DevQueryParam(PLDParams.PLDParamsToQuery.TRG_TYPE);
        }

        PLDParams.PulseType TransPulseType(string mode)
        {
            if (mode == "变频")
                return PLDParams.PulseType.SPWM;
            return PLDParams.PulseType.NOR;
        }

        void DevSetPulseType()
        {
            DevSetParam(PLDParams.PLDParamsToSet.PulseType, (byte)TransPulseType(SettingParams.PulseType));
            DevQueryParam(PLDParams.PLDParamsToQuery.PulseType);
        }

        byte[]? ConvertFloatStringToS16(string s, int factor = 10)
        {
            try
            {
                Int16 value = (Int16)(Convert.ToSingle(s) * factor);
                return BitConverter.GetBytes(value);
            }
            catch (Exception ex)
            {
                aggregator.SendMessage(ex.Message, "Main");
                return null;
            }
        }
        byte[]? ConvertFloatStringToU16(string s, int factor = 10)
        {
            try
            {
                UInt16 value = (UInt16)(Convert.ToSingle(s) * factor);
                return BitConverter.GetBytes(value);
            }
            catch (Exception ex)
            {
                aggregator.SendMessage(ex.Message, "Main");
                return null;
            }
        }



        //byte[]? ConvertFloatString<T>(string s, int factor = 10) where T : struct
        //{
        //    try
        //    {
        //        float floatValue = Convert.ToSingle(s) * factor;
        //        dynamic t = Convert.ChangeType(floatValue, typeof(T));
        //        return BitConverter.GetBytes(t);
        //    }
        //    catch (Exception ex)
        //    {
        //        aggregator.SendMessage(ex.Message, "Main");
        //        return null;
        //    }
        //}

        byte[]? ConvertFloatString<T>(string s, int factor = 10 ,float maxValue = float.MaxValue,float minValue = float.MinValue) where T : struct
        {
            try
            {
                float floatValue = Convert.ToSingle(s) * factor;
                if (floatValue> maxValue|| floatValue< minValue)
                {
                    aggregator.SendMessage($"输入值超出范围( {minValue} ~ {maxValue} )。\r\n", "Main");
                    return null;
                }
                dynamic t = Convert.ChangeType(floatValue, typeof(T));
                return BitConverter.GetBytes(t);
            }
            catch (Exception ex)
            {
                aggregator.SendMessage(ex.Message, "Main");
                return null;
            }
        }


        void DevSetLD1Curr()
        {
           // byte[]? bytes = ConvertFloatStringToU16(SettingParams.LDParams[0].Curr);
            byte[]? bytes = ConvertFloatString<UInt16>(SettingParams.LDParams[0].Curr);
            if (bytes != null)
            {
                DevSetParam(PLDParams.PLDParamsToSet.LD1_S_Cur, bytes);
            }
            DevQueryParam(PLDParams.PLDParamsToQuery.LD1_S_Cur);

        }
        void DevSetLD2Curr()
        {
            byte[]? bytes = ConvertFloatStringToU16(SettingParams.LDParams[1].Curr);
            if (bytes != null)
            {
                DevSetParam(PLDParams.PLDParamsToSet.LD2_S_Cur, bytes);
            }
            DevQueryParam(PLDParams.PLDParamsToQuery.LD2_S_Cur);
        }
        void DevSetTECxTemp(int idx)
        {
            try
            {
                Tecparams[idx].Temp = Convert.ToSingle(SettingParams.TECParams[idx].Temp) * 10;
                List <byte> bytes = new List<byte>();
                bytes.AddRange(BitConverter.GetBytes((Int16)Tecparams[idx].Temp));
                bytes.AddRange(BitConverter.GetBytes((Int16)Tecparams[idx].Vol));
                bytes.AddRange(BitConverter.GetBytes((Int16)Tecparams[idx].Mode));
                switch (idx)
                {
                    case 0: DevSetParam(PLDParams.PLDParamsToSet.TEC1_PARA, bytes.ToArray()); DevQueryParam(PLDParams.PLDParamsToQuery.TEC1_PARA); break;
                    case 1: DevSetParam(PLDParams.PLDParamsToSet.TEC2_PARA, bytes.ToArray()); DevQueryParam(PLDParams.PLDParamsToQuery.TEC2_PARA); break;
                    case 2: DevSetParam(PLDParams.PLDParamsToSet.TEC3_PARA, bytes.ToArray()); DevQueryParam(PLDParams.PLDParamsToQuery.TEC3_PARA); break;
                    case 3: DevSetParam(PLDParams.PLDParamsToSet.TEC4_PARA, bytes.ToArray()); DevQueryParam(PLDParams.PLDParamsToQuery.TEC4_PARA); break;
                    default:
                        break;
                }
                
            }
            catch (Exception ex)
            {
                aggregator.SendMessage(ex.Message, "Main");
            }
        }

        void DevSetTQSW()
        {
            byte data = SettingParams.TQParams.IsWork == "开" ? (byte)PLDParams.WorkType.ON : (byte)PLDParams.WorkType.OFF;
            DevSetParam(PLDParams.PLDParamsToSet.Q_SW, data);
            DevQueryParam(PLDParams.PLDParamsToQuery.Q_SW);
        }

        void DevSetTECxSW(int idx)
        {
            byte data = SettingParamsToShow.TECParams[idx].IsWork == true ? (byte)PLDParams.WorkType.OFF : (byte)PLDParams.WorkType.ON;
            switch (idx)
            {
                case 0: DevSetParam(PLDParams.PLDParamsToSet.TEC1_SW, data); DevQueryParam(PLDParams.PLDParamsToQuery.TEC1_SW); break;
                case 1: DevSetParam(PLDParams.PLDParamsToSet.TEC2_SW, data); DevQueryParam(PLDParams.PLDParamsToQuery.TEC2_SW); break;
                case 2: DevSetParam(PLDParams.PLDParamsToSet.TEC3_SW, data); DevQueryParam(PLDParams.PLDParamsToQuery.TEC3_SW); break;
                case 3: DevSetParam(PLDParams.PLDParamsToSet.TEC4_SW, data); DevQueryParam(PLDParams.PLDParamsToQuery.TEC4_SW); break;
                default:
                    break;
            }
        }
        void DevSetLDxSW(int idx)
        {
            byte data = SettingParamsToShow.LDParams[idx].IsWork == true ? (byte)PLDParams.WorkType.OFF : (byte)PLDParams.WorkType.ON;
            switch (idx)
            {
                case 0: DevSetParam(PLDParams.PLDParamsToSet.LD1_SW, data); DevQueryParam(PLDParams.PLDParamsToQuery.LD1_SW); break;
                case 1: DevSetParam(PLDParams.PLDParamsToSet.LD2_SW, data); DevQueryParam(PLDParams.PLDParamsToQuery.LD2_SW); break;
                default:
                    break;
            }
        }
      

        void DevSetTQDelay()
        {
            byte[]? bytes = ConvertFloatString<UInt16>(SettingParams.TQParams.Delay, factor: 1, maxValue: 300, minValue: 1);
            if (bytes != null)
            {
                DevSetParam(PLDParams.PLDParamsToSet.Q_DELAY, bytes);
            }
            DevQueryParam(PLDParams.PLDParamsToQuery.Q_DELAY);
        }
        
        

        
        void DevSetPulseWidth()
        {
            byte[]? bytes = ConvertFloatString<UInt16>(SettingParams.PulseParams.Width, factor: 1, minValue: 200, maxValue: 240);
            if (bytes != null)
            {
                DevSetParam(PLDParams.PLDParamsToSet.PULSE_WIDTH, bytes);
            }
            DevQueryParam(PLDParams.PLDParamsToQuery.PULSE_WIDTH);
        }
        void DevSetPulseFreq()
        {
            byte[]? bytes = ConvertFloatString<UInt16>(SettingParams.PulseParams.Freq, factor: 1, minValue: 1, maxValue: 1000);
            if (bytes != null)
            {
                DevSetParam(PLDParams.PLDParamsToSet.INTER_TRG_FREQ, bytes);
            }
            DevQueryParam(PLDParams.PLDParamsToQuery.INTER_TRG_FREQ);
        }
        void DevSetPulseParam()
        {
            try
            {
                PLDParams.PulseParams pulseParams = new PLDParams.PulseParams();
                List<byte> bytes = new List<byte>();
                pulseParams.Num = Convert.ToUInt16(SettingParams.PulseParams.Num);
                bytes.AddRange(BitConverter.GetBytes(pulseParams.Num));
                for (int i = 0; i < 11; i++)
                {
                    pulseParams.Interval[i] = Convert.ToUInt16(SettingParams.PulseParams.Interval[i].Value);
                    bytes.AddRange(BitConverter.GetBytes(pulseParams.Interval[i]));
                }   
                DevSetParam(PLDParams.PLDParamsToSet.PULSE_PARA, bytes.ToArray());
                DevQueryParam(PLDParams.PLDParamsToQuery.PULSE_PARA);
            }
            catch (Exception ex)
            {
                aggregator.SendMessage(ex.Message, "Main");
            }
        }

        void DevSetLCM()
        {
            try
            {

                PLDParams.LCMSetParams lCMSetParams = new PLDParams.LCMSetParams();
                lCMSetParams.PwrEn = SettingParams.LCMParams.IsPowerOn == "开" ? (byte)PLDParams.WorkType.ON : (byte)PLDParams.WorkType.OFF;
                lCMSetParams.MotorEn = SettingParams.LCMParams.IsIsMotorWork == "开" ? (byte)PLDParams.WorkType.ON : (byte)PLDParams.WorkType.OFF;
                lCMSetParams.MotorSpeed = Convert.ToUInt16(SettingParams.LCMParams.MotorSpeed);
                lCMSetParams.Fan = Convert.ToByte(SettingParams.LCMParams.FanSpeed);
                lCMSetParams.PwrLimit = Convert.ToByte(SettingParams.LCMParams.PwrLimit);
                List<byte> data = new List<byte>();
                data.Add(lCMSetParams.PwrEn);
                data.Add(lCMSetParams.MotorEn);
                data.AddRange(BitConverter.GetBytes(lCMSetParams.MotorSpeed));
                data.Add(lCMSetParams.Fan);
                data.Add(lCMSetParams.PwrLimit);
                DevSetParam(PLDParams.PLDParamsToSet.LCM, data.ToArray());
                DevQueryParam(PLDParams.PLDParamsToQuery.S_LCM);
            }
            catch (Exception ex)
            {
                aggregator.SendMessage(ex.Message, "Main");
            }
        }

        void DevSetLDxHOC(int idx)
        {
            // byte[]? bytes = ConvertFloatStringToU16(SettingParams.LDParams[0].Curr);
            byte[]? bytes = ConvertFloatString<UInt16>(SettingParams.LDParams[idx].HOC);
            if (bytes != null)
            {
                switch (idx)
                {
                    case 0: DevSetParam(PLDParams.PLDParamsToSet.LD1_HOC, bytes); DevQueryParam(PLDParams.PLDParamsToQuery.LD1_HOC); break;
                    case 1: DevSetParam(PLDParams.PLDParamsToSet.LD2_HOC, bytes); DevQueryParam(PLDParams.PLDParamsToQuery.LD2_HOC); break;
                    default:
                        break;
                }
            }
        }
        void DevSetLDxVol(int idx)
        {
          
            byte[]? bytes = ConvertFloatString<UInt16>(SettingParams.LDParams[idx].Vol);
            if (bytes != null)
            {
                switch (idx)
                {
                    case 0: DevSetParam(PLDParams.PLDParamsToSet.LD1_Vol, bytes); DevQueryParam(PLDParams.PLDParamsToQuery.LD1_Vol); break;
                    case 1: DevSetParam(PLDParams.PLDParamsToSet.LD2_Vol, bytes); DevQueryParam(PLDParams.PLDParamsToQuery.LD2_Vol); break;
                    default:
                        break;
                }
            }
        }

        void DevSetKBParam(PLDParams.PLDParamsToSet pLDParamsToSet, PLDParams.PLDParamsToQuery pLDParamsToQuery, CalibParam calibParam)
        {
            try
            {
                List<byte> data = new List<byte>();
                Int16 k = (Int16)(Convert.ToSingle(calibParam.K) * 100);
                data.AddRange(BitConverter.GetBytes(k));
                Int16 b = (Int16)(Convert.ToSingle(calibParam.B) * 100);
                data.AddRange(BitConverter.GetBytes(b));
                DevSetParam(pLDParamsToSet, data.ToArray());
                DevQueryParam(pLDParamsToQuery);
            }
            catch (Exception ex)
            {
                aggregator.SendMessage(ex.Message, "Main");
            }
        }
        void DevSetPIDParam(PLDParams.PLDParamsToSet pLDParamsToSet, PLDParams.PLDParamsToQuery pLDParamsToQuery, PIDCalibParam param)
        {
            try
            {
                List<byte> data = new List<byte>();
                Int16 p = (Int16)(Convert.ToSingle(param.P) * 100);
                data.AddRange(BitConverter.GetBytes(p));
                Int16 i = (Int16)(Convert.ToSingle(param.I) * 100);
                data.AddRange(BitConverter.GetBytes(i));
                Int16 d = (Int16)(Convert.ToSingle(param.D) * 100);
                data.AddRange(BitConverter.GetBytes(d));
                DevSetParam(pLDParamsToSet, data.ToArray());
                DevQueryParam(pLDParamsToQuery);
            }
            catch (Exception ex)
            {
                aggregator.SendMessage(ex.Message, "Main");
            }
        }
        async void DevParamUpdata()
        {
            if (!IsUpdatePara)
            {
                IsUpdatePara = true;
                foreach (var item in Enum.GetValues(typeof(PLDParams.PLDParamsToQuery)))
                {
                    if ((PLDParams.PLDParamsToQuery)item == PLDParams.PLDParamsToQuery.ALL_SET)
                    {
                        IsUpdatePara = false;
                        return;
                    }
                    DevQueryParam(((PLDParams.PLDParamsToQuery)item));
                    await Task.Delay(200);
                }
            }
        }
        void DevClearErr()
        {
            DevSetParam(PLDParamsToSet.ClearErr);
        }

        string getDataFilePath()
        {

            DateTime now = DateTime.Now;

            // 创建以年月命名的目录
            string directoryPath = Path.Combine(FileSavePath, now.ToString("yyyy-MM"));
            // 创建以年月日命名的文件
            string fileName = now.ToString("yyyy-MM-dd") + ".csv";
            string filePath = Path.Combine(directoryPath, fileName);

            try
            {
                // 检查目录是否存在
                if (!Directory.Exists(directoryPath))
                {
                    // 创建目录
                    Directory.CreateDirectory(directoryPath);
                    // Console.WriteLine("目录创建成功: " + directoryPath);
                }
                if (!File.Exists(filePath))
                {
                    using (StreamWriter streamWriter = new StreamWriter(filePath, append: true, encoding: Encoding.UTF8))
                    {
                        //if (devType == DevType.X002_001)
                        //{
                        //    streamWriter.WriteLine("时间,LD电流(mA),LD电压(mV),PIN背光电流(uA),LD出光功率(mW),NTC值(Ω),LD温度(℃),TEC电流(mA)");
                        //}
                        //else if (devType == DevType.X002_002)
                        //{
                        //    streamWriter.WriteLine("时间,LD电流(mA),LD电压(mV),PIN背光电流(uA),LD出光功率(mW),NTC值(Ω),LD温度(℃),TEC电流(mA),R-温度(℃),AHT20湿度(%),AHT20温度(℃)");
                        //}

                    }
                }

                return filePath;
            }
            catch (Exception ex)
            {
                //Console.WriteLine("操作时发生错误: " + ex.Message);
                return null;
            }
        }
        void saveDataFile(PLDParams data)
        {
            string filePath = getDataFilePath();
            if (filePath == null)
                return;
            using (StreamWriter streamWriter = new StreamWriter(filePath, append: true, encoding: Encoding.UTF8))
            {
                try
                {
                    //if (devType == DevType.X002_001)
                    //{
                    //    streamWriter.WriteLine("{0:yyyy年MM月dd日 HH:mm:ss.fff},{1:F4},{2:F4},{3:F4},{4:F4},{5:F1},{6:F4},{7:F1}",
                    //    data.time, data.LD_curr, data.LD_vol, data.PIN_curr, data.power, data.NTC_vaule, data.temperature, data.TEC_curr);
                    //}
                    //else if (devType == DevType.X002_002)
                    //{
                    //    streamWriter.WriteLine("{0:yyyy年MM月dd日 HH:mm:ss.fff},{1:F4},{2:F4},{3:F4},{4:F4},{5:F1},{6:F4},{7:F1},{8:F2},{9:F2},{10:F2}",
                    //    data.time, data.LD_curr, data.LD_vol, data.PIN_curr, data.power, data.NTC_vaule, data.temperature, data.TEC_curr, data.Board_temp, data.AHT20_humidity, data.AHT20_temp);
                    //}

                }
                catch (Exception)
                {

                }
            }
        }

        public void FolderPiker()
        {
            var dlg = new PickupFolderDialog();
            if (dlg.ShowDialog())
            {
                FileSavePath = dlg.SelectedPath;

            }

        }
        public void FilePicker()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (Update.FilePath != null && ((new FileInfo(Update.FilePath)).Exists))
            {
                ofd.InitialDirectory = new FileInfo(Update.FilePath).DirectoryName;
                //ofd.FileName = InputConfi.FilePath;
            }
            ofd.Filter = "BIN(*.BIN, *bin)|*.BIN;*bin" +
                        "|All files (*.*)|*.*";

            if (ofd.ShowDialog() == true)
            {
                Update.FilePath = ofd.FileName;                
            }
        }
        public void GoToSurce(string obj)
        {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = "explorer.exe";
            switch (obj)
            {
                case "打开目录": proc.StartInfo.Arguments = FileSavePath; break;
                case "打开升级文件目录": proc.StartInfo.Arguments = new FileInfo(Update.FilePath).DirectoryName; break;
                default:
                    break;
            }
            proc.Start();
        }
      


        private void Execute(string obj)
        {
            switch (obj)
            {
                case "连接设备": DeviceConnect(); break;
                case "扫描端口": UpdataSerialPort(); break;
                case "设置模式": DevSetMode(); break;
                case "设置脉冲类型": DevSetPulseType(); break;
                case "设置LD1电流": DevSetLD1Curr(); break;
                case "设置LD2电流": DevSetLD2Curr(); break;
                case "设置LD1温度": SettingParams.TECParams[0].Temp = SettingParams.LDParams[0].Temp;  DevSetTECxTemp(0); break;
                case "设置LD2温度": SettingParams.TECParams[1].Temp = SettingParams.LDParams[1].Temp; DevSetTECxTemp(1); break;
                case "设置晶体1温度": DevSetTECxTemp(2); break;
                case "设置晶体2温度": DevSetTECxTemp(3); break;
                case "设置调Q开关": DevSetTQSW(); break;
                case "设置调Q时延": DevSetTQDelay(); break;
                case "TEC1启动": DevSetTECxSW(0); break;
                case "TEC2启动": DevSetTECxSW(1); break;
                case "TEC3启动": DevSetTECxSW(2); break;
                case "TEC4启动": DevSetTECxSW(3); break;
                case "LD1启动": DevSetLDxSW(0); break;
                case "LD2启动": DevSetLDxSW(1); break;
                case "设置脉冲宽度": DevSetPulseWidth(); break;
                case "设置脉冲定频": DevSetPulseFreq(); break;
                case "设置脉冲串参数": DevSetPulseParam(); break;
                case "设置液冷参数": DevSetLCM(); break;
                case "设置LD1过流保护": DevSetLDxHOC(0); break;
                case "设置LD2过流保护": DevSetLDxHOC(1); break;       
                case "设置LD1电压": DevSetLDxVol(0); break;
                case "设置LD2电压": DevSetLDxVol(1); break;
                case "设置LD1SKB": DevSetKBParam(PLDParams.PLDParamsToSet.LD1_SKB, PLDParams.PLDParamsToQuery.LD1_SKB, SettingParams.LDParams[0].CalibSet); break;
                case "设置LD2SKB": DevSetKBParam(PLDParams.PLDParamsToSet.LD2_SKB, PLDParams.PLDParamsToQuery.LD2_SKB, SettingParams.LDParams[1].CalibSet); break;
                case "设置LD1MKB": DevSetKBParam(PLDParams.PLDParamsToSet.LD1_MKB, PLDParams.PLDParamsToQuery.LD1_MKB, SettingParams.LDParams[0].CalibMeasure); break;
                case "设置LD2MKB": DevSetKBParam(PLDParams.PLDParamsToSet.LD2_MKB, PLDParams.PLDParamsToQuery.LD2_MKB, SettingParams.LDParams[1].CalibMeasure); break;
                case "设置PDKB": DevSetKBParam(PLDParams.PLDParamsToSet.PD_MKB, PLDParams.PLDParamsToQuery.PD_MKB, SettingParams.CalibPD); break;
                case "设置TEC1PID": DevSetPIDParam(PLDParams.PLDParamsToSet.TEC1_PID, PLDParams.PLDParamsToQuery.TEC1_PID, SettingParams.TECParams[0].PID); break;
                case "设置TEC2PID": DevSetPIDParam(PLDParams.PLDParamsToSet.TEC2_PID, PLDParams.PLDParamsToQuery.TEC2_PID, SettingParams.TECParams[1].PID); break;
                case "设置TEC3PID": DevSetPIDParam(PLDParams.PLDParamsToSet.TEC3_PID, PLDParams.PLDParamsToQuery.TEC3_PID, SettingParams.TECParams[2].PID); break;
                case "设置TEC4PID": DevSetPIDParam(PLDParams.PLDParamsToSet.TEC4_PID, PLDParams.PLDParamsToQuery.TEC4_PID, SettingParams.TECParams[3].PID); break;

                case "参数更新": DevParamUpdata(); break;
                case "参数固化": DevSetParam(PLDParams.PLDParamsToSet.SaveParam); break;

                case "设置目录": FolderPiker(); break;
                case "打开目录": GoToSurce(obj); break;
                case "选择升级文件": FilePicker(); break;
                case "打开升级文件目录": GoToSurce(obj); break;
                case "清空错误": DevClearErr(); break;

                //case "设置温度": DevSetParamFloatString(DevParamsToSet.LDTemp, DevParaSetting.LDTemp); break;
                //case "设置电流": DevSetParamFloatString(DevParamsToSet.LDCurr, DevParaSetting.LDCurr); break;
                //case "设置功率": DevSetParamFloatString(DevParamsToSet.LDPower, DevParaSetting.LDPower); break;
                //case "TEC启动": DevSetTECLaunch(); break;
                //case "LD启动": DevSetLDLaunch(); break;
                //case "设置LD最大电流": DevSetParamFloatString(DevParamsToSet.LDMaxCurr, DevParaSetting.LDMaxCurr); break;
                //case "设置LD过流保护": DevSetParamFloatString(DevParamsToSet.LDHOC, DevParaSetting.LDHOC); break;
                //case "设置LD测量电阻": DevSetParamFloatString(DevParamsToSet.LDMeasureR, DevParaSetting.LDMeasureR); break;
                //case "设置TEC最低温度": DevSetParamFloatString(DevParamsToSet.TECMinTemp, DevParaSetting.TECMinTemp); break;
                //case "设置TEC最高温度": DevSetParamFloatString(DevParamsToSet.TECMaxTemp, DevParaSetting.TECMaxTemp); break;
                //case "设置TEC温度偏置": DevSetParamFloatString(DevParamsToSet.TECTempOffset, DevParaSetting.TECTempOffset); break;
                //case "设置TEC最大电压": DevSetParamFloatString(DevParamsToSet.TECMaxVol, DevParaSetting.TECMaxVol); break;
                //case "设置开机启动": DevSetAutoRun(); break;


                //case "打开数据曲线": ShowDataChart(); break;
                //case "设置目录": FolderPiker(); break;
                //case "打开目录": GoToSurce(); break;
                default: break;
            }
        }



    }
}
