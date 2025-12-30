
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
//using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using Microsoft.Win32;
using wpfApp.Extensions;
using Prism.Events;
using wpfApp.Common.Events;
using System.IO;
using System.Threading;
using ScottPlot;
using System.Globalization;
using wpfApp.Common;


using wpfApp.Common.Dialog;
using wpfApp.Common.Models;
using Prism.Navigation.Regions;

namespace wpfApp.ViewModels
{
    public partial class HomePageViewModel : BindableBase, INavigationAware
    {
        public DelegateCommand<string> ExecuteCommand { get; private set; }

        public DelegateCommand<object> Relay1StateChangeCommand { get; private set; }
        public DelegateCommand<object> Relay2StateChangeCommand { get; private set; }

        public DelegateCommand<object> ChannelChangeCommand { get; private set; }

        public DelegateCommand<object> PauseLoadInputFileCommand { get; private set; }

        public DelegateCommand<object> HoldingTimeChangeCommand { get; private set; }

        private readonly IContainerExtension container;
        private readonly IEventAggregator aggregator;
        private readonly IDialogHostService dialogHostService;


        public HomePageViewModel(IContainerExtension container, IEventAggregator aggregator, IDialogHostService dialogHostService)
        {

            ExecuteCommand = new DelegateCommand<string>(Execute);

            
            this.container = container;
            this.aggregator = aggregator;
            this.dialogHostService = dialogHostService;
            MaxMaginitude = 20;
            MaginitudeTick = 1;
            BaudRate = 1000;
            WaveLenth = 10;
            ReadConfi();
            CreateMainChart();
        }



        private Wave mainWave = new Wave();
        private Wave secondWave = new Wave();
        private Wave thirdWave = new Wave();
        private NosiyWave noisy = new NosiyWave();

        private double waveLenth;
        private double maxMaginitude;
        private double maginitudeTick;
        private int baudRate;
        private bool isScaleZoom;
        private double scaleZoom;
        private ObservableCollection<double> scaleZooms = new ObservableCollection<double> { 0.1, 0.2, 0.5, 1, 2, 5, 10 };
       
        public Wave MainWave { get => mainWave; set { mainWave = value; RaisePropertyChanged(); } }

        public Wave SecondWave { get => secondWave; set { secondWave = value; RaisePropertyChanged(); } }

        public Wave ThirdWave { get => thirdWave; set { thirdWave = value; RaisePropertyChanged(); } }

        public NosiyWave Noisy { get => noisy; set { noisy = value; RaisePropertyChanged(); } }

        public double WaveLenth { get => waveLenth; set { waveLenth = value; RaisePropertyChanged(); OperateIniFile.WriteIniData("User", "WaveLenth", value.ToString(), @"..\..\..\Confi.ini"); } }

        public double MaxMaginitude { get => maxMaginitude; set { maxMaginitude = value; RaisePropertyChanged(); OperateIniFile.WriteIniData("User", "MaxMaginitude", value.ToString(), @"..\..\..\Confi.ini"); } }
        public double MaginitudeTick { get => maginitudeTick; set { maginitudeTick = value; RaisePropertyChanged(); OperateIniFile.WriteIniData("User", "MaginitudeTick", value.ToString(), @"..\..\..\Confi.ini"); } }

        public int BaudRate { get => baudRate; set { baudRate = value; RaisePropertyChanged(); OperateIniFile.WriteIniData("User", "BaudRate", value.ToString(), @"..\..\..\Confi.ini"); } }

        public bool IsScaleZoom { get => isScaleZoom; set { isScaleZoom = value; RaisePropertyChanged(); OperateIniFile.WriteIniData("User", "IsScaleZoom", value.ToString(), @"..\..\..\Confi.ini"); } }

        public double ScaleZoom { get => scaleZoom; set { scaleZoom = value; RaisePropertyChanged(); OperateIniFile.WriteIniData("User", "ScaleZoom", value.ToString(), @"..\..\..\Confi.ini"); } }


        public ObservableCollection<double> ScaleZooms
        {
            get { return scaleZooms; }
            set { scaleZooms = value; RaisePropertyChanged(); }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {

        }


        private void Execute(string obj)
        {
      
        }




    }



}
