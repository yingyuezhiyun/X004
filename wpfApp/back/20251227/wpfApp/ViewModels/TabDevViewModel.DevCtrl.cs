using Prism.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using wpfApp.Common.CtrlProtocol;
using wpfApp.Common.Dialog;
using wpfApp.Common.Events;
using wpfApp.Extensions;
using static ScottPlot.Generate;

namespace wpfApp.ViewModels
{
    public partial class TabDevViewModel
    {

  
        public class type_map_t
        {
            public string Name;
            public byte Mode;
        }
        type_map_t[] work_modes =
        {
            new type_map_t() { Name= "恒电流",Mode =0 },
            new type_map_t() { Name= "恒功率",Mode =1 },
            new type_map_t() { Name= "外调制",Mode =2 },
        };
        string trans_work_mode(byte mode)
        {
            foreach (var item in work_modes)
            {
                if (item.Mode == mode)
                {
                    return item.Name;
                }
            }
            return "其他";

        }

        byte trans_work_mode(string Name)
        {
            foreach (var item in work_modes)
            {
                if (item.Name == Name)
                {
                    return item.Mode;
                }
            }
            return 0;
        }
        string trans_ld_running_status(byte mode)
        {

            switch (mode)
            {
                case 0: return "待机";
                case 1: return "运行中";
                case 2: return "过流";
                case 3: return "欠流";
                default: return "待机";
            }
        }

        string trans_tec_running_status(byte mode)
        {
            switch (mode)
            {
                case 0: return "待机";
                case 1: return "运行中";
                case 2: return "过温";
                case 3: return "欠温";
                default: return "待机";
            }
        }


        void para_save_reslt(byte status)
        {
            //TODO 状态提示
            var resl = (status == 1) ? "固化成功!" : "固化失败！";

            Application.Current.Dispatcher.Invoke(() => aggregator.SendMessage(resl, "Main"));
           
        }
        List<DevMeasureData> devMeasureDatas = new List<DevMeasureData>();
        void m_para(byte[] data, System.DateTime dateTime)
        {
            DevMeasureData data1 = new DevMeasureData();
            data1.time = dateTime;
            data1.temperature = BitConverter.ToSingle(data, 0);
            data1.power = BitConverter.ToSingle(data, 4);
            data1.LD_curr = BitConverter.ToSingle(data, 8);
            data1.PIN_curr = BitConverter.ToSingle(data, 12);
            data1.LD_vol = BitConverter.ToSingle(data, 16);
            data1.TEC_curr = BitConverter.ToSingle(data, 20);
            data1.NTC_vaule = BitConverter.ToSingle(data, 24);
            devMeasureDatas.Add(data1);
           

            DevParaShow.M.LDTemp.Vaule = data1.temperature.ToString("F4");
            DevParaShow.M.LDPower.Vaule = data1.power.ToString("F4");
            DevParaShow.M.LDCurr.Vaule = data1.LD_curr.ToString("F4");
            DevParaShow.M.PINCurr.Vaule = (data1.PIN_curr.ToString("F4"));
            DevParaShow.M.LDVol.Vaule = (data1.LD_vol.ToString("F4"));
            DevParaShow.M.TECCurr.Vaule = (data1.TEC_curr.ToString("F1"));
            DevParaShow.M.NTCVaule.Vaule = data1.NTC_vaule.ToString("F1");

            sig_invert(sig1, data1);
            sig_invert(sig2, data1);
            ChartRender();
            DevUpdataPublish(data1);

            if (IsFileSave)
            {
                saveDataFile(data1);
            }
            
        }
        void UpadtaDevMeasureParams(object data)
        {
            var measuerPairs = data as Dictionary<DevParamsFromGet, object>;
            DevMeasureData data1 = new DevMeasureData();

            foreach (var item in measuerPairs)
            {
                switch (item.Key)
                {
                    case DevParamsFromGet.M_DataTime:
                        data1.time = (System.DateTime)item.Value;
                        break;
                    case DevParamsFromGet.M_LDTemp:
                        data1.temperature= (float)item.Value;
                        break;
                    case DevParamsFromGet.M_LDPower:
                        data1.power = (float)item.Value;
                        break;
                    case DevParamsFromGet.M_LDCurr:
                        data1.LD_curr = (float)item.Value;
                        break;
                    case DevParamsFromGet.M_PINCurr:
                        data1.PIN_curr = (float)item.Value;
                        break;
                    case DevParamsFromGet.M_LDVol:
                        data1.LD_vol = (float)item.Value;
                        break;
                    case DevParamsFromGet.M_TECCurr:
                        data1.TEC_curr = (float)item.Value;
                        break;
                    case DevParamsFromGet.M_NTC:
                        data1.NTC_vaule = (float)item.Value;
                        break;
                    case DevParamsFromGet.BoardTemp:
                        data1.Board_temp = (float)item.Value;
                        break;
                    case DevParamsFromGet.AHT20Temp:
                        data1.AHT20_temp = (float)item.Value;
                        break;
                    case DevParamsFromGet.AHT20Humidity: 
                        data1.AHT20_humidity = (float)item.Value;
                        break;
                    default:
                        break;
                }
            }

            
            devMeasureDatas.Add(data1);

            DevParaShow.M.LDTemp.Vaule = data1.temperature.ToString("F4");
            DevParaShow.M.LDPower.Vaule = data1.power.ToString("F4");
            DevParaShow.M.LDCurr.Vaule = data1.LD_curr.ToString("F4");
            DevParaShow.M.PINCurr.Vaule = (data1.PIN_curr.ToString("F4"));
            DevParaShow.M.LDVol.Vaule = (data1.LD_vol.ToString("F4"));
            DevParaShow.M.TECCurr.Vaule = (data1.TEC_curr.ToString("F1"));
            DevParaShow.M.NTCVaule.Vaule = data1.NTC_vaule.ToString("F1");
            DevParaShow.M.BoardTemp.Vaule = data1.NTC_vaule.ToString("F2");
            DevParaShow.M.AHT20Temp.Vaule = data1.AHT20_temp.ToString("F2");
            DevParaShow.M.AHT20Humidity.Vaule = data1.AHT20_humidity.ToString("F2");
            sig_invert(sig1, data1);
            sig_invert(sig2, data1);
            ChartRender();
            DevUpdataPublish(data1);

            if (IsFileSave)
            {
                saveDataFile(data1);
            }

        }

        void DevUpdataPublish(DevMeasureData data)
        {
            var param = new DevUpdateModel();
            param.DevName = NameSpace;
            param.CH = CH;
            param.Type = DevUpdateType.MeasureData;
            param.devMeasureData = data;            
            aggregator.GetEvent<DevUpdateEvent>().Publish(param);
        }

        [Flags]
        public enum StatusFlags : UInt32
        {
            None = 0,
            i2c_err = 1 << 0,   // i2c错误
            tec_err = 1 << 1,   // tec报错
            ld_err = 1 << 2,   // LD报错
            high_temperature = 1 << 3,   // 高温
            low_temperature = 1 << 4,   // 低温
            over_current = 1 << 5,   // 过流
            under_current = 1 << 6,   // 欠流
            exit_over_current = 1 << 7,   // 硬件过流
            ld_run_sta = 1 << 16,
            tec_run_sta = 1 << 17,
        }

        public struct bit_sta_t
        {
            public StatusFlags statusFlags;
            public string Name;
        }
        bit_sta_t[] bit_stas =
        {
            new bit_sta_t() { Name= "存储错误 ",statusFlags=StatusFlags.i2c_err },
            new bit_sta_t() { Name= "LD错误 ",statusFlags=StatusFlags.ld_err },
            new bit_sta_t() { Name= "TEC错误 ",statusFlags=StatusFlags.tec_err },
            new bit_sta_t() { Name= "高温 ",statusFlags=StatusFlags.high_temperature },
            new bit_sta_t() { Name= "低温 ",statusFlags=StatusFlags.low_temperature },
            new bit_sta_t() { Name= "欠流 ",statusFlags=StatusFlags.under_current },
            new bit_sta_t() { Name= "过流 ",statusFlags=StatusFlags.exit_over_current },
        };

        void DevClose()
        {
            try
            {
                bitTimer.Stop();
                queryTimer.Stop();
                connectTimer.Stop();
                LDIsWork = false;
                TECIsWork = false;                
                DeviceSerialPort.Close();
                DeviceIsOpen = DeviceSerialPort.IsOpen;
                IsUpdatePara = false;   
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => aggregator.SendMessage(ex.Message, "Main"));
                
            }
        }

        

        bool isIgnoreBitErr = false;

        async void UpdataBITStatus(UInt32 bit)
        {
            bool tec_run_st = (bit & (UInt32)StatusFlags.tec_run_sta) != 0;
            //通过返回状态 判断TEC是否处于工作
            if (tec_run_st && TECIsWork == false)
            {
                TECIsWork = true;
                queryTimer.Start();
            }
            else if (!tec_run_st && TECIsWork == true)
            {
                TECIsWork = false;
                queryTimer.Stop();
            }
            bool ld_run_st = (bit & (UInt32)StatusFlags.ld_run_sta) != 0;
            ///通过返回状态 判断LD是否处于工作
            if (ld_run_st && LDIsWork == false)
            {
                LDIsWork = true;
            }
            else if (!ld_run_st && LDIsWork == true)
            {
                LDIsWork = false;
            }
            string status = "";

            foreach (var item in bit_stas)
            {
                if ((bit & (UInt32)item.statusFlags) != 0)
                {
                    status += item.Name;
                }
            }
            if (status == "")
            {
                status = "正常";
            }
            //TODO 状态提示
            else if (!isIgnoreBitErr)
            {
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    var dialogResult = await dialogHostService.Question("运行错误", NameSpace + "\r\n设备状态异常，已经停止运行!\r\n" + "错误内容：" + status, msgType: MsgType.YesIgnoreRetry);
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

            DevParaShow.StatusInfo.BITStatus.Vaule = status;

            DevUpdataPublish(DevParamsFromGet.BIT, DevParaShow.StatusInfo.BITStatus.Vaule);
        }

     

        void DevUpdataPublish(DevParamsFromGet Type, string Value)
        {
            var param = new DevUpdateModel();
            param.DevName = NameSpace;
            param.CH = CH;
            param.Type = DevUpdateType.Status;
            param.devStatusData.ParaType = Type;
            param.devStatusData.ParaValue = Value;
            aggregator.GetEvent<DevUpdateEvent>().Publish(param);
        }

        void dataHandler()
        {
            while (true)
            {
                Thread.Sleep(100);
                List<RevData> revData_temp = new List<RevData>();
                lock (locker)
                {
                    revData_temp.AddRange(revDatas);
                    revDatas.Clear();
                }

                foreach (var data in revData_temp)
                {

                    var d = protocolManager.Parse(data);
                    switch (d.ParamsGet)
                    {
                        case DevParamsFromGet.Mode:
                            DevParaShow.StatusInfo.Mode.Vaule = (string)d.result;
                            DevUpdataPublish(DevParamsFromGet.Mode, DevParaShow.StatusInfo.Mode.Vaule);
                            break;
                        case DevParamsFromGet.LDTemp:
                            DevParaShow.S.LDTemp.Vaule = ((float)d.result).ToString("F3");
                            DevUpdataPublish(DevParamsFromGet.LDTemp, DevParaShow.S.LDTemp.Vaule);
                            break;
                        case DevParamsFromGet.LDCurr:
                            DevParaShow.S.LDCurr.Vaule = ((float)d.result).ToString("F2");
                            DevUpdataPublish(DevParamsFromGet.LDCurr, DevParaShow.S.LDCurr.Vaule);
                            break;
                        case DevParamsFromGet.LDPower:
                            DevParaShow.S.LDPower.Vaule = ((float)d.result).ToString("F2");
                            DevUpdataPublish(DevParamsFromGet.LDPower, DevParaShow.S.LDPower.Vaule);
                            break;
                        case DevParamsFromGet.LD_SW:
                            DevParaShow.StatusInfo.LDStatus.Vaule = (string)d.result;
                            DevUpdataPublish(DevParamsFromGet.LD_SW, DevParaShow.StatusInfo.LDStatus.Vaule);
                            break;
                        case DevParamsFromGet.TEC_SW:
                            DevParaShow.StatusInfo.TECStatus.Vaule = (string)d.result;
                            DevUpdataPublish(DevParamsFromGet.TEC_SW, DevParaShow.StatusInfo.TECStatus.Vaule);
                            break;
                        case DevParamsFromGet.LDType:
                            break;
                        case DevParamsFromGet.LDOC:
                            break;
                        case DevParamsFromGet.LDHOC:
                            DevParaShow.S.LDHOC.Vaule = ((float)d.result).ToString("F0");
                            break;
                        case DevParamsFromGet.LDMeasureR:
                            DevParaShow.S.LDMeasureR.Vaule=((float)d.result).ToString("F4");
                            break;
                        case DevParamsFromGet.LDMaxCurr:
                            DevParaShow.S.LDMaxCurr.Vaule = ((float)d.result).ToString("F1"); 
                            break;
                        case DevParamsFromGet.TECMaxVol:
                            DevParaShow.S.TECMaxVol.Vaule = ((float)d.result).ToString("F0");
                            break;
                        case DevParamsFromGet.TECMinTemp:
                            DevParaShow.S.TECMinTemp.Vaule = ((float)d.result).ToString("F3");
                            break;
                        case DevParamsFromGet.TECMaxTemp:
                            DevParaShow.S.TECMaxTemp.Vaule = ((float)d.result).ToString("F3");
                            break;
                        case DevParamsFromGet.TECTempOffset:
                            DevParaShow.S.TECTempOffset.Vaule = ((float)d.result).ToString("F3");
                            break;
                        case DevParamsFromGet.PIN_PW_k:
                            break;
                        case DevParamsFromGet.PIN_PW_b:
                            break;
                        case DevParamsFromGet.PW_CUR_k:
                            break;
                        case DevParamsFromGet.PW_CUR_b:
                            break;
                        case DevParamsFromGet.TEC_k:
                            break;
                        case DevParamsFromGet.TEC_b:
                            break;
                        case DevParamsFromGet.CUR_S_k:
                            break;
                        case DevParamsFromGet.CUR_S_b:
                            break;
                        case DevParamsFromGet.CUR_M_k:
                            break;
                        case DevParamsFromGet.CUR_M_b:
                            break;
                        case DevParamsFromGet.LASER_p:
                            break;
                        case DevParamsFromGet.LASER_i:
                            break;
                        case DevParamsFromGet.LASER_d:
                            break;
                        case DevParamsFromGet.IsAutoRun:
                            DevParaShow.S.IsAutoRun.Vaule = (string)d.result;
                            break;
                        case DevParamsFromGet.M_LDTemp:
                            DevParaShow.M.LDTemp.Vaule = ((float)d.result).ToString("F4");
                            break;
                        case DevParamsFromGet.M_LDPower:
                            DevParaShow.M.LDPower.Vaule = ((float)d.result).ToString("F4");
                            break;
                        case DevParamsFromGet.M_LDCurr:
                            DevParaShow.M.LDCurr.Vaule = ((float)d.result).ToString("F4");
                            break;
                        case DevParamsFromGet.M_PINCurr:
                            DevParaShow.M.PINCurr.Vaule = ((float)d.result).ToString("F4");
                            break;
                        case DevParamsFromGet.M_LDVol:
                            DevParaShow.M.LDVol.Vaule = ((float)d.result).ToString("F4");
                            break;
                        case DevParamsFromGet.M_TECCurr:
                            DevParaShow.M.TECCurr.Vaule = ((float)d.result).ToString("F1");
                            break;
                        case DevParamsFromGet.M_NTC:
                            DevParaShow.M.NTCVaule.Vaule = ((float)d.result).ToString("F1");
                            break;
                        case DevParamsFromGet.M_ALL:
                            UpadtaDevMeasureParams(d.result);
                            break;
                        case DevParamsFromGet.Save_Para:
                            Application.Current.Dispatcher.Invoke(() => aggregator.SendMessage((string)d.result, "Main"));
                            break;
                        case DevParamsFromGet.REST:
                            break;
                        case DevParamsFromGet.ClearErr:
                            break;
                        case DevParamsFromGet.None:
                            break;
                        case DevParamsFromGet.BIT:
                            UpdataBITStatus((UInt32)d.result);
                            break;
                        default:
                            break;
                    }

                }

            }
        }


        void DevClearErr()
        {
            DevSetParam(DevParamsToSet.ClearErr);
        }
   

        void DevSetMode()
        {
            DevSetParam(DevParamsToSet.Mode, trans_work_mode(DevParaSetting.Mode));
        }
        void DevSetAutoRun()
        {
            byte state = (byte)(DevParaSetting.IsAutoRun == "是" ? 1 : 0);
            DevSetParam(DevParamsToSet.IsAutoRun, state);
        }
        void DevSetLDLaunch()
        {
            byte state = (byte)(LDIsWork == true ? 0 : 1);
            DevSetParam(DevParamsToSet.LD_SW, state);
        }
        void DevSetTECLaunch()
        {
            byte state = (byte)(TECIsWork == true ? 0 : 1);
            DevSetParam(DevParamsToSet.TEC_SW, state);
        }

      

        async void DevParamUpdata()
        {
            if (!IsUpdatePara)
            {
                IsUpdatePara = true;
                foreach (var item in Enum.GetValues(typeof(DevParamsFromGet)))
                {
                    if ((DevParamsFromGet)item == DevParamsFromGet.IsAutoRun)
                    {
                        DevGetParam(((DevParamsFromGet)item));
                        IsUpdatePara = false;
                        return;
                    }
                    DevGetParam(((DevParamsFromGet)item));
                    await Task.Delay(200);
                }
            }
        }

    }

    public class DevStatusData
    {
        public System.DateTime time { get; set; }

        public DevParamsFromGet ParaType { get; set; }

        public string ParaValue { get; set; }

    }

    public class DevMeasureData
    {
        public System.DateTime time { get; set; }

        public double temperature { get; set; }

        public double power { get; set; }

        public double LD_curr { get; set; }

        public double PIN_curr { get; set; }

        public double LD_vol { get; set; }

        public double NTC_vaule { get; set; }

        public double TEC_curr { get; set; }

        public double Board_temp { get; set; }
        public double AHT20_temp { get; set; }
        public double AHT20_humidity { get; set; }

        public List<double> data_list
        {
            get
            {
                return new List<double>() {
                    this.temperature,   this.power,         this.LD_curr,
                    this.PIN_curr,      this.LD_vol,        this.NTC_vaule,
                    this.TEC_curr ,     this.Board_temp,    this.AHT20_humidity,
                    this.AHT20_temp,
                };
            }
        }
        //  public static List<DevMeasureData> content { get; set; } = new List<DevMeasureData>();

    }


 

   

}
