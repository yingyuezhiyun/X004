using DryIoc;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Devices.PointOfService;
using wpfApp.Common.Dialog;

namespace wpfApp.ViewModels
{
    public class PLDMainViewModel : NavigationViewModel
    {
        private readonly IRegionManager regionManager;
        private readonly IContainerExtension container;
        private readonly IDialogHostService dialogHostService;

        public PLDMainViewModel(IRegionManager regionManager, IContainerExtension container, IDialogHostService dialogHostService) : base(container)
        {
            this.container = container;
            this.regionManager = regionManager;
            this.dialogHostService = dialogHostService;
           
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
          
            public class Param
            {
                public string Name { get; set; }
                public string Value { get; set; }
                public string Unit { get; set; }
            }

            public class LDParam : BindableBase
            {
                private Param _temp = new Param() { Name = "温度:", Value = "26.0", Unit = "℃" };
                public Param Temp
                {
                    get { return _temp; }
                    set { _temp = value; RaisePropertyChanged(); }
                }
                private Param _vol = new Param() { Name = "电压:", Value = "27.0", Unit = "V" };
                public Param Vol
                {
                    get { return _vol; }
                    set { _vol = value; RaisePropertyChanged(); }
                }
                private Param _curr = new Param() { Name = "电流:", Value = "11.0", Unit = "A" };
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
                private Param _temp = new Param() { Name = "温度:", Value = "26.0", Unit = "℃" };
                public Param Temp
                {
                    get { return _temp; }
                    set { _temp = value; RaisePropertyChanged(); }
                }
                private Param _vol = new Param() { Name = "电压:", Value = "27.0", Unit = "V" };
                public Param Vol
                {
                    get { return _vol; }
                    set { _vol = value; RaisePropertyChanged(); }
                }
                private Param _curr = new Param() { Name = "电流:", Value = "11.0", Unit = "A" };
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
                private Param _vol = new Param() { Name = "电压:", Value = "27.0", Unit = "V" };
                public Param Vol
                {
                    get { return _vol; }
                    set { _vol = value; RaisePropertyChanged(); }
                }
                private Param _curr = new Param() { Name = "电流:", Value = "11.0", Unit = "A" };
                public Param Curr
                {
                    get { return _curr; }
                    set { _curr = value; RaisePropertyChanged(); }
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
                private Param _vol = new Param() { Name = "电压:", Value = "227.0", Unit = "V" };
                public Param Vol
                {
                    get { return _vol; }
                    set { _vol = value; RaisePropertyChanged(); }
                }
                private Param _curr = new Param() { Name = "电流:", Value = "41.0", Unit = "A" };
                public Param Curr
                {
                    get { return _curr; }
                    set { _curr = value; RaisePropertyChanged(); }
                }

                private Param _power = new Param() { Name = "功率:", Value = "127.0", Unit = "W" };
                public Param Power
                {
                    get { return _power; }
                    set { _power = value; RaisePropertyChanged(); }
                }
                private Param _motorSped = new Param() { Name = "电机转速:", Value = "30000", Unit = "rpm" };
                public Param MotorSpeed
                {
                    get { return _motorSped; }
                    set { _motorSped = value; RaisePropertyChanged(); }
                }

                private Param _temp = new Param() { Name = "控制器温度:", Value = "26.0", Unit = "℃" };
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

            public MeasureParam()
            {

                StatusList = new List<Param>() {StatusInfo.Mode, StatusInfo.LDStatus[0], StatusInfo.LDStatus[1],
                StatusInfo.TECStatus[0], StatusInfo.TECStatus[1] , StatusInfo.TECStatus[2], StatusInfo.TECStatus[3] ,
                StatusInfo.TQStatus, StatusInfo.PumpStatus,StatusInfo.FanStatus};

                LDParams = new List<LDParam>() { new LDParam() { }, new LDParam() { } };
                TECParams = new List<TECParam>() { new TECParam(), new TECParam(), new TECParam(), new TECParam() };
                MeasureParamsShowList = new ObservableCollection<MeasureParamInfo>();
                MeasureParamsShowList.Add(new MeasureParamInfo()
                {
                    Tile = "LD1测量参数:",
                    Icon = "CogSync",
                    Params = new List<Param>() {
                        LDParams[0].Temp,LDParams[0].Curr,LDParams[0].Vol }
                });
                MeasureParamsShowList.Add(new MeasureParamInfo()
                {
                    Tile = "LD2测量参数:",
                    Icon = "CogSync",
                    Params = new List<Param>() {
                        LDParams[1].Temp,LDParams[1].Curr,LDParams[1].Vol }
                });

                MeasureParamsShowList.Add(new MeasureParamInfo()
                {
                    Tile = "TEC1测量参数:",
                    Icon = "CogSync",
                    Params = new List<Param>() {
                       /* TECParams[0].Temp,*/TECParams[0].Curr,TECParams[0].Vol }
                });
                MeasureParamsShowList.Add(new MeasureParamInfo()
                {
                    Tile = "TEC2测量参数:",
                    Icon = "CogSync",
                    Params = new List<Param>() {
                       /* TECParams[1].Temp,*/TECParams[1].Curr,TECParams[1].Vol }
                });
                MeasureParamsShowList.Add(new MeasureParamInfo()
                {
                    Tile = "TEC3测量参数:",
                    Icon = "CogSync",
                    Params = new List<Param>() {
                        TECParams[2].Temp,TECParams[2].Curr,TECParams[2].Vol }
                });
                MeasureParamsShowList.Add(new MeasureParamInfo()
                {
                    Tile = "TEC4测量参数:",
                    Icon = "CogSync",
                    Params = new List<Param>() {
                        TECParams[3].Temp,TECParams[3].Curr,TECParams[3].Vol }
                });

                MeasureParamsShowList.Add(new MeasureParamInfo()
                {
                    Tile = "PD测量参数:",
                    Icon = "CogSync",
                    Params = new List<Param>() {
                        PDParams.Curr,PDParams.Vol }
                });

                MeasureParamsShowList.Add(new MeasureParamInfo()
                {
                    Tile = "液冷测量参数:",
                    Icon = "CogSync",
                    Params = new List<Param>() {
                       LCMParams.Vol,LCMParams.Curr,LCMParams.Power,LCMParams.MotorSpeed,LCMParams.Temp }
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

            private CalibParam _calibPD = new CalibParam();

            public CalibParam CalibPD
            {
                get { return _calibPD; }
                set { _calibPD = value; RaisePropertyChanged(); }
            }


            public class CalibParam
            {
                private string _k;

                public string K
                {
                    get { return _k; }
                    set { _k = value; }
                }
                private string _b;

                public string B
                {
                    get { return _b; }
                    set { _b = value; }
                }
            }

            public class PIDCalibParam
            {
                private string _p;

                public string P
                {
                    get { return _p; }
                    set { _p = value; }
                }
                private string _i;

                public string I
                {
                    get { return _i; }
                    set { _i = value; }
                }

                private string _d;

                public string D
                {
                    get { return _d; }
                    set { _d = value; }
                }
            }

            public class LDParam : BindableBase
            {

                private bool _IsWork = false;

                public bool IsWork
                {
                    get { return _IsWork; }
                    set { _IsWork = value; RaisePropertyChanged(); }
                }

                private string _temp = "20.000";
                /// <summary>
                /// LD温度
                /// </summary>
                public string Temp
                {
                    get { return _temp; }
                    set { _temp = value; RaisePropertyChanged(); }
                }

                private string _curr = "0.000";
                /// <summary>
                /// LD电流
                /// </summary>
                public string Curr
                {
                    get { return _curr; }
                    set { _curr = value; RaisePropertyChanged(); }
                }

                private string _vol = "0.000";
                /// <summary>
                /// LD电压
                /// </summary>
                public string Vol
                {
                    get { return _vol; }
                    set { _vol = value; RaisePropertyChanged(); }
                }

                private string _MaxCurr = "0.000";
                /// <summary>
                /// LD最大电流
                /// </summary>
                public string MaxCurr
                {
                    get { return _MaxCurr; }
                    set { _MaxCurr = value; RaisePropertyChanged(); }
                }

                private string _HOC = "0.000";
                /// <summary>
                /// LD硬件过流保护
                /// </summary>
                public string HOC
                {
                    get { return _HOC; }
                    set { _HOC = value; RaisePropertyChanged(); }
                }

                private CalibParam _calibMeasure = new CalibParam();

                public CalibParam CalibMeasure
                {
                    get { return _calibMeasure; }
                    set { _calibMeasure = value; }
                }

                private CalibParam _calibSet = new CalibParam();

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

                private string _temp = "20.000";
                /// <summary>
                /// 温度
                /// </summary>
                public string Temp
                {
                    get { return _temp; }
                    set { _temp = value; RaisePropertyChanged(); }
                }

             

                private string _vol = "0.000";
                /// <summary>
                /// 电压
                /// </summary>
                public string Vol
                {
                    get { return _vol; }
                    set { _vol = value; RaisePropertyChanged(); }
                }
                

                private PIDCalibParam _PID = new PIDCalibParam();

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

                private string _delay = "10";

                public string Delay
                {
                    get { return _delay; }
                    set { _delay = value; RaisePropertyChanged(); }
                }
            }

            public class PulseParam : BindableBase
            {
                private string _width ;

                public string Width
                {
                    get { return _width; }
                    set { _width = value; RaisePropertyChanged(); }
                }

                private string _freq;

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


                private string[] _interval = new string[11];

                public string[] Interval
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

                private string _fanSpeed ;

                public string FanSpeed
                {
                    get { return _fanSpeed; }
                    set { _fanSpeed = value; RaisePropertyChanged(); }
                }

                private string _motorSpeed;

                public string MotorSpeed
                {
                    get { return _motorSpeed; }
                    set { _motorSpeed = value; RaisePropertyChanged(); }
                }

                private string _PwrLimit;

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
            }

        }
   
    
    
    }


    

}
