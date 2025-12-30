using MathNet.Numerics.LinearAlgebra.Factorization;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Dialogs;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Windows.Networking;
using wpfApp.Common.CtrlProtocol;
using wpfApp.Common.Dialog;
using wpfApp.Common.Events;
using wpfApp.Extensions;

namespace wpfApp.ViewModels
{
    public partial class TabDevViewModel : NavigationViewModel
    {
        private readonly IRegionManager regionManager;
        private readonly IContainerExtension container;
        private readonly IDialogHostService dialogHostService;


        private string NameSpace = string.Empty;
        private int CH = -1;
        private DevType devType = DevType.X002_002;
        public WpfPlot scottplot { get; set; }
        private ProtocolManager protocolManager = new ProtocolManager();
        public DelegateCommand<string> ExecuteCommand { get; private set; }
     
        public TabDevViewModel(IRegionManager regionManager, IContainerExtension container, IDialogHostService dialogHostService) : base(container)
        {
            this.container = container;
            this.regionManager = regionManager;
            this.dialogHostService = dialogHostService;

            ExecuteCommand = new DelegateCommand<string>(Execute);

            ChartIni();
            
            BitTimerInit();
            QueryTimerInit();
            ConnectTimerInit();
            DeviceSerialPort.DataReceived += serialPortDataReceived;
            DevPortName = PortName.First();
            Thread thread = new Thread(new ThreadStart(() => { this.dataHandler(); }));
            thread.Start();

            DevParaShow = new DevParaShow();
            
            DevParaSetting = new ParaToSetting();
            
            IsAutoRunList = new List<string> { "是","否"};

            //TODO 根据类型区分显示内容
            //DevParaShow.MeasurePara = new List<DevPara>() { DevParaShow.M.LDTemp, DevParaShow.M.LDCurr, DevParaShow.M.LDVol, DevParaShow.M.LDPower, DevParaShow.M.TECCurr, DevParaShow.M.PINCurr };
            //ModeList = new List<string> { "恒电流", "恒功率", "外调制" };
            //MeasureDataChartList = new List<string>() { "LD温度", "LD电流", "LD电压", "LD出光功率", "PIN背光电流", "TEC电流", "NTC阻值" };
            //MeasureDataChartSelected = MeasureDataChartList[0];
            //protocolManager.SetProtocol(new Protocol_X002_001());
            SetDevType(DevType.X002_002);

            FileSavePath = @".\";//GetCurrentFilePath();

            aggregator.GetEvent<DevManageEvent>().Subscribe( e => { DevManageUpdata(e); });

            
        }


        void DevManageUpdata(DevManageUpdataModel devManageUpdataModel)
        {
            if (devManageUpdataModel.Type == DevManageUpdataType.Updata && devManageUpdataModel.CH == CH
                && !DeviceSerialPort.IsOpen && devManageUpdataModel.IsOpen)
            {
                SetDevType(devManageUpdataModel.devType);
                DevPortName = devManageUpdataModel.DevPort;
                DeviceConnect();
            }
            else if (devManageUpdataModel.Type == DevManageUpdataType.ClearErr && DevParaShow.StatusInfo.BITStatus.Vaule != "正常")
            {
                if (DeviceSerialPort.IsOpen == false)
                {
                    try
                    {
                        DeviceSerialPort.Open();
                        DevClearErr();
                        DeviceSerialPort.Close();
                        DeviceConnect();
                    }
                    catch (Exception)
                    {

                    }

                }
                else
                {
                    DevClearErr();
                }
            }

        }

        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {

            if (navigationContext.Parameters.ContainsKey("NameSpace"))
            {
                //取出传过来的值
                if (NameSpace == navigationContext.Parameters.GetValue<string>("NameSpace"))
                {
                    return true;
                }

            }
            return false;
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.ContainsKey("NameSpace"))
            {
                //取出传过来的值
                NameSpace = navigationContext.Parameters.GetValue<string>("NameSpace");
            }
            if (navigationContext.Parameters.ContainsKey("CH"))
            {
                CH = navigationContext.Parameters.GetValue<int>("CH");
            }
        }
        public TabDevViewModel()
        {

        }
        ~TabDevViewModel()
        {

        }

        #region param

        private DevParaShow _devStatus;

        public DevParaShow DevParaShow
        {
            get { return _devStatus; }
            set { _devStatus = value; RaisePropertyChanged(); }
        }

        private ParaToSetting _devParaSetting;

        public ParaToSetting DevParaSetting
        {
            get { return _devParaSetting; }
            set { _devParaSetting = value; RaisePropertyChanged(); }
        }



        private List<string> portName;
        public List<string> PortName
        {
            get => SerialPort.GetPortNames().ToList();
            set { portName = value; RaisePropertyChanged(); }
        }

        private Visibility _powerFuncVisibility = Visibility.Collapsed;

        public Visibility PowerFuncVisibility
        {
            get { return _powerFuncVisibility; }
            set { _powerFuncVisibility = value; RaisePropertyChanged(); }
        }

        private List<string> _modeList = new List<string>();
        public List<string> ModeList
        {
            get { return _modeList; }
            set { _modeList = value; RaisePropertyChanged(); }
        }

        private List<string> _isAutoRunList;
        public List<string> IsAutoRunList
        {
            get { return _isAutoRunList; }
            set { _isAutoRunList = value; RaisePropertyChanged(); }
        }

        private List<string> _measureDataChartList = new List<string>();
        public List<string> MeasureDataChartList
        {
            get { return _measureDataChartList; }
            set { _measureDataChartList = value; RaisePropertyChanged(); }
        }

        private string _measureDataChartSelected;

        public string MeasureDataChartSelected
        {
            get { return _measureDataChartSelected; }
            set { _measureDataChartSelected = value; RaisePropertyChanged(); }
        }



        private bool _deviceIsOpen;

        public bool DeviceIsOpen
        {
            get => DeviceSerialPort.IsOpen;
            set { _deviceIsOpen = value; RaisePropertyChanged(); }
        }


        private bool _ldIsWork = false;

        public bool LDIsWork
        {
            get { return _ldIsWork; }
            set { _ldIsWork = value; RaisePropertyChanged(); }
        }

        private bool _tecIsWork = false;

        public bool TECIsWork
        {
            get { return _tecIsWork; }
            set { _tecIsWork = value; RaisePropertyChanged(); }
        }
        
        private bool _isUpdatePara = false;

        public bool IsUpdatePara
        {
            get { return _isUpdatePara; }
            set { _isUpdatePara = value; RaisePropertyChanged(); }
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

        private string _fileSavePath;

        public string FileSavePath
        {
            get { return _fileSavePath; }
            set { _fileSavePath = value; RaisePropertyChanged(); }
        }

        private bool _isFileSave = true;

        public bool IsFileSave
        {
            get { return _isFileSave; }
            set { _isFileSave = value; RaisePropertyChanged(); }
        }

        private string _devPortName;

        public string DevPortName
        {
            get { return _devPortName; }
            set { _devPortName = value; RaisePropertyChanged(); }
        }

        #endregion



        void SetDevType(DevType devType)
        {
            ModeList.Clear();
            MeasureDataChartList.Clear();
            DevParaShow.MeasurePara.Clear();
            
            switch (devType)
            {
                case DevType.X002_001:
                    protocolManager.SetProtocol(new Protocol_X002_001());
                    DevParaShow.MeasurePara.AddRange(new List<DevPara>() { DevParaShow.M.LDTemp, DevParaShow.M.LDCurr, DevParaShow.M.LDVol, DevParaShow.M.LDPower,
                        DevParaShow.M.TECCurr, DevParaShow.M.PINCurr });
                    ModeList.AddRange( new List<string> { "恒电流", "恒功率", "外调制" });
                    MeasureDataChartList.AddRange(new List<string>() { "LD温度", "LD电流", "LD电压", 
                        "LD出光功率", "PIN背光电流", "TEC电流", "NTC阻值" });
                   
                    PowerFuncVisibility = Visibility.Visible;
                    queryTimer.Interval = 500;
                    break;
                case DevType.X002_002:
                    protocolManager.SetProtocol(new Protocol_X002_002());
                    DevParaShow.MeasurePara.AddRange(new List<DevPara>() { DevParaShow.M.LDTemp, DevParaShow.M.LDCurr, DevParaShow.M.LDVol, DevParaShow.M.LDPower,
                        DevParaShow.M.TECCurr, DevParaShow.M.PINCurr, DevParaShow.M.BoardTemp, DevParaShow.M.AHT20Temp, DevParaShow.M.AHT20Humidity });
                    ModeList.AddRange(new List<string> { "恒电流",  "外调制" });
                    MeasureDataChartList.AddRange(new List<string>() { "LD温度", "LD电流", "LD电压", "LD出光功率", 
                        "PIN背光电流", "TEC电流", "NTC阻值", "R-温度","AHT20温度","AHT20湿度"});
                    PowerFuncVisibility = Visibility.Collapsed;
                    queryTimer.Interval = 200;
                    break;
                default:
                    break;
            }
            this.devType = devType;
            DevParaSetting.Mode = ModeList[0];
            MeasureDataChartSelected = MeasureDataChartList[0];
            SigThemeCreate(devType);
            ChartCustomContextMenu();
        }





        private string GetCurrentFilePath()
        {
            // 获取当前执行的程序集
            var assembly = Assembly.GetExecutingAssembly();
            // 获取程序集的位置
            string path = assembly.Location;
            return path;
        }

        string getDataFilePath()
        {

            DateTime now = DateTime.Now;

            // 创建以年月命名的目录
            string directoryPath = Path.Combine(FileSavePath, now.ToString("yyyy-MM"));
            // 创建以年月日命名的文件
            string fileName = NameSpace+now.ToString("-yyyy-MM-dd") + ".csv";
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
                        if (devType == DevType.X002_001)
                        {
                            streamWriter.WriteLine("时间,LD电流(mA),LD电压(mV),PIN背光电流(uA),LD出光功率(mW),NTC值(Ω),LD温度(℃),TEC电流(mA)");
                        }
                        else if (devType == DevType.X002_002)
                        {
                            streamWriter.WriteLine("时间,LD电流(mA),LD电压(mV),PIN背光电流(uA),LD出光功率(mW),NTC值(Ω),LD温度(℃),TEC电流(mA),R-温度(℃),AHT20湿度(%),AHT20温度(℃)");
                        }
                        
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
        void saveDataFile(DevMeasureData data)
        {
            string filePath = getDataFilePath();
            if (filePath == null)
                return;
            using (StreamWriter streamWriter = new StreamWriter(filePath, append: true, encoding: Encoding.UTF8))
            {
                try
                {
                    if (devType == DevType.X002_001)
                    {
                        streamWriter.WriteLine("{0:yyyy年MM月dd日 HH:mm:ss.fff},{1:F4},{2:F4},{3:F4},{4:F4},{5:F1},{6:F4},{7:F1}",
                        data.time, data.LD_curr, data.LD_vol, data.PIN_curr, data.power, data.NTC_vaule, data.temperature, data.TEC_curr);
                    }
                    else if (devType == DevType.X002_002)
                    {
                        streamWriter.WriteLine("{0:yyyy年MM月dd日 HH:mm:ss.fff},{1:F4},{2:F4},{3:F4},{4:F4},{5:F1},{6:F4},{7:F1},{8:F2},{9:F2},{10:F2}",
                        data.time, data.LD_curr, data.LD_vol, data.PIN_curr, data.power, data.NTC_vaule, data.temperature, data.TEC_curr, data.Board_temp, data.AHT20_humidity, data.AHT20_temp);
                    }
                        
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
        public void GoToSurce()
        {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = "explorer.exe";
            proc.StartInfo.Arguments = FileSavePath;
            proc.Start();
        }

  
        void ShowDataChart()
        {
            DialogParameters param = new DialogParameters();
            param.Add(key: "SigName", MeasureDataChartSelected);
            param.Add("CH", CH);
            param.Add("DevName", NameSpace);
            param.Add("DevMeasureDatas", devMeasureDatas);

            dialogHostService.Show("DataChartView2", param,null);
        }

        private void Execute(string obj)
        {
            switch (obj)
            {
                case "连接设备": DeviceConnect(); break;
                case "扫描端口": UpdataSerialPort(); break;
                case "设置模式": DevSetMode(); break;
                case "设置温度": DevSetParamFloatString(DevParamsToSet.LDTemp, DevParaSetting.LDTemp);break;
                case "设置电流": DevSetParamFloatString(DevParamsToSet.LDCurr, DevParaSetting.LDCurr); break;
                case "设置功率": DevSetParamFloatString(DevParamsToSet.LDPower, DevParaSetting.LDPower); break;
                case "TEC启动":  DevSetTECLaunch(); break;
                case "LD启动":   DevSetLDLaunch(); break;
                case "设置LD最大电流": DevSetParamFloatString(DevParamsToSet.LDMaxCurr, DevParaSetting.LDMaxCurr); break;
                case "设置LD过流保护": DevSetParamFloatString(DevParamsToSet.LDHOC, DevParaSetting.LDHOC); break;
                case "设置LD测量电阻": DevSetParamFloatString(DevParamsToSet.LDMeasureR, DevParaSetting.LDMeasureR); break;
                case "设置TEC最低温度": DevSetParamFloatString(DevParamsToSet.TECMinTemp, DevParaSetting.TECMinTemp); break;
                case "设置TEC最高温度": DevSetParamFloatString(DevParamsToSet.TECMaxTemp, DevParaSetting.TECMaxTemp); break;
                case "设置TEC温度偏置": DevSetParamFloatString(DevParamsToSet.TECTempOffset, DevParaSetting.TECTempOffset); break;
                case "设置TEC最大电压": DevSetParamFloatString(DevParamsToSet.TECMaxVol, DevParaSetting.TECMaxVol); break;
                case "设置开机启动": DevSetAutoRun(); break;

                case "参数更新": DevParamUpdata(); break;
                case "参数固化": DevSetParam(DevParamsToSet.Save_Para); break;
                case "打开数据曲线": ShowDataChart(); break;
                case "设置目录": FolderPiker(); break;
                case "打开目录": GoToSurce(); break;
                default: break;
            }
        }

    }

    public class ParaToSetting : BindableBase
    {

        private string _mode = "恒电流";
        /// <summary>
        /// 工作模式
        /// </summary>
        public string Mode
        {
            get { return _mode; }
            set { _mode = value; RaisePropertyChanged(); }
        }
        private string _ldTemp = "20.000";
        /// <summary>
        /// LD温度
        /// </summary>
        public string LDTemp
        {
            get { return _ldTemp; }
            set { _ldTemp = value; RaisePropertyChanged(); }
        }

        private string _ldCurr = "0.000";
        /// <summary>
        /// LD电流
        /// </summary>
        public string LDCurr
        {
            get { return _ldCurr; }
            set { _ldCurr = value; RaisePropertyChanged(); }
        }

        private string _ldPower = "0.000";
        /// <summary>
        /// LD功率
        /// </summary>
        public string LDPower
        {
            get { return _ldPower; }
            set { _ldPower = value; RaisePropertyChanged(); }
        }

        private string _ldMaxCurr = "0.000";
        /// <summary>
        /// LD最大电流
        /// </summary>
        public string LDMaxCurr
        {
            get { return _ldMaxCurr; }
            set { _ldMaxCurr = value; RaisePropertyChanged(); }
        }

        private string _ldHOC = "0.000";
        /// <summary>
        /// LD硬件过流保护
        /// </summary>
        public string LDHOC
        {
            get { return _ldHOC; }
            set { _ldHOC = value; RaisePropertyChanged(); }
        }

        private string _LDMeasureR = "0.000";
        /// <summary>
        /// LD测量电阻
        /// </summary>
        public string LDMeasureR
        {
            get { return _LDMeasureR; }
            set { _LDMeasureR = value; RaisePropertyChanged(); }
        }

        private string _tecMinTemp = "0.000";
        /// <summary>
        /// TEC最低温度
        /// </summary>
        public string TECMinTemp
        {
            get { return _tecMinTemp; }
            set { _tecMinTemp = value; RaisePropertyChanged(); }
        }

        private string _tecMaxTemp = "0.000";
        /// <summary>
        /// TEC最高温度
        /// </summary>
        public string TECMaxTemp
        {
            get { return _tecMaxTemp; }
            set { _tecMaxTemp = value; RaisePropertyChanged(); }
        }

        private string _tecTempOffset = "0.000";
        /// <summary>
        /// TEC温度偏置
        /// </summary>
        public string TECTempOffset
        {
            get { return _tecTempOffset; }
            set { _tecTempOffset = value; RaisePropertyChanged(); }
        }

        private string _tecMaxVol = "0.000";
        /// <summary>
        /// TEC最大电压
        /// </summary>
        public string TECMaxVol
        {
            get { return _tecMaxVol; }
            set { _tecMaxVol = value; RaisePropertyChanged(); }
        }

        private string _isAutoRun = "是";
        /// <summary>
        /// 上电启动
        /// </summary>
        public string IsAutoRun
        {
            get { return _isAutoRun; }
            set { _isAutoRun = value; RaisePropertyChanged(); }
        }

    }

}
