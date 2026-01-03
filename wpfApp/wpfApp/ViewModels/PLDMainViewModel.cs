using DryIoc;
using Prism.Commands;
using Prism.Dialogs;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Devices.PointOfService;
using wpfApp.Common.CtrlProtocol;
using wpfApp.Common.Dialog;
using wpfApp.Common.PLDProtocol;
using wpfApp.Views;
using static wpfApp.ViewModels.PLDMainViewModel.MeasureParam;


namespace wpfApp.ViewModels
{
    public partial class PLDMainViewModel : NavigationViewModel
    {
        private readonly IRegionManager regionManager;
        private readonly IContainerExtension container;
        private readonly IDialogHostService dialogHostService;

        public DelegateCommand<string> ExecuteCommand { get; private set; }

        public PLDMainViewModel(IRegionManager regionManager, IContainerExtension container, IDialogHostService dialogHostService) : base(container)
        {
            this.container = container;
            this.regionManager = regionManager;
            this.dialogHostService = dialogHostService;
            if (PortName.Count > 0)
            {
                DevPortName = PortName.First();
            }
            List<byte> bytes = new List<byte>() { 00, 0x1b, 0xff, 0x00, 0xdd, 0x1b, 0x00, 0xff,0x00,0x00,0x00,0x00 };
            var tt = CRC32_Uint32(bytes.ToArray());

            ExecuteCommand = new DelegateCommand<string>(Execute);
            protocolManager.SetProtocol(new Protocol_X004_001());
            protocolManager.SetCallback(serialPortDataCallBack);
            DeviceSerialPort.DataReceived += serialPortDataReceived;
            setParam = pLDParams.SetParams;
            InitModels();
            
            ConnectTimerInit();
            BitTimerInit();            
        }

        void ShowDataChart()
        {
            DialogParameters param = new DialogParameters();
            //param.Add(key: "SigName", MeasureDataChartSelected);
            //param.Add("CH", CH);
            //param.Add("DevName", NameSpace);
            //param.Add("DevMeasureDatas", devMeasureDatas);

            dialogHostService.Show("PLDChart", param, null);
                //dialogHostService.Show("DataChartView2", param, null);
            
        }

        #region Params
        void InitModels()
        {
            KBModelList = new ObservableCollection<KBModel>()
            {
                new KBModel(){Tile = "LD1电流设定值标定",Icon="CogSync", CommandParameter = "设置LD1SKB", KB=SettingParams.LDParams[0].CalibSet,KB2Show=SettingParamsToShow.LDParams[0].CalibSet},
                new KBModel(){Tile = "LD2电流设定值标定",Icon="CogSync", CommandParameter = "设置LD2SKB", KB=SettingParams.LDParams[1].CalibSet,KB2Show=SettingParamsToShow.LDParams[1].CalibSet},
                new KBModel(){Tile = "LD1电流测量值标定",Icon="CogSync", CommandParameter = "设置LD1MKB", KB=SettingParams.LDParams[0].CalibMeasure,KB2Show=SettingParamsToShow.LDParams[0].CalibMeasure},
                new KBModel(){Tile = "LD2电流测量值标定",Icon="CogSync", CommandParameter = "设置LD2MKB", KB=SettingParams.LDParams[1].CalibMeasure,KB2Show=SettingParamsToShow.LDParams[1].CalibMeasure},
            };
        
            PIDModelList = new ObservableCollection<PIDModel>()
            { 
                new PIDModel(){Tile = "TEC1 PID标定",Icon="CogSync",CommandParameter = "设置TEC1PID", PID = SettingParams.TECParams[0].PID, PID2Show = SettingParamsToShow.TECParams[0].PID},
                new PIDModel(){Tile = "TEC2 PID标定",Icon="CogSync",CommandParameter = "设置TEC2PID", PID = SettingParams.TECParams[1].PID, PID2Show = SettingParamsToShow.TECParams[1].PID},
                new PIDModel(){Tile = "TEC3 PID标定",Icon="CogSync",CommandParameter = "设置TEC3PID", PID = SettingParams.TECParams[2].PID, PID2Show = SettingParamsToShow.TECParams[2].PID},
                new PIDModel(){Tile = "TEC4 PID标定",Icon="CogSync",CommandParameter = "设置TEC4PID", PID = SettingParams.TECParams[3].PID, PID2Show = SettingParamsToShow.TECParams[3].PID},
            };

            PulseIntervalModelList = new ObservableCollection<PulseIntervalModel>();
            for (int i = 0; i < 11; i++)
            {
                PulseIntervalModelList.Add(new PulseIntervalModel() { Tile = "间隔" + (i + 1), Interval = SettingParams.PulseParams.Interval[i], Interval2Show = SettingParamsToShow.PulseParams.Interval[i] });

            }
            
        }


        private ObservableCollection<KBModel> _KBModelList;
        public ObservableCollection<KBModel> KBModelList
        {
            get { return _KBModelList; }
            set { _KBModelList = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<PIDModel> _PIDModelList;
        public ObservableCollection<PIDModel> PIDModelList
        {
            get { return _PIDModelList; }
            set { _PIDModelList = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<PulseIntervalModel> _PulseIntervalModelList;
        public ObservableCollection<PulseIntervalModel> PulseIntervalModelList
        {
            get { return _PulseIntervalModelList; }
            set { _PulseIntervalModelList = value; RaisePropertyChanged(); }
        }

        public class CalibParam : BindableBase
        {
            private string _k;

            public string K
            {
                get { return _k; }
                set { _k = value; RaisePropertyChanged(); }
            }
            private string _b;

            public string B
            {
                get { return _b; }
                set { _b = value; RaisePropertyChanged(); }
            }
        }

        public class PIDCalibParam : BindableBase
        {
            private string _p;

            public string P
            {
                get { return _p; }
                set { _p = value; RaisePropertyChanged(); }
            }
            private string _i;

            public string I
            {
                get { return _i; }
                set { _i = value; RaisePropertyChanged(); }
            }

            private string _d;

            public string D
            {
                get { return _d; }
                set { _d = value; RaisePropertyChanged(); }
            }
        }
        
        public class PulseInterval : BindableBase
        {
            private string _value;

            public string Value
            {
                get { return _value; }
                set { _value = value; RaisePropertyChanged(); }
            }
        }

        public class PulseIntervalModel : BindableBase
        {
            private string _tile;
            public string Tile
            {
                get { return _tile; }
                set { _tile = value; RaisePropertyChanged(); }
            }

            private PulseInterval _interval;
            public PulseInterval Interval
            {
                get { return _interval; }
                set { _interval = value; RaisePropertyChanged(); }
            }

            private PulseInterval _interval2Show;
            public PulseInterval Interval2Show
            {
                get { return _interval2Show; }
                set { _interval2Show = value; RaisePropertyChanged(); }
            }
        }

        public class KBModel : BindableBase
        {
            private string _tile;
            public string Tile
            {
                get { return _tile; }
                set { _tile = value; RaisePropertyChanged(); }
            }
            private string _icon;
            public string Icon
            {
                get { return _icon; }
                set { _icon = value; RaisePropertyChanged(); }
            }

            private string _commandParameter;
            public string CommandParameter
            {
                get { return _commandParameter; }
                set { _commandParameter = value; RaisePropertyChanged(); }
            }

            private CalibParam _KB;
            public CalibParam KB
            {
                get { return _KB; }
                set { _KB = value; RaisePropertyChanged(); }
            }

            private CalibParam _KB2Show;
            public CalibParam KB2Show
            {
                get { return _KB2Show; }
                set { _KB2Show = value; RaisePropertyChanged(); }
            }

        }
   
        public class PIDModel : BindableBase
        {
            private string _tile;
            public string Tile
            {
                get { return _tile; }
                set { _tile = value; RaisePropertyChanged(); }
            }
            private string _icon;
            public string Icon
            {
                get { return _icon; }
                set { _icon = value; RaisePropertyChanged(); }
            }

            private string _commandParameter;
            public string CommandParameter
            {
                get { return _commandParameter; }
                set { _commandParameter = value; RaisePropertyChanged(); }
            }

            private PIDCalibParam _PID;
            public PIDCalibParam PID
            {
                get { return _PID; }
                set { _PID = value; RaisePropertyChanged(); }
            }

            private PIDCalibParam _PID2Show;
            public PIDCalibParam PID2Show
            {
                get { return _PID2Show; }
                set { _PID2Show = value; RaisePropertyChanged(); }
            }

        }

        private SettingParam _settingParamsToShow = new SettingParam();

        public SettingParam SettingParamsToShow
        {
            get { return _settingParamsToShow; }
            set { _settingParamsToShow = value; RaisePropertyChanged(); }
        }


        private SettingParam _settingParams = new SettingParam();

        public SettingParam SettingParams
        {
            get { return _settingParams; }
            set { _settingParams = value; RaisePropertyChanged(); }
        }

        private MeasureParam _measureParams = new MeasureParam();

        public MeasureParam MeasureParams
        {
            get { return _measureParams ; }
            set { _measureParams =  value; RaisePropertyChanged(); }
        }

        

        public class MeasureParam : BindableBase
        {

            public class Param : BindableBase
            {
                public string Name { get; set ; }
                private string _Value;

                public string Value
                {
                    get { return _Value; }
                    set { _Value = value; RaisePropertyChanged(); }
                }
                public string Unit { get; set; }
            }

            public class LDParam : BindableBase
            {
                private Param _temp = new Param() { Name = "温度:", Value = "00.0", Unit = "℃" };
                public Param Temp
                {
                    get { return _temp; }
                    set { _temp = value; RaisePropertyChanged(); }
                }
                private Param _vol = new Param() { Name = "电压:", Value = "00.0", Unit = "V" };
                public Param Vol
                {
                    get { return _vol; }
                    set { _vol = value; RaisePropertyChanged(); }
                }
                private Param _curr = new Param() { Name = "电流:", Value = "00.0", Unit = "A" };
                public Param Curr
                {
                    get { return _curr; }
                    set { _curr = value; RaisePropertyChanged(); }
                }
            }

            private List<LDParam> _ldParams;
            public List<LDParam> LDParams
            {
                get { return _ldParams; }
                set { _ldParams = value; RaisePropertyChanged(); }
            }

            public class TECParam : BindableBase
            {
                private Param _temp = new Param() { Name = "温度:", Value = "00.0", Unit = "℃" };
                public Param Temp
                {
                    get { return _temp; }
                    set { _temp = value; RaisePropertyChanged(); }
                }
                private Param _vol = new Param() { Name = "电压:", Value = "00.0", Unit = "V" };
                public Param Vol
                {
                    get { return _vol; }
                    set { _vol = value; RaisePropertyChanged(); }
                }
                private Param _power = new Param() { Name = "功率:", Value = "00.0", Unit = "W" };
                public Param Power
                {
                    get { return _power; }
                    set { _power = value; RaisePropertyChanged(); }
                }

                private Param _curr = new Param() { Name = "电流:", Value = "00.0", Unit = "A" };
                public Param Curr
                {
                    get { return _curr; }
                    set { _curr = value; RaisePropertyChanged(); }
                }
            }

            private List<TECParam> _tecParams;

            public List<TECParam> TECParams
            {
                get { return _tecParams; }
                set { _tecParams = value; RaisePropertyChanged(); }
            }


            public class PDParam : BindableBase
            {
                private Param _power = new Param() { Name = "功率:", Value = "00.0", Unit = "W" };
                public Param Power
                {
                    get { return _power; }
                    set { _power = value; RaisePropertyChanged(); }
                }
                private Param _temp = new Param() { Name = "温度:", Value = "00.0", Unit = "℃" };
                public Param Temp
                {
                    get { return _temp; }
                    set { _temp = value; RaisePropertyChanged(); }
                }
            }


            private PDParam _pdParams = new PDParam();

            public PDParam PDParams
            {
                get { return _pdParams; }
                set { _pdParams = value; RaisePropertyChanged(); }
            }


            public class LCMParam : BindableBase
            {
                private Param _vol = new Param() { Name = "电压:", Value = "000.0", Unit = "V" };
                public Param Vol
                {
                    get { return _vol; }
                    set { _vol = value; RaisePropertyChanged(); }
                }
                private Param _curr = new Param() { Name = "电流:", Value = "00.0", Unit = "A" };
                public Param Curr
                {
                    get { return _curr; }
                    set { _curr = value; RaisePropertyChanged(); }
                }

                private Param _power = new Param() { Name = "功率:", Value = "000.0", Unit = "W" };
                public Param Power
                {
                    get { return _power; }
                    set { _power = value; RaisePropertyChanged(); }
                }
                private Param _motorSped = new Param() { Name = "电机转速:", Value = "0", Unit = "rpm" };
                public Param MotorSpeed
                {
                    get { return _motorSped; }
                    set { _motorSped = value; RaisePropertyChanged(); }
                }

                private Param _temp = new Param() { Name = "控制器温度:", Value = "00.0", Unit = "℃" };
                public Param Temp
                {
                    get { return _temp; }
                    set { _temp = value; RaisePropertyChanged(); }
                }
            }

            private LCMParam _lcmParam = new LCMParam();

            public LCMParam LCMParams
            {
                get { return _lcmParam; }
                set { _lcmParam = value; RaisePropertyChanged(); }
            }

            public class Status : BindableBase
            {

                private Param _mode = new Param() { Name = "调制:", Value = "Nan" };
                /// <summary>
                /// 调制模式
                /// </summary>
                public Param Mode
                {
                    get { return _mode; }
                    set { _mode = value; RaisePropertyChanged(); }
                }

                private Param _freqType = new Param() { Name = "脉冲类型:", Value = "Nan" };
                /// <summary>
                /// 调制模式
                /// </summary>
                public Param PulseType
                {
                    get { return _freqType; }
                    set { _freqType = value; RaisePropertyChanged(); }
                }
                private List<Param> _ldStatus = new List<Param> {
                new Param() { Name = "LD1:", Value = "Nan" },
                new Param() { Name = "LD2:", Value = "Nan" }
                };
                /// <summary>
                /// LD状态
                /// </summary>
                public List<Param> LDStatus
                {
                    get { return _ldStatus; }
                    set { _ldStatus = value; RaisePropertyChanged(); }
                }



                private List<Param> _tecStatus = new List<Param> {
                new Param() { Name = "TEC1:", Value = "Nan" } ,
                new Param() { Name = "TEC2:", Value = "Nan" } ,
                new Param() { Name = "TEC3:", Value = "Nan" } ,
                new Param() { Name = "TEC4:", Value = "Nan" } ,
            };
                /// <summary>
                /// TEC状态
                /// </summary>
                public List<Param> TECStatus
                {
                    get { return _tecStatus; }
                    set { _tecStatus = value; RaisePropertyChanged(); }
                }

                private Param _tQStatus = new Param() { Name = "调Q:", Value = "Nan" };
                /// <summary>
                /// 调Q状态
                /// </summary>
                public Param TQStatus
                {
                    get { return _tQStatus; }
                    set { _tQStatus = value; RaisePropertyChanged(); }
                }

                private Param _pumpStatus = new Param() { Name = "水泵:", Value = "Nan" };
                /// <summary>
                /// 水泵状态
                /// </summary>
                public Param PumpStatus
                {
                    get { return _pumpStatus; }
                    set { _pumpStatus = value; RaisePropertyChanged(); }
                }

                private Param _fanStatus = new Param() { Name = "风扇:", Value = "Nan" };
                /// <summary>
                /// 水泵状态
                /// </summary>
                public Param FanStatus
                {
                    get { return _fanStatus; }
                    set { _fanStatus = value; RaisePropertyChanged(); }
                }

                private Param _bitStatus = new Param() { Name = "自检状态:", Value = "Nan" };
                /// <summary>
                /// 自检状态
                /// </summary>
                public Param BITStatus
                {
                    get { return _bitStatus; }
                    set { _bitStatus = value; RaisePropertyChanged(); }
                }

                private Param _connectStatus = new Param() { Name = "连接状态:", Value = "未连接" };
                /// <summary>
                /// 自检状态
                /// </summary>
                public Param ConnectStatus
                {
                    get { return _connectStatus; }
                    set { _connectStatus = value; RaisePropertyChanged(); }
                }
            }

            private Status _statusInfo = new Status();

            public Status StatusInfo
            {
                get { return _statusInfo; }
                set { _statusInfo = value; RaisePropertyChanged(); }
            }

            private List<Param> _statusList = new List<Param>();

            public List<Param> StatusList
            {
                get { return _statusList; }
                set { _statusList = value; RaisePropertyChanged(); }
            }
            public class MeasureParamInfo : BindableBase
            {
                private string _tile;
                public string Tile
                {
                    get { return _tile; }
                    set { _tile = value; RaisePropertyChanged(); }
                }
                private string _icon;
                public string Icon
                {
                    get { return _icon; }
                    set { _icon = value; RaisePropertyChanged(); }
                }

                private List<Param> _params;
                public List<Param> Params
                {
                    get { return _params; }
                    set { _params = value; RaisePropertyChanged(); }
                }
            }

            private ObservableCollection<MeasureParamInfo> _measureParamsShowList;
            public ObservableCollection<MeasureParamInfo> MeasureParamsShowList
            {
                get { return _measureParamsShowList; }
                set { _measureParamsShowList = value; RaisePropertyChanged(); }
            }


            public class OtherInfo : BindableBase
            {
                private Param _pwrtemp = new Param() { Name = "电源温度:", Value = "00.0", Unit = "℃" };
                public Param PwrTemp
                {
                    get { return _pwrtemp; }
                    set { _pwrtemp = value; RaisePropertyChanged(); }
                }
                private Param _sysVol = new Param() { Name = "系统电压:", Value = "00.0", Unit = "V" };
                public Param SysVol
                {
                    get { return _sysVol; }
                    set { _sysVol = value; RaisePropertyChanged(); }
                }
                private Param _sysCurr = new Param() { Name = "系统电流:", Value = "00.0", Unit = "A" };
                public Param SysCurr
                {
                    get { return _sysCurr; }
                    set { _sysCurr = value; RaisePropertyChanged(); }
                }
            }
            private OtherInfo _otherInfos = new OtherInfo();

            public OtherInfo OtherInfos
            {
                get { return _otherInfos; }
                set { _otherInfos = value; RaisePropertyChanged(); }
            }


            public MeasureParam()
            {

                StatusList = new List<Param>() { /*StatusInfo.Mode,StatusInfo.FreqType,*/ StatusInfo.LDStatus[0], StatusInfo.LDStatus[1],
                StatusInfo.TECStatus[0], StatusInfo.TECStatus[1] , StatusInfo.TECStatus[2], StatusInfo.TECStatus[3] ,
                StatusInfo.TQStatus, StatusInfo.PumpStatus,StatusInfo.FanStatus};

                LDParams = new List<LDParam>() { new LDParam() { }, new LDParam() { } };
                TECParams = new List<TECParam>() { new TECParam(), new TECParam(), new TECParam(), new TECParam() };
                MeasureParamsShowList = new ObservableCollection<MeasureParamInfo>();
                MeasureParamsShowList.Add(new MeasureParamInfo()
                {
                    Tile = "LD1测量参数:",
                    //Icon = "AlphaLCircle",
                    Icon = "AlphaLBox",
                    Params = new List<Param>() {
                        LDParams[0].Temp,LDParams[0].Curr,LDParams[0].Vol }
                });
                MeasureParamsShowList.Add(new MeasureParamInfo()
                {
                    Tile = "LD2测量参数:",
                    Icon = "AlphaLBox",
                    Params = new List<Param>() {
                        LDParams[1].Temp,LDParams[1].Curr,LDParams[1].Vol }
                });

                MeasureParamsShowList.Add(new MeasureParamInfo()
                {
                    Tile = "TEC1测量参数:",
                    Icon = "AlphaTBox",
                    Params = new List<Param>() {
                        TECParams[0].Temp,TECParams[0].Curr,TECParams[0].Power }
                });
                MeasureParamsShowList.Add(new MeasureParamInfo()
                {
                    Tile = "TEC2测量参数:",
                    Icon = "AlphaTBox",
                    Params = new List<Param>() {
                        TECParams[1].Temp,TECParams[1].Curr,TECParams[1].Power }
                });
                MeasureParamsShowList.Add(new MeasureParamInfo()
                {
                    Tile = "TEC3测量参数:",
                    Icon = "AlphaTBox",
                    Params = new List<Param>() {
                        TECParams[2].Temp,TECParams[2].Curr,TECParams[2].Power }
                });
                MeasureParamsShowList.Add(new MeasureParamInfo()
                {
                    Tile = "TEC4测量参数:",
                    Icon = "AlphaTBox",
                    Params = new List<Param>() {
                        TECParams[3].Temp,TECParams[3].Curr,TECParams[3].Power }
                });

                MeasureParamsShowList.Add(new MeasureParamInfo()
                {
                    Tile = "光功率测量参数:",
                    Icon = "AlphaGBox",
                    Params = new List<Param>() {
                        PDParams.Temp,PDParams.Power }
                });

                MeasureParamsShowList.Add(new MeasureParamInfo()
                {
                    Tile = "液冷测量参数:",
                    Icon = "AlphaYBox",
                    Params = new List<Param>() {
                       LCMParams.Vol,LCMParams.Curr,LCMParams.Power,LCMParams.MotorSpeed,LCMParams.Temp }
                });
                MeasureParamsShowList.Add(new MeasureParamInfo()
                {
                    Tile = "其他信息:",
                    Icon = "AlphaQBox",
                    Params = new List<Param>() {
                       OtherInfos.PwrTemp ,OtherInfos.SysCurr,OtherInfos.SysVol}
                });
            }
        }



        public class SettingParam : BindableBase
        {

            private string _mode = "内调制";
            /// <summary>
            /// 工作模式
            /// </summary>
            public string Mode
            {
                get { return _mode; }
                set { _mode = value; RaisePropertyChanged(); }
            }

            private string _freqType = "变频";
            /// <summary>
            /// 工作模式
            /// </summary>
            public string PulseType
            {
                get { return _freqType; }
                set { _freqType = value; RaisePropertyChanged(); }
            }

            private CalibParam _calibPD = new CalibParam() { K = "1.00", B = "0.00" };

            public CalibParam CalibPD
            {
                get { return _calibPD; }
                set { _calibPD = value; RaisePropertyChanged(); }
            }



            public class LDParam : BindableBase
            {

                private bool _IsWork = false;

                public bool IsWork
                {
                    get { return _IsWork; }
                    set { _IsWork = value; RaisePropertyChanged(); }
                }

                private string _temp = "20.0";
                /// <summary>
                /// LD温度
                /// </summary>
                public string Temp
                {
                    get { return _temp; }
                    set { _temp = value; RaisePropertyChanged(); }
                }

                private string _curr = "0.0";
                /// <summary>
                /// LD电流
                /// </summary>
                public string Curr
                {
                    get { return _curr; }
                    set { _curr = value; RaisePropertyChanged(); }
                }

                private string _vol = "25.0";
                /// <summary>
                /// LD电压
                /// </summary>
                public string Vol
                {
                    get { return _vol; }
                    set { _vol = value; RaisePropertyChanged(); }
                }

                private string _MaxCurr = "16.0";
                /// <summary>
                /// LD最大电流
                /// </summary>
                public string MaxCurr
                {
                    get { return _MaxCurr; }
                    set { _MaxCurr = value; RaisePropertyChanged(); }
                }

                private string _HOC = "16.6";
                /// <summary>
                /// LD硬件过流保护
                /// </summary>
                public string HOC
                {
                    get { return _HOC; }
                    set { _HOC = value; RaisePropertyChanged(); }
                }

                private CalibParam _calibMeasure = new CalibParam() { K = "1.00", B = "0.00" };

                public CalibParam CalibMeasure
                {
                    get { return _calibMeasure; }
                    set { _calibMeasure = value; }
                }

                private CalibParam _calibSet = new CalibParam() { K="1.00",B="0.00"};

                public CalibParam CalibSet
                {
                    get { return _calibSet; }
                    set { _calibSet = value; }
                }

            }

            public class TECParam : BindableBase
            {

                private bool _IsWork = false;

                public bool IsWork
                {
                    get { return _IsWork; }
                    set { _IsWork = value; RaisePropertyChanged(); }
                }

                private string _temp = "20.0";
                /// <summary>
                /// 温度
                /// </summary>
                public string Temp
                {
                    get { return _temp; }
                    set { _temp = value; RaisePropertyChanged(); }
                }

             

                private string _vol = "19.5";
                /// <summary>
                /// 电压
                /// </summary>
                public string Vol
                {
                    get { return _vol; }
                    set { _vol = value; RaisePropertyChanged(); }
                }
                

                private PIDCalibParam _PID = new PIDCalibParam() { P="0.50",I="0.15",D="0.05"};

                public PIDCalibParam PID
                {
                    get { return _PID; }
                    set { _PID = value; }
                }
            }

            public class TQParam : BindableBase
            {
                private string _IsWork ="开";

                public string IsWork
                {
                    get { return _IsWork; }
                    set { _IsWork = value; RaisePropertyChanged(); }
                }

                private string _delay = "100";

                public string Delay
                {
                    get { return _delay; }
                    set { _delay = value; RaisePropertyChanged(); }
                }
            }

            public class PulseParam : BindableBase
            {
                private string _width = "200";

                public string Width
                {
                    get { return _width; }
                    set { _width = value; RaisePropertyChanged(); }
                }

                private string _freq = "1000";

                public string Freq
                {
                    get { return _freq; }
                    set { _freq = value; RaisePropertyChanged(); }
                }

                private string _num ="11";

                public string Num
                {
                    get { return _num; }
                    set { _num = value; RaisePropertyChanged(); }
                }


                private List<PulseInterval>  _interval = new List<PulseInterval>();

                public List<PulseInterval> Interval
                {
                    get { return _interval; }
                    set { _interval = value; RaisePropertyChanged(); }
                }

            }
            
            public class LCMParam : BindableBase
            {
                private string _IsPowerOn = "开";

                public string IsPowerOn
                {
                    get { return _IsPowerOn; }
                    set { _IsPowerOn = value; RaisePropertyChanged(); }
                }

                private string _IsMotorWork = "开";

                public string IsIsMotorWork
                {
                    get { return _IsMotorWork; }
                    set { _IsMotorWork = value; RaisePropertyChanged(); }
                }

                private string _fanSpeed ="关";

                public string FanSpeed
                {
                    get { return _fanSpeed; }
                    set { _fanSpeed = value; RaisePropertyChanged(); }
                }

                private string _motorSpeed="30000";

                public string MotorSpeed
                {
                    get { return _motorSpeed; }
                    set { _motorSpeed = value; RaisePropertyChanged(); }
                }

                private string _PwrLimit="150";

                public string PwrLimit
                {
                    get { return _PwrLimit; }
                    set { _PwrLimit = value; RaisePropertyChanged(); }
                }

            }



            private List<LDParam> _ldParams;
            public List<LDParam> LDParams
            {
                get { return _ldParams; }
                set { _ldParams = value; RaisePropertyChanged(); }
            }

            private List<TECParam> _tecParams;
            public List<TECParam> TECParams
            {
                get { return _tecParams; }
                set { _tecParams = value; RaisePropertyChanged(); }
            }

            private LCMParam _lcmParam = new LCMParam();
            public LCMParam LCMParams
            {
                get { return _lcmParam; }
                set { _lcmParam = value; RaisePropertyChanged(); }
            }

            private TQParam _TQParams = new TQParam();
            public TQParam TQParams
            {
                get { return _TQParams; }
                set { _TQParams = value; RaisePropertyChanged(); }
            }

            private PulseParam _PulseParams = new PulseParam();
            public PulseParam PulseParams
            {
                get { return _PulseParams; }
                set { _PulseParams = value; RaisePropertyChanged(); }
            }

            public SettingParam()
            {
                
                LDParams = new List<LDParam>() { new LDParam() { }, new LDParam() { } };
                TECParams = new List<TECParam>() { new TECParam(), new TECParam(), new TECParam(), new TECParam() };

                for (int i = 0; i < 11; i++)
                {
                    PulseParams.Interval.Add(new PulseInterval() { Value = "250" });
                }
            }

        }


        private bool _isUpdatePara = false;

        public bool IsUpdatePara
        {
            get { return _isUpdatePara; }
            set { _isUpdatePara = value; RaisePropertyChanged(); }
        }


        private List<string> _modeList = new List<string>() { "内调制","外调制"};
        public List<string> ModeList
        {
            get { return _modeList; }
            set { _modeList = value; RaisePropertyChanged(); }
        }

        private List<string> _pulseTypeList = new List<string>() { "变频", "定频" };
        public List<string> PulseTypeList
        {
            get { return _pulseTypeList; }
            set { _pulseTypeList = value; RaisePropertyChanged(); }
        }

        private List<string> _swicthList = new List<string>() { "开", "关" };
        public List<string> SwicthList
        {
            get { return _swicthList; }
            set { _swicthList = value; RaisePropertyChanged(); }
        }


        private string _fileSavePath = @".\";

        public string FileSavePath
        {
            get { return _fileSavePath; }
            set { _fileSavePath = value; RaisePropertyChanged(); }
        }


        private UpdateInfo _Update =new UpdateInfo();

        public UpdateInfo Update
        {
            get { return _Update; }
            set { _Update = value; RaisePropertyChanged(); }
        }

        public class UpdateInfo:BindableBase
        {
            private string _FilePath = @".\";

            public string FilePath
            {
                get { return _FilePath; }
                set { _FilePath = value; RaisePropertyChanged(); }
            }
            private string _Version = "NA";
            public string Version
            {
                get { return _Version; }
                set { _Version = value; RaisePropertyChanged(); }
            }

            private string _LastResult = "NA";
            public string LastResult
            {
                get { return _LastResult; }
                set { _LastResult = value; RaisePropertyChanged(); }
            }

            private bool _IsUpdate = false;

            public bool IsUpdate
            {
                get { return _IsUpdate; }
                set { _IsUpdate = value; RaisePropertyChanged(); }
            }

            private float _ProcessValue = 0;

            public float ProcessValue
            {
                get { return _ProcessValue; }
                set { _ProcessValue = value; RaisePropertyChanged(); }
            }

            private string _ButtonContent = "升级";
            public string ButtonContent
            {
                get { return _ButtonContent; }
                set { _ButtonContent = value; RaisePropertyChanged(); }
            }
            public bool IsAccess { get; set; }

            public PLDParams.BootMode BootMode { get; set; } = PLDParams.BootMode.None;

            public PLDParams.UpgradeStatus UpgradeStatus { get; set; }

            public UInt16 CurrIdx { get; set; }
        }



        private bool _LDCtrlEn = true;

        public bool LDCtrlEn
        {
            get { return _LDCtrlEn ; }
            set { _LDCtrlEn = value; RaisePropertyChanged(); }
        }




        #endregion



    }


    

}
