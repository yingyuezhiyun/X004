using DryIoc.ImTools;
using Microsoft.Win32;
using ScottPlot.Colormaps;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using wpfApp.Common.CtrlProtocol;
using wpfApp.Common.Dialog;
using wpfApp.Common.Events;
using wpfApp.Extensions;
using static wpfApp.Common.CtrlProtocol.PLDParams;

namespace wpfApp.ViewModels
{
    public partial class PLDMainViewModel
    {

        private ProtocolManager protocolManager = new ProtocolManager();


        PLDParams pLDParams = new PLDParams();

        PLDParams.SetParam setParam;
       
    

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
            setParam.TrigType = TransWorkMode(SettingParams.Mode);
            DevSetParam(PLDParams.PLDParamsToSet.TRG_TYPE);
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
            setParam.PulseType = TransPulseType(SettingParams.PulseType);
            DevSetParam(PLDParams.PLDParamsToSet.PulseType);
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
                aggregator.SendMessage(ex.Message, "Main", Type: MessageModel.MessageType.Warning);
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
                aggregator.SendMessage(ex.Message, "Main", Type: MessageModel.MessageType.Warning);
                return null;
            }
        }

 

        byte[]? ConvertFloatString<T>(string s, int factor = 10 ,float maxValue = float.MaxValue,float minValue = float.MinValue) where T : struct
        {
            try
            {
                float floatValue = Convert.ToSingle(s) * factor;
                if (floatValue> maxValue|| floatValue< minValue)
                {
                    aggregator.SendMessage($"输入值超出范围( {minValue} ~ {maxValue} )。\r\n", "Main", Type: MessageModel.MessageType.Warning);
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

        float ? ConvertStringFloat(string s,float minValue = float.MinValue,float maxValue = float.MaxValue)
        {
            try
            {
                float floatValue = Convert.ToSingle(s);
                if (floatValue > maxValue || floatValue < minValue)
                {
                    aggregator.SendMessage($"输入值({floatValue})超出范围( {minValue} ~ {maxValue} )。\r\n", "Main", Type: MessageModel.MessageType.Warning);
                    return null;
                }
                return floatValue;
            }
            catch (Exception ex)
            {
                aggregator.SendMessage(ex.Message, "Main");
                return null;
            }
        }
        int? ConvertStringInt(string s, int minValue = int.MinValue, int maxValue = int.MaxValue)
        {
            try
            {
                int Value = Convert.ToInt32(s);
                if (Value > maxValue || Value < minValue)
                {
                    aggregator.SendMessage($"输入值({Value})超出范围( {minValue} ~ {maxValue} )。\r\n", "Main", Type: MessageModel.MessageType.Warning);
                    return null;
                }
                return Value;
            }
            catch (Exception ex)
            {
                aggregator.SendMessage(ex.Message, "Main");
                return null;
            }
        }


        void DevSetLD1Curr()
        {
            var p = ConvertStringFloat(SettingParams.LDParams[0].Curr);
            if (p != null)
            {
                setParam.LDParams[0].Curr = (float)p;
                DevSetParam(PLDParams.PLDParamsToSet.LD1_S_Cur);
                DevQueryParam(PLDParams.PLDParamsToQuery.LD1_S_Cur);
            }
        }
        void DevSetLD2Curr()
        {
            var p = ConvertStringFloat(SettingParams.LDParams[1].Curr);
            if (p != null)
            {
                setParam.LDParams[1].Curr = (float)p;
                DevSetParam(PLDParams.PLDParamsToSet.LD2_S_Cur);
                DevQueryParam(PLDParams.PLDParamsToQuery.LD2_S_Cur);
            }
        }
        void DevSetTECxMaxVol(int idx)
        {
            var p = ConvertStringFloat(SettingParams.TECParams[idx].Vol);
            PLDParams pLDParams = new PLDParams();
            if (p != null)
            {
                pLDParams.SetParams.TECParams[idx].Temp =setParam.TECParams[idx].Temp;
                pLDParams.SetParams.TECParams[idx].Vol = (float)p;
                switch (idx)
                {
                    case 0: DevSetParam(PLDParams.PLDParamsToSet.TEC1_PARA, pLDParams); DevQueryParam(PLDParams.PLDParamsToQuery.TEC1_PARA); break;
                    case 1: DevSetParam(PLDParams.PLDParamsToSet.TEC2_PARA, pLDParams); DevQueryParam(PLDParams.PLDParamsToQuery.TEC2_PARA); break;
                    case 2: DevSetParam(PLDParams.PLDParamsToSet.TEC3_PARA, pLDParams); DevQueryParam(PLDParams.PLDParamsToQuery.TEC3_PARA); break;
                    case 3: DevSetParam(PLDParams.PLDParamsToSet.TEC4_PARA, pLDParams); DevQueryParam(PLDParams.PLDParamsToQuery.TEC4_PARA); break;
                    default:
                        break;
                }
            }
        }


        void DevSetTECxTemp(int idx)
        {
            var p = ConvertStringFloat(SettingParams.TECParams[idx].Temp);
            if (p != null)
            {
                setParam.TECParams[idx].Temp = (float)p;
                switch (idx)
                {
                    case 0: DevSetParam(PLDParams.PLDParamsToSet.TEC1_PARA); DevQueryParam(PLDParams.PLDParamsToQuery.TEC1_PARA); break;
                    case 1: DevSetParam(PLDParams.PLDParamsToSet.TEC2_PARA); DevQueryParam(PLDParams.PLDParamsToQuery.TEC2_PARA); break;
                    case 2: DevSetParam(PLDParams.PLDParamsToSet.TEC3_PARA); DevQueryParam(PLDParams.PLDParamsToQuery.TEC3_PARA); break;
                    case 3: DevSetParam(PLDParams.PLDParamsToSet.TEC4_PARA); DevQueryParam(PLDParams.PLDParamsToQuery.TEC4_PARA); break;
                    default:
                        break;
                }
            }
        }
        
        void DevSetTQSW()
        {
            setParam.TQParams.WorkType = SettingParams.TQParams.IsWork == "开" ?PLDParams.WorkType.ON : PLDParams.WorkType.OFF;
            DevSetParam(PLDParams.PLDParamsToSet.Q_SW);
            DevQueryParam(PLDParams.PLDParamsToQuery.Q_SW);
        }

        void DevSetTECxSW(int idx)
        {
            setParam.TECParams[idx].WorkType = SettingParamsToShow.TECParams[idx].IsWork == true ? PLDParams.WorkType.OFF : PLDParams.WorkType.ON;
            switch (idx)
            {
                case 0: DevSetParam(PLDParams.PLDParamsToSet.TEC1_SW); DevQueryParam(PLDParams.PLDParamsToQuery.TEC1_SW); break;
                case 1: DevSetParam(PLDParams.PLDParamsToSet.TEC2_SW); DevQueryParam(PLDParams.PLDParamsToQuery.TEC2_SW); break;
                case 2: DevSetParam(PLDParams.PLDParamsToSet.TEC3_SW); DevQueryParam(PLDParams.PLDParamsToQuery.TEC3_SW); break;
                case 3: DevSetParam(PLDParams.PLDParamsToSet.TEC4_SW); DevQueryParam(PLDParams.PLDParamsToQuery.TEC4_SW); break;
                default:
                    break;
            }
        }
        void DevSetLDxSW(int idx)
        {
            setParam.LDParams[idx].WorkType = SettingParamsToShow.LDParams[idx].IsWork == true ? PLDParams.WorkType.OFF :PLDParams.WorkType.ON;
            switch (idx)
            {
                case 0: DevSetParam(PLDParams.PLDParamsToSet.LD1_SW); DevQueryParam(PLDParams.PLDParamsToQuery.LD1_SW); break;
                case 1: DevSetParam(PLDParams.PLDParamsToSet.LD2_SW); DevQueryParam(PLDParams.PLDParamsToQuery.LD2_SW); break;
                default:
                    break;
            }
        }


        void DevSetTQDelay()
        {
            var p = ConvertStringFloat(SettingParams.TQParams.Delay, maxValue: 300, minValue: 1);
            if (p != null)
            {
                setParam.TQParams.Delay = (float)p;
                DevSetParam(PLDParams.PLDParamsToSet.Q_DELAY);
                DevQueryParam(PLDParams.PLDParamsToQuery.Q_DELAY);
            }
        }

        void DevSetPulseWidth()
        {
            var p = ConvertStringInt(SettingParams.PulseParams.Width, minValue: 200, maxValue: 240);
            if (p != null)
            {
                setParam.PulseParams.Width = (UInt16)p;
                DevSetParam(PLDParams.PLDParamsToSet.PULSE_WIDTH);
                DevQueryParam(PLDParams.PLDParamsToQuery.PULSE_WIDTH);
            }
        }
        void DevSetPulseFreq()
        {
            var p = ConvertStringInt(SettingParams.PulseParams.Freq, minValue: 1, maxValue: 1000);
            if (p != null)
            {
                setParam.PulseParams.Freq = (UInt16)p;
                DevSetParam(PLDParams.PLDParamsToSet.INTER_TRG_FREQ);
                DevQueryParam(PLDParams.PLDParamsToQuery.INTER_TRG_FREQ);
            }
           
        }
        void DevSetPulseParam()
        {
            try
            {
                UInt16 Value = 0;
                Value = Convert.ToUInt16(SettingParams.PulseParams.Num);
                if (Value > 11 || Value < 0)
                {
                    aggregator.SendMessage($"脉冲间隔个数({Value})超出范围( 0 ~ 11 )。\r\n", "Main", Type: MessageModel.MessageType.Warning);
                    return;
                }
                setParam.PulseParams.Num = Value;

                List<int> Values = new List<int>();
                for (int i = 0; i < 11; i++)
                {
                    Value = Convert.ToUInt16(SettingParams.PulseParams.Interval[i].Value);
                    if (Value > 660 || Value < 220)
                    {
                        aggregator.SendMessage($"脉冲间隔({Value})超出范围( 220 ~ 660 us)。\r\n", "Main",Type: MessageModel.MessageType.Warning);
                        return;
                    }
                    Values.Add(Value);
                }
                if (Values.Sum() > 5000)
                {
                    aggregator.SendMessage($"脉冲串总时长({Values.Sum()})超出范围( 5000 us)。\r\n", "Main", Type: MessageModel.MessageType.Warning);
                    return;
                }
                for (int i = 0; i < 11; i++)
                {
                    setParam.PulseParams.Interval[i] = (UInt16)Values[i];
                }
               
                DevSetParam(PLDParams.PLDParamsToSet.PULSE_PARA);
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
                setParam.LCMParams.IsPowerOn = SettingParams.LCMParams.IsPowerOn == "开" ? PLDParams.WorkType.ON : PLDParams.WorkType.OFF;
                setParam.LCMParams.IsIsMotorWork = SettingParams.LCMParams.IsIsMotorWork == "开" ? PLDParams.WorkType.ON : PLDParams.WorkType.OFF;
                setParam.LCMParams.MotorSpeed = Convert.ToUInt16(SettingParams.LCMParams.MotorSpeed);
                
                //setParam.LCMParams.FanSpeed = Convert.ToByte(SettingParams.LCMParams.FanSpeed);
                setParam.LCMParams.FanSpeed = SettingParams.LCMParams.FanSpeed == "开" ? PLDParams.WorkType.ON : PLDParams.WorkType.OFF;
                setParam.LCMParams.PwrLimit = Convert.ToByte(SettingParams.LCMParams.PwrLimit);
                DevSetParam(PLDParams.PLDParamsToSet.LCM);
                DevQueryParam(PLDParams.PLDParamsToQuery.S_LCM);
            }
            catch (Exception ex)
            {
                aggregator.SendMessage(ex.Message, "Main");
            }
        }

        void DevSetLDxHOC(int idx)
        {
            var p = ConvertStringFloat(SettingParams.LDParams[idx].HOC);
            if (p != null)
            {
                setParam.LDParams[idx].HOC = (float)p;
                switch (idx)
                {
                    case 0: DevSetParam(PLDParams.PLDParamsToSet.LD1_HOC); DevQueryParam(PLDParams.PLDParamsToQuery.LD1_HOC); break;
                    case 1: DevSetParam(PLDParams.PLDParamsToSet.LD2_HOC); DevQueryParam(PLDParams.PLDParamsToQuery.LD2_HOC); break;
                    default:
                        break;
                }
            }
        }
        void DevSetLDxVol(int idx)
        {
            var p = ConvertStringFloat(SettingParams.LDParams[idx].Vol);
            if (p != null)
            {
                setParam.LDParams[idx].Vol = (float)p;
                switch (idx)
                {
                    case 0: DevSetParam(PLDParams.PLDParamsToSet.LD1_Vol); DevQueryParam(PLDParams.PLDParamsToQuery.LD1_Vol); break;
                    case 1: DevSetParam(PLDParams.PLDParamsToSet.LD2_Vol); DevQueryParam(PLDParams.PLDParamsToQuery.LD2_Vol); break;
                    default:
                        break;
                }
            }
        }

        void DevSetKBParam(PLDParams.PLDParamsToSet setCmd, PLDParams.PLDParamsToQuery queryCmd,
            CalibParam viewset, PLDParams.SetParam.CalibParam paramset)
        {
            
            try
            {
                paramset.K = Convert.ToSingle(viewset.K);
                paramset.B = Convert.ToSingle(viewset.B);
                DevSetParam(setCmd);
                DevQueryParam(queryCmd);
            }
            catch (Exception ex)
            {
                aggregator.SendMessage(ex.Message, "Main");
            }
        }

        
        void DevSetTECxPID(int idx)
        {
            try
            {
                setParam.TECParams[idx].PID.P = Convert.ToSingle(SettingParams.TECParams[idx].PID.P);
                setParam.TECParams[idx].PID.I = Convert.ToSingle(SettingParams.TECParams[idx].PID.I);
                setParam.TECParams[idx].PID.D = Convert.ToSingle(SettingParams.TECParams[idx].PID.D);
                switch (idx)
                {
                    case 0: DevSetParam(PLDParams.PLDParamsToSet.TEC1_PID); DevQueryParam(PLDParams.PLDParamsToQuery.TEC1_PID); break;
                    case 1: DevSetParam(PLDParams.PLDParamsToSet.TEC2_PID); DevQueryParam(PLDParams.PLDParamsToQuery.TEC2_PID); break;
                    case 2: DevSetParam(PLDParams.PLDParamsToSet.TEC3_PID); DevQueryParam(PLDParams.PLDParamsToQuery.TEC3_PID); break;
                    case 3: DevSetParam(PLDParams.PLDParamsToSet.TEC4_PID); DevQueryParam(PLDParams.PLDParamsToQuery.TEC4_PID); break;
                    default:
                        break;
                }
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

                        streamWriter.WriteLine("时间,LD1电流(A),LD1电压(V),LD2电流(A),LD2电压(V)," +
                            "PD温度(℃),光功率(W)," +
                            "TEC1温度(℃),TEC1电流(A),TEC1功率(W)," +
                            "TEC2温度(℃),TEC2电流(A),TEC2功率(W)," +
                            "TEC3温度(℃),TEC3电流(A),TEC3功率(W)," +
                            "TEC4温度(℃),TEC4电流(A),TEC4功率(W),");

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
                    streamWriter.WriteLine("{0:yyyy年MM月dd日 HH:mm:ss.fff},{1:F1},{2:F1},{3:F1},{4:F1},{5:F1},{6:F1}, " +
                        "{7:F1},{8:F1},{9:F1}," +
                        "{10:F1},{11:F1},{12:F1},"+
                        "{13:F1},{14:F1},{15:F1}," +
                        "{16:F1},{17:F1},{18:F1},",
                    data.time, data.MeasureParams.LDParams[0].Curr, data.MeasureParams.LDParams[0].Vol,
                    data.MeasureParams.LDParams[1].Curr, data.MeasureParams.LDParams[1].Vol,
                    data.MeasureParams.PDParams.Temp, data.MeasureParams.PDParams.Power,
                    data.MeasureParams.TECParams[0].Temp, data.MeasureParams.TECParams[0].Curr, data.MeasureParams.TECParams[0].Power,
                    data.MeasureParams.TECParams[1].Temp, data.MeasureParams.TECParams[1].Curr, data.MeasureParams.TECParams[1].Power,
                    data.MeasureParams.TECParams[2].Temp, data.MeasureParams.TECParams[2].Curr, data.MeasureParams.TECParams[2].Power,
                    data.MeasureParams.TECParams[3].Temp, data.MeasureParams.TECParams[3].Curr, data.MeasureParams.TECParams[3].Power
                   );


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


        public static UInt32 CRC32_Uint32(ReadOnlySpan<byte> data)
        {
            if (data.Length == 0) return 0u;
            const UInt32 POLY = 0x04C11DB7u;
            UInt32 crc = 0xFFFFFFFFu;

            int words = (data.Length + 3) / 4;
            for (int wi = 0; wi < words; wi++)
            {
                UInt32 w = 0;
                int baseIdx = wi * 4;
                for (int b = 0; b < 4; b++)
                {
                    int idx = baseIdx + b;
                    byte v = (idx < data.Length) ? data[idx] : (byte)0;
                    w |= (UInt32)v << (8 * b); // little-endian pack
                }

                // process MSB-first inside word
                for (int byte_i = 3; byte_i >= 0; byte_i--)
                {
                    byte cur = (byte)((w >> (8 * byte_i)) & 0xFFu);
                    crc ^= (UInt32)cur << 24;
                    for (int bit = 0; bit < 8; bit++)
                    {
                        crc = (crc & 0x80000000u) != 0 ? (crc << 1) ^ POLY : (crc << 1);
                    }
                }
            }

            return crc;
        }
        public static UInt32 CRC32_Uint32(ReadOnlySpan<byte> data, int len)
        {
            if (len == 0) return 0u;
            const UInt32 POLY = 0x04C11DB7u;
            UInt32 crc = 0xFFFFFFFFu;

            int words = (len + 3) / 4;
            for (int wi = 0; wi < words; wi++)
            {
                UInt32 w = 0;
                int baseIdx = wi * 4;
                for (int b = 0; b < 4; b++)
                {
                    int idx = baseIdx + b;
                    byte v = (idx < len) ? data[idx] : (byte)0;
                    w |= (UInt32)v << (8 * b); // little-endian pack
                }

                // process MSB-first inside word
                for (int byte_i = 3; byte_i >= 0; byte_i--)
                {
                    byte cur = (byte)((w >> (8 * byte_i)) & 0xFFu);
                    crc ^= (UInt32)cur << 24;
                    for (int bit = 0; bit < 8; bit++)
                    {
                        crc = (crc & 0x80000000u) != 0 ? (crc << 1) ^ POLY : (crc << 1);
                    }
                }
            }

            return crc;
        }

        public static UInt32 CRC32(ReadOnlySpan<byte> data)
        {
            UInt32 crc = 0xFFFFFFFFu;
            List<byte> bytes = new List<byte>();
            bytes.AddRange(data);
            int add =(int)Math.Ceiling((double)data.Length / 4.0)*4 - data.Length;
            for (int i = 0; i < add; i++)
            {
                bytes.Add(0);
            }
            foreach (byte b in bytes)
            {
                crc ^= (UInt32)b << 24; // MSB-first, no reflection
                for (int i = 0; i < 8; i++)
                {
                    crc = (crc & 0x80000000u) != 0 ? (crc << 1) ^ 0x04C11DB7u : (crc << 1);
                }
            }
            return crc; // no final xor
        }


        async void UpgradeStatus(PLDParams.UpgradeStatus status, UInt16 CurrIdx)
        {

            Update.UpgradeStatus = status;
            Update.CurrIdx = CurrIdx;
        }
        async void Upgrade()
        {

            try
            {
                int loop = 90;
                using (FileStream fs = new FileStream(Update.FilePath, FileMode.Open, FileAccess.Read))
                {
                    UpdateLoading(Msg:"检查状态中...");
                    await Task.Delay(200);
                    bitTimer.Stop();
                    Update.BootMode = BootMode.None;                  
                    loop = 20;
                    while (Update.BootMode == BootMode.None && loop > 0)
                    {
                        DevQueryParam(PLDParams.PLDParamsToQuery.BootMode);
                        loop--;
                        await Task.Delay(50);
                    }
                    UpdateLoading(false);
                    if (Update.BootMode == BootMode.None)
                    {
                       
                        await Application.Current.Dispatcher.Invoke(() =>
                               dialogHostService.Question("升级失败", "\r\n设备未响应，请检查连接后重试！", msgType: MsgType.Yes)
                          );
                        bitTimer.Start();
                        return;
                    }
                 
                    if (Update.BootMode != BootMode.Boot)
                    {
                        
                        var dialogResult = await dialogHostService.Question("程序升级", "\r\n程序升级需要停止运行，是否继续?\r\n", msgType: MsgType.YesNo);
                        if (dialogResult.Result != Prism.Dialogs.ButtonResult.Yes)
                        {
                            bitTimer.Start();
                            return;
                        }
                        
                        //connectTimer.Stop();
                        UpdateLoading(Msg: "停止设备中...");
                        await Task.Delay(100);
                        for (int i = 0; i < 4; i++)
                        {
                            setParam.TECParams[i].WorkType = PLDParams.WorkType.OFF;
                            SettingParamsToShow.TECParams[i].IsWork = false;
                        }
                        DevSetParam(PLDParams.PLDParamsToSet.TEC1_SW);
                        DevSetParam(PLDParams.PLDParamsToSet.TEC2_SW);
                        DevSetParam(PLDParams.PLDParamsToSet.TEC3_SW);
                        DevSetParam(PLDParams.PLDParamsToSet.TEC4_SW);

                        for (int i = 0; i < 2; i++)
                        {
                            setParam.LDParams[i].WorkType = PLDParams.WorkType.OFF;
                            SettingParamsToShow.LDParams[i].IsWork = false;
                        }
                        DevSetParam(PLDParams.PLDParamsToSet.LD1_SW);
                        DevSetParam(PLDParams.PLDParamsToSet.LD2_SW);
                        await Task.Delay(500);
                        //UpdateLoading(false);
                        UpdateLoading(Msg: "进入BOOT模式中...");
                        setParam.BootMode = BootMode.Boot;
                        loop = 50;
                        while (Update.BootMode != BootMode.Boot && loop > 0)
                        {
                            DevSetParam(PLDParams.PLDParamsToSet.BootMode);
                            loop--;
                            await Task.Delay(60);
                        }
                        if (Update.BootMode != BootMode.Boot)
                        {
                            UpdateLoading(false);                          
                            await Application.Current.Dispatcher.Invoke(() =>
                                   dialogHostService.Question("升级失败", "\r\n进入BOOT模式失败！", msgType: MsgType.Yes)
                              );
                            Application.Current.Dispatcher.Invoke(() => aggregator.SendMessage("进入BOOT模式失败!", "Main", Type: MessageModel.MessageType.Error, TimeSpan: 4.0));
                            //connectTimer.Start();
                            bitTimer.Start();
                            return;
                        }
                        
                    }


                    fs.Seek(0, SeekOrigin.Begin);
                    byte[] dataArr = new byte[fs.Length];
                    fs.Read(dataArr, 0, dataArr.Length);

                    //UInt32 tts = CRC32_Uint32(dataArr, 12);
                    //return;
                    //List<byte> dataList = new List<byte>(dataArr);
                    //dataList.InsertRange(0, BitConverter.GetBytes(CRC32_Uint32(dataArr)));
                    //dataList.InsertRange(0, BitConverter.GetBytes((UInt32)dataArr.Length));
                    //byte[] dataArr = new byte[] { 00, 0x1b, 0xff, 0x00, 0xdd, 0x1b, 0x00/*, 0xff*/ };
                    //List<byte> dataList = new List<byte>(dataArr);
                    //dataList.InsertRange(0, BitConverter.GetBytes(CRC32_Uint32(dataArr)));
                    //dataList.InsertRange(0, BitConverter.GetBytes((UInt32)dataArr.Length));

                    Update.IsUpdate = true;
                    int UnitPacketLen = 32;//最小内存为32字节
                    int maxPacketLen = UnitPacketLen * 7;
                    UInt16 TotalPacketNum = (UInt16)Math.Ceiling(dataArr.Length / (double)maxPacketLen);
                    int pos = 0;
                    setParam.UpgradeParams.TotalPaketNum = TotalPacketNum;
                    setParam.UpgradeParams.CurrIdx = 1;
                    Update.CurrIdx = 0;
                    while (dataArr.Length - pos > 0)
                    {
                        Update.ButtonContent = $"升级中({setParam.UpgradeParams.CurrIdx}/{TotalPacketNum})...";
                        UpdateLoading(Msg: $"升级中({setParam.UpgradeParams.CurrIdx}/{TotalPacketNum})...");
                        setParam.UpgradeParams.Data.Clear();
                        int curPacketLen = dataArr.Length - pos > maxPacketLen ? maxPacketLen : dataArr.Length - pos;
                        setParam.UpgradeParams.Data.AddRange(dataArr.Skip(pos).Take(curPacketLen));
                        if (curPacketLen % UnitPacketLen != 0)
                        {
                            int addLen = UnitPacketLen - (curPacketLen % UnitPacketLen);
                            for (int i = 0; i < addLen; i++)
                            {
                                setParam.UpgradeParams.Data.Add(0);
                            }
                        }
                        if (setParam.UpgradeParams.CurrIdx ==1)
                        {
                            setParam.UpgradeParams.Data.InsertRange(0, BitConverter.GetBytes(CRC32_Uint32(dataArr)));
                            setParam.UpgradeParams.Data.InsertRange(0, BitConverter.GetBytes((UInt32)dataArr.Length));
                        }
                       
                        DevSetParam(PLDParamsToSet.Upgrade);
                        Update.UpgradeStatus = PLDParams.UpgradeStatus.None;
                        //Update.IsAccess = false;
                        loop = 100;
                        while ((Update.UpgradeStatus == PLDParams.UpgradeStatus.None || Update.UpgradeStatus == PLDParams.UpgradeStatus.RUNNING) &&
                                 loop > 0)
                        {
                            DevQueryParam(PLDParamsToQuery.Upgrade);
                            loop--;
                            await Task.Delay(30);
                        }
                        if (Update.UpgradeStatus != PLDParams.UpgradeStatus.CUR_DONE && Update.UpgradeStatus != PLDParams.UpgradeStatus.LAST_DONE)
                        {
                            goto Failed;
                        }
                        setParam.UpgradeParams.CurrIdx = (UInt16)(Update.CurrIdx + 1);          
                        //setParam.UpgradeParams.CurrIdx++;        
                        pos += curPacketLen;
                    }
                }
                
                await Task.Delay(200);
                loop = 80;
                Update.IsAccess = false;
                Update.UpgradeStatus = PLDParams.UpgradeStatus.None;
                while ((Update.UpgradeStatus == PLDParams.UpgradeStatus.None || Update.UpgradeStatus == PLDParams.UpgradeStatus.RUNNING) &&
                                 loop > 0)
                {
                    DevQueryParam(PLDParamsToQuery.Upgrade);
                    loop--;
                    await Task.Delay(30);
                }
                if (Update.UpgradeStatus != PLDParams.UpgradeStatus.CUR_DONE && (Update.UpgradeStatus != PLDParams.UpgradeStatus.LAST_DONE))
                {
                    goto Failed;
                }

                Application.Current.Dispatcher.Invoke(() => aggregator.SendMessage("程序烧写完成!", "Main", Type: MessageModel.MessageType.Success, TimeSpan: 4.0));

                Update.IsUpdate = false;
                Update.ButtonContent = "升级";


                UpdateLoading(Msg: "烧写完成，应用重启检验中...");
                await Task.Delay(100);
                setParam.BootMode = BootMode.App;
                Update.BootMode = BootMode.None;
                loop = 50;
                while (Update.BootMode != BootMode.App && loop > 0)
                {
                    DevSetParam(PLDParams.PLDParamsToSet.BootMode);
                    loop--;
                    await Task.Delay(60);
                }
                if (Update.BootMode != BootMode.App)
                {
                    UpdateLoading(false);
                    bitTimer.Start();
                    await Application.Current.Dispatcher.Invoke(() =>
                                   dialogHostService.Question("程序升级结果", "\r\n程序升级失败，请重试！", msgType: MsgType.Yes)
                              );
                    bitTimer.Start();
                    return;
                }      
                UpdateLoading(false);

                await Application.Current.Dispatcher.Invoke(() =>
                                      dialogHostService.Question("程序升级结果", "\r\n程序升级成功!", msgType: MsgType.Yes)
                                 );
                bitTimer.Start();
                return;

            Failed:
                Update.IsUpdate = false;
                Update.ButtonContent = "升级";
                bitTimer.Start();
                UpdateLoading(false);
                if (loop == 0)
                {
                    //Application.Current.Dispatcher.Invoke(() => aggregator.SendMessage($"程序升级失败,第{setParam.UpgradeParams.CurrIdx + 1}包升级超时!", "Main", Type: MessageModel.MessageType.Error, TimeSpan: 4.0));
                    await Application.Current.Dispatcher.Invoke(() =>
                           dialogHostService.Question("升级失败", $"程序升级失败,第{setParam.UpgradeParams.CurrIdx + 1}包升级超时!", msgType: MsgType.Yes)
                      );
                }
                else
                    switch (Update.UpgradeStatus)
                    {
                        case PLDParams.UpgradeStatus.IDLE:
                            break;
                        case PLDParams.UpgradeStatus.RUNNING:
                            break;
                        case PLDParams.UpgradeStatus.LAST_DONE:
                            break;
                        case PLDParams.UpgradeStatus.LAST_CHECK_ERR:
                        case PLDParams.UpgradeStatus.LAST_FAILED:
                            await Application.Current.Dispatcher.Invoke(() =>
                                               dialogHostService.Question("升级失败", "程序升级失败,CRC校验失败!!", msgType: MsgType.Yes)
                                          );
                            break;
                        case PLDParams.UpgradeStatus.CUR_DONE:
                            break;
                        case PLDParams.UpgradeStatus.CUR_CHECK_ERR:
                        case PLDParams.UpgradeStatus.CUR_FALIED:
                            await Application.Current.Dispatcher.Invoke(() =>
                                               dialogHostService.Question("升级失败", $"程序升级失败,第{setParam.UpgradeParams.CurrIdx}包校验错误!", msgType: MsgType.Yes)
                                          );
                            break;
                        case PLDParams.UpgradeStatus.None:
                            break;
                        default:
                            break;
                    }

            }
            catch (Exception ex)
            {

                aggregator.SendMessage(ex.Message, "Main");
            }
            bitTimer.Start();
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
                case "设置TEC1限压": DevSetTECxMaxVol(0); break;
                case "设置TEC2限压": DevSetTECxMaxVol(1); break;
                case "设置TEC3限压": DevSetTECxMaxVol(2); break;
                case "设置TEC4限压": DevSetTECxMaxVol(3); break;
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
                case "设置LD1SKB": DevSetKBParam(PLDParams.PLDParamsToSet.LD1_SKB, PLDParams.PLDParamsToQuery.LD1_SKB, SettingParams.LDParams[0].CalibSet, setParam.LDParams[0].CalibSet); break;
                case "设置LD2SKB": DevSetKBParam(PLDParams.PLDParamsToSet.LD2_SKB, PLDParams.PLDParamsToQuery.LD2_SKB, SettingParams.LDParams[1].CalibSet, setParam.LDParams[1].CalibSet); break;
                case "设置LD1MKB": DevSetKBParam(PLDParams.PLDParamsToSet.LD1_MKB, PLDParams.PLDParamsToQuery.LD1_MKB, SettingParams.LDParams[0].CalibMeasure, setParam.LDParams[0].CalibMeasure); break;
                case "设置LD2MKB": DevSetKBParam(PLDParams.PLDParamsToSet.LD2_MKB, PLDParams.PLDParamsToQuery.LD2_MKB, SettingParams.LDParams[1].CalibMeasure, setParam.LDParams[1].CalibMeasure); break;
                case "设置PDKB": DevSetKBParam(PLDParams.PLDParamsToSet.PD_MKB, PLDParams.PLDParamsToQuery.PD_MKB, SettingParams.CalibPD,setParam.CalibPD); break;
                case "设置TEC1PID": DevSetTECxPID(0); break;
                case "设置TEC2PID": DevSetTECxPID(1); break;
                case "设置TEC3PID": DevSetTECxPID(2); break;
                case "设置TEC4PID": DevSetTECxPID(3); break;

                case "参数更新": DevParamUpdata(); break;
                case "参数固化": DevSetParam(PLDParams.PLDParamsToSet.SaveParam); break;

                case "设置目录": FolderPiker(); break;
                case "打开目录": GoToSurce(obj); break;
                case "选择升级文件": FilePicker(); break;
                case "打开升级文件目录": GoToSurce(obj); break;
                case "清空错误": DevClearErr(); break;
                case "升级": Upgrade(); break;


                default: break;
            }
        }



    }
}
