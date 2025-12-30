using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Windows.Networking;
using wpfApp.Common.CtrlProtocol;
using wpfApp.Common.Dialog;
using wpfApp.Common.Events;
using wpfApp.Extensions;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;


namespace wpfApp.ViewModels
{
    public class TabStatusViewModel : NavigationViewModel
    {
        
        private readonly IDialogHostService dialogHostService;
        public ObservableCollection<DevParaShow> DevParaShow { get; private set; }
        int DevCNT = 8;
        public DelegateCommand<string> ExecuteCommand { get; private set; }
        public TabStatusViewModel(IRegionManager regionManager, IContainerExtension container, IDialogHostService dialogHostService) : base(container)
        {
           
            this.dialogHostService = dialogHostService;
            CreateDevStatus();

            //DevParaShow[0].Visibility = Visibility.Visible;

            aggregator.GetEvent<DevUpdateEvent>().Subscribe(x =>
            {
                DevUpdata(x);
            });

            ExecuteCommand = new DelegateCommand<string>(Execute);

             //int ch=  GetDevChFormName("WCH USB-SERIAL Ch A (COM25)");
        }
        public TabStatusViewModel()
        {

        }

        
        

        void CreateDevStatus()
        {
            DevParaShow = new ObservableCollection<DevParaShow>();
            for (int i = 0; i < DevCNT; i++)
            {
                char c = (char)('A' + i);
                DevParaShow.Add(new DevParaShow() { Title = "通道" + c + " 参数信息" });
            }
            for (int i = 0; i < DevCNT; i++)
            {
                char c = (char)('A' + i);
                string temp;
                temp = OperateIniFile.ReadIniData("Channel" + c, "Title");
                if (temp != null && temp != string.Empty)
                {
                    DevParaShow[i].Title = temp + " 参数信息";
                }
            }

        }

   
        bool IsFisrtLoad = true;

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (IsFisrtLoad)
            {
                SearchDev();
                IsFisrtLoad = false;
            }
        }
        ~TabStatusViewModel()
        {

        }
        class WCHDev
        {
            public bool valid = false;
            public string Port;
            public string Name;
            public DevType DevType;
        }

        string ExtractComPort(string device)
        {
            // 假设 COM 端口在括号内
            int startIndex = device.IndexOf('(') + 1;
            int endIndex = device.IndexOf(')');
            //int endIndex = device.IndexOf("->");
            if (startIndex>=0&& endIndex>=0&& endIndex - startIndex>0)
            {
                return device.Substring(startIndex, endIndex - startIndex).Trim();
            }
            return null;
        }
        List<WCHDev> GetWCHPort()
        {

            var ports = new List<WCHDev>();
            //string query2 = "select * from Win32_PnPEntity WHERE Name LIKE '%PORT%' AND Name LIKE '%COM%'";
            string query2 = "select * from Win32_PnPEntity WHERE Name LIKE '%WCH%' AND Name LIKE '%COM%'";
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query2))
            {
                var hardInfos = searcher.Get();
                foreach (var hardInfo in hardInfos)
                {

                    var dev = new WCHDev();
                    dev.Name = hardInfo["Name"].ToString();
                    dev.Port = ExtractComPort(dev.Name);
                    if (dev.Port!=null)
                    {
                        ports.Add(dev);
                    }
                }
            }
            return ports;
        }

        int GetDevChFormName(string device)
        {
            int startIndex = device.IndexOf("Ch ") + 1;
            int endIndex = device.IndexOf(" (");
            if (startIndex >=0 && endIndex >= 0 && endIndex - startIndex > 0)
            {
                string descri = device.Substring(startIndex, endIndex - startIndex).Trim();
                for (int i = 0; i < DevCNT; i++)
                {
                    char c = (char)('A' + i);
                    if (descri.Contains(c))
                    {
                        return i;
                    }
                }
                return -1;
            }
            return -1;
        }

        private Visibility _tipVisibility = Visibility.Visible;

        public Visibility TipVisibility
        {
            get { return _tipVisibility; }
            set { _tipVisibility = value; RaisePropertyChanged(); }
        }


        async void SearchDev()
        {
            UpdateLoading(true, "查找设备中...");

            //var param1 = new DevManageUpdataModel();
            //param1.Type = DevManageUpdataType.Updata;
            //param1.IsOpen = true;
            //param1.DevPort = "COM22";
            //param1.devType = DevType.X002_001;
            //param1.CH = 0;
            //if (param1.CH >= 0)
            //{
            //    DevParaShow[param1.CH].Visibility = param1.IsOpen ? Visibility.Visible : Visibility.Collapsed;
            //    aggregator.GetEvent<DevManageEvent>().Publish(param1);
            //}

            // var test  = await new ProtocolCheck().DevCheck("COM22");

            List<WCHDev> wchDevs = new List<WCHDev>();
            await Task.Run(() =>
            {
                wchDevs = GetWCHPort(); 
            });
            var tasks = new List<Task<DevType>>();

            //foreach (var item in wchDevs)
            //{
            //    tasks.Add(DevCheck2(item.Port));
            //}
            foreach (var item in wchDevs)
            {
                tasks.Add(new ProtocolCheck().DevCheck(item.Port));
            }
            var results = await Task.WhenAll(tasks);
            UpdateLoading(false);
            int DevFindNum = 0;
            for (int i = 0; i < wchDevs.Count(); i++)
            {
                wchDevs[i].DevType = results[i];
                if (results[i] != DevType.None)
                {
                    wchDevs[i].valid = true;
                    DevFindNum++;
                }
            }
#if false
            var dialogResult = await dialogHostService.Question("检测到有效设备", $"检测到{DevFindNum}个可用设备。\r\n\r\n是否立即连接?");
            if (dialogResult.Result == Prism.Dialogs.ButtonResult.Yes)
            {
                UpdateLoading(true, "添加设备中...");
                await Task.Delay(500);
                var param = new DevManageUpdataModel();
                param.Type = DevManageUpdataType.Updata;
                param.IsOpen = true;
                param.DevPort = "COM2";
                param.CH = 1;
                if (param.CH >= 0)
                {
                    DevParaShow[param.CH].Visibility = param.IsOpen ? Visibility.Visible : Visibility.Collapsed;
                    aggregator.GetEvent<DevManageEvent>().Publish(param);
                }
                Visibility TipVisibilityTemp = Visibility.Visible;
                foreach (var item in DevParaShow)
                {
                    if (item.Visibility == Visibility.Visible)
                    {
                        TipVisibilityTemp = Visibility.Collapsed;
                        break;
                    }
                }
                TipVisibility = TipVisibilityTemp;
                await Task.Delay(500);
                UpdateLoading(false);
            }
#else
            if (DevFindNum == 0)
            {
                int ChNum = 0;
                foreach (var item in DevParaShow)
                {
                    if (item.Visibility == Visibility.Visible)
                    {
                        ChNum++;
                    }
                }
                if (ChNum > 0)
                {
                    dialogHostService.Question("温馨提示", $"已连接到多通道激光器系统({ChNum}合1)。\r\n\r\n暂未发现新通道接入。", msgType: MsgType.Yes);
                }
                else
                {
                    dialogHostService.Question("未检测到有效设备", "请确保设备已开机，且正确连接。\r\n\r\n请稍后重试。", msgType: MsgType.Yes);
                }

            }
            else
            {
                var dialogResult = await dialogHostService.Question("检测到有效设备", $"检测到多通道激光器系统({DevFindNum}合1)。\r\n\r\n是否立即连接?");
                if (dialogResult.Result == Prism.Dialogs.ButtonResult.Yes)
                {
                    UpdateLoading(true, "添加设备中...");
                    await Task.Delay(500);
                    foreach (var item in wchDevs)
                    {
                        var param = new DevManageUpdataModel();
                        param.Type = DevManageUpdataType.Updata;
                        param.IsOpen = item.valid;
                        param.DevPort = item.Port;
                        param.devType = item.DevType;
                        param.CH = GetDevChFormName(item.Name);
                        if (param.CH >= 0)
                        {
                            DevParaShow[param.CH].Visibility = param.IsOpen? Visibility.Visible : Visibility.Collapsed;
                            aggregator.GetEvent<DevManageEvent>().Publish(param);
                        }
                    }
                    Visibility TipVisibilityTemp = Visibility.Visible;
                    foreach (var item in DevParaShow)
                    {
                        if (item.Visibility == Visibility.Visible)
                        {
                            TipVisibilityTemp = Visibility.Collapsed;
                            break;
                        }
                    }
                    TipVisibility = TipVisibilityTemp;
                    await Task.Delay(500);
                    UpdateLoading(false);
                }
            }
#endif
        }

       
        private void Execute(string obj)
        {
            switch (obj)
            {
               
                case "自动搜索设备": SearchDev(); break;
                case "清空异常": ClearDevErr();  break;
                default: break;
            }
        }

        async void ClearDevErr()
        {
            UpdateLoading(true, "清空异常中...");
            for (int i = 0; i < DevCNT; i++)
            {
                var param = new DevManageUpdataModel();
                param.Type = DevManageUpdataType.ClearErr;
                param.CH = i;
                if (DevParaShow[param.CH].Visibility == Visibility.Visible)
                {
                    aggregator.GetEvent<DevManageEvent>().Publish(param);
                }
            }
            await Task.Delay(500);
            UpdateLoading(false);
        }
        void DevUpdata(DevUpdateModel devUpdateModel)
        {
            int ch = devUpdateModel.CH;
            if (devUpdateModel.Type == DevUpdateType.Status)
            {
                switch (devUpdateModel.devStatusData.ParaType)
                {
                    case DevParamsFromGet.Mode: DevParaShow[ch].StatusInfo.Mode.Vaule = devUpdateModel.devStatusData.ParaValue; break;
                    case DevParamsFromGet.LDTemp: DevParaShow[ch].S.LDTemp.Vaule = devUpdateModel.devStatusData.ParaValue; break;
                    case DevParamsFromGet.LDCurr: DevParaShow[ch].S.LDCurr.Vaule = devUpdateModel.devStatusData.ParaValue; break;
                    case DevParamsFromGet.LDPower: DevParaShow[ch].S.LDPower.Vaule = devUpdateModel.devStatusData.ParaValue; break;
                    case DevParamsFromGet.LD_SW: DevParaShow[ch].StatusInfo.LDStatus.Vaule = devUpdateModel.devStatusData.ParaValue; break;
                    case DevParamsFromGet.TEC_SW: DevParaShow[ch].StatusInfo.TECStatus.Vaule = devUpdateModel.devStatusData.ParaValue; break;
                    case DevParamsFromGet.BIT: DevParaShow[ch].StatusInfo.BITStatus.Vaule = devUpdateModel.devStatusData.ParaValue; break;
                    default: break;
                }
            }
            else
            {
                DevParaShow[ch].M.LDTemp.Vaule = devUpdateModel.devMeasureData.temperature.ToString("F4");
                DevParaShow[ch].M.LDPower.Vaule = devUpdateModel.devMeasureData.power.ToString("F4");
                DevParaShow[ch].M.LDCurr.Vaule = devUpdateModel.devMeasureData.LD_curr.ToString("F4");
                DevParaShow[ch].M.LDVol.Vaule = devUpdateModel.devMeasureData.LD_vol.ToString("F4");
                DevParaShow[ch].M.PINCurr.Vaule = devUpdateModel.devMeasureData.PIN_curr.ToString("F4");
                DevParaShow[ch].M.TECCurr.Vaule = devUpdateModel.devMeasureData.TEC_curr.ToString("F1");
                DevParaShow[ch].M.NTCVaule.Vaule = devUpdateModel.devMeasureData.NTC_vaule.ToString("F1");
            }

        }
    }

    public class DevParaShow : BindableBase
    {


        public class Measure : BindableBase
        {

            private DevPara _ldTemp = new DevPara() { Name = "LD温度(℃):", Vaule = "Nan" };
            /// <summary>
            /// LD温度
            /// </summary>
            public DevPara LDTemp
            {
                get { return _ldTemp; }
                set { _ldTemp = value; RaisePropertyChanged(); }
            }

            private DevPara _ldCurr = new DevPara() { Name = "LD电流(mA):", Vaule = "Nan" };
            /// <summary>
            /// LD电流
            /// </summary>
            public DevPara LDCurr
            {
                get { return _ldCurr; }
                set { _ldCurr = value; RaisePropertyChanged(); }
            }

            private DevPara _ldVol = new DevPara() { Name = "LD电压(mV):", Vaule = "Nan" };
            /// <summary>
            /// LD电压
            /// </summary>
            public DevPara LDVol
            {
                get { return _ldVol; }
                set { _ldVol = value; RaisePropertyChanged(); }
            }


            private DevPara _ldPower = new DevPara() { Name = "LD出光功率(mW):", Vaule = "Nan" };
            /// <summary>
            /// LD功率
            /// </summary>
            public DevPara LDPower
            {
                get { return _ldPower; }
                set { _ldPower = value; RaisePropertyChanged(); }
            }



            private DevPara _pinCurr = new DevPara() { Name = "PIN背光电流(uA):", Vaule = "Nan" };
            /// <summary>
            /// PIN电流
            /// </summary>
            public DevPara PINCurr
            {
                get { return _pinCurr; }
                set { _pinCurr = value; RaisePropertyChanged(); }
            }

            private DevPara _tecCurr = new DevPara() { Name = "TEC电流(mA):", Vaule = "Nan" };
            /// <summary>
            /// TEC电流
            /// </summary>
            public DevPara TECCurr
            {
                get { return _tecCurr; }
                set { _tecCurr = value; RaisePropertyChanged(); }
            }

            private DevPara _ntcVaule = new DevPara() { Name = "NTC阻值(Ω):", Vaule = "Nan" };
            /// <summary>
            /// 电阻值
            /// </summary>
            public DevPara NTCVaule
            {
                get { return _ntcVaule; }
                set { _ntcVaule = value; RaisePropertyChanged(); }
            }

            private DevPara _boardTemp = new DevPara() { Name = "R-温度(℃):", Vaule = "Nan" };
            /// <summary>
            /// 电阻值
            /// </summary>
            public DevPara BoardTemp
            {
                get { return _boardTemp; }
                set { _boardTemp = value; RaisePropertyChanged(); }
            }

            private DevPara _AHT20Temp = new DevPara() { Name = "AHT20温度(℃):", Vaule = "Nan" };
            /// <summary>
            /// 电阻值
            /// </summary>
            public DevPara AHT20Temp
            {
                get { return _AHT20Temp; }
                set { _AHT20Temp = value; RaisePropertyChanged(); }
            }

            private DevPara _AHT20Humidity = new DevPara() { Name = "AHT20湿度(%):", Vaule = "Nan" };
            /// <summary>
            /// 电阻值
            /// </summary>
            public DevPara AHT20Humidity
            {
                get { return _AHT20Humidity; }
                set { _AHT20Humidity = value; RaisePropertyChanged(); }
            }
        }


        public class Setting : BindableBase
        {

            private DevPara _ldTemp = new DevPara() { Name = "LD温度(℃):", Vaule = "Nan" };
            /// <summary>
            /// LD温度
            /// </summary>
            public DevPara LDTemp
            {
                get { return _ldTemp; }
                set { _ldTemp = value; RaisePropertyChanged(); }
            }

            private DevPara _ldCurr = new DevPara() { Name = "LD电流(mA):", Vaule = "Nan" };
            /// <summary>
            /// LD电流
            /// </summary>
            public DevPara LDCurr
            {
                get { return _ldCurr; }
                set { _ldCurr = value; RaisePropertyChanged(); }
            }

            private DevPara _ldVol = new DevPara() { Name = "LD电压(mV):", Vaule = "Nan" };
            /// <summary>
            /// LD电压
            /// </summary>
            public DevPara LDVol
            {
                get { return _ldVol; }
                set { _ldVol = value; RaisePropertyChanged(); }
            }

            private DevPara _ldPower = new DevPara() { Name = "LD出光功率(mW):", Vaule = "Nan" };
            /// <summary>
            /// LD功率
            /// </summary>
            public DevPara LDPower
            {
                get { return _ldPower; }
                set { _ldPower = value; RaisePropertyChanged(); }
            }


            private DevPara _ldMaxCurr = new DevPara() { Name = "LD最大电流(mA):", Vaule = "Nan" };
            /// <summary>
            /// LD最大电流
            /// </summary>
            public DevPara LDMaxCurr
            {
                get { return _ldMaxCurr; }
                set { _ldMaxCurr = value; RaisePropertyChanged(); }
            }

            private DevPara _ldHOC = new DevPara() { Name = "LD硬件过流保护(mA):", Vaule = "Nan" };
            /// <summary>
            /// LD硬件过流保护
            /// </summary>
            public DevPara LDHOC
            {
                get { return _ldHOC; }
                set { _ldHOC = value; RaisePropertyChanged(); }
            }

            private DevPara _LDMeasureR = new DevPara() { Name = "LD测量电阻(Ω):", Vaule = "Nan" };
            /// <summary>
            /// LD测量电阻
            /// </summary>
            public DevPara LDMeasureR
            {
                get { return _LDMeasureR; }
                set { _LDMeasureR = value; RaisePropertyChanged(); }
            }

            private DevPara _tecMinTemp = new DevPara() { Name = "TEC最低温度(℃):", Vaule = "Nan" };
            /// <summary>
            /// TEC最低温度
            /// </summary>
            public DevPara TECMinTemp
            {
                get { return _tecMinTemp; }
                set { _tecMinTemp = value; RaisePropertyChanged(); }
            }

            private DevPara _tecMaxTemp = new DevPara() { Name = "TEC最高温度(℃)", Vaule = "Nan" };
            /// <summary>
            /// TEC最高温度
            /// </summary>
            public DevPara TECMaxTemp
            {
                get { return _tecMaxTemp; }
                set { _tecMaxTemp = value; RaisePropertyChanged(); }
            }

            private DevPara _tecTempOffset = new DevPara() { Name = "TEC温度偏置(℃):", Vaule = "Nan" };
            /// <summary>
            /// TEC温度偏置
            /// </summary>
            public DevPara TECTempOffset
            {
                get { return _tecTempOffset; }
                set { _tecTempOffset = value; RaisePropertyChanged(); }
            }

            private DevPara _tecMaxVol = new DevPara() { Name = "TEC最大电压(mV):", Vaule = "Nan" };
            /// <summary>
            /// TEC最大电压
            /// </summary>
            public DevPara TECMaxVol
            {
                get { return _tecMaxVol; }
                set { _tecMaxVol = value; RaisePropertyChanged(); }
            }

            private DevPara _isAutoRun = new DevPara() { Name = "开机启动:", Vaule = "Nan" };
            /// <summary>
            /// TEC最大电压
            /// </summary>
            public DevPara IsAutoRun
            {
                get { return _isAutoRun; }
                set { _isAutoRun = value; RaisePropertyChanged(); }
            }
        }


        public class Status : BindableBase
        {

            private DevPara _mode = new DevPara() { Name = "工作模式:", Vaule = "Nan" };
            /// <summary>
            /// 工作模式
            /// </summary>
            public DevPara Mode
            {
                get { return _mode; }
                set { _mode = value; RaisePropertyChanged(); }
            }

            private DevPara _ldStatus = new DevPara() { Name = "LD状态:", Vaule = "Nan" };
            /// <summary>
            /// LD状态
            /// </summary>
            public DevPara LDStatus
            {
                get { return _ldStatus; }
                set { _ldStatus = value; RaisePropertyChanged(); }
            }


            private DevPara _tecStatus = new DevPara() { Name = "TEC状态:", Vaule = "Nan" };
            /// <summary>
            /// TEC状态
            /// </summary>
            public DevPara TECStatus
            {
                get { return _tecStatus; }
                set { _tecStatus = value; RaisePropertyChanged(); }
            }
            private DevPara _bitStatus = new DevPara() { Name = "自检状态:", Vaule = "Nan" };
            /// <summary>
            /// 自检状态
            /// </summary>
            public DevPara BITStatus
            {
                get { return _bitStatus; }
                set { _bitStatus = value; RaisePropertyChanged(); }
            }
        }


        


        public DevParaShow()
        {
            MeasurePara =  new List<DevPara>() { M.LDTemp, M.LDCurr, M.LDVol, M.LDPower, M.TECCurr };
            //MeasurePara = new List<DevPara>() { M.LDTemp, M.LDCurr, M.LDVol, M.LDPower, M.TECCurr, M.NTCVaule, M.PINCurr };
            SettingPara = new List<DevPara>() { S.LDTemp, S.LDCurr, S.LDPower };
            StatusPara = new List<DevPara>() { StatusInfo.Mode, StatusInfo.TECStatus, StatusInfo.LDStatus, StatusInfo.BITStatus, };

        }
        private string _title;

        public string Title
        {
            get { return _title; }
            set { _title = value; RaisePropertyChanged(); }
        }

        private Visibility _visibility = Visibility.Collapsed;

        public Visibility Visibility
        {
            get { return _visibility; }
            set { _visibility = value; RaisePropertyChanged(); }
        }


        private List<DevPara> _mesurePara = new List<DevPara>();

        public List<DevPara> MeasurePara
        {
            get { return _mesurePara; }
            set { _mesurePara = value; RaisePropertyChanged(); }
        }

        private List<DevPara> _settingPara = new List<DevPara>();

        public List<DevPara> SettingPara
        {
            get { return _settingPara; }
            set { _settingPara = value; RaisePropertyChanged(); }
        }

        private List<DevPara> _statusPara = new List<DevPara>();

        public List<DevPara> StatusPara
        {
            get { return _statusPara; }
            set { _statusPara = value; RaisePropertyChanged(); }
        }

        private Measure _m = new Measure();
        public Measure M
        {
            get { return _m; }
            set { _m = value; RaisePropertyChanged(); }
        }

        private Setting _s = new Setting();
        public Setting S
        {
            get { return _s; }
            set { _s = value; RaisePropertyChanged(); }
        }



        private Status _statusInfo = new Status();

        public Status StatusInfo
        {
            get { return _statusInfo; }
            set { _statusInfo = value; RaisePropertyChanged(); }
        }





    }

    public class DevPara : BindableBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(); }
        }
        private string _value;
        public string Vaule
        {
            get { return _value; }
            set { _value = value; RaisePropertyChanged(); }
        }
    }


}
