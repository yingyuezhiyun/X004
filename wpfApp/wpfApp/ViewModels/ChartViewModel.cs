using Microsoft.Win32;
using wpf_app.Common.DataHandlers.Interface;
using wpf_app.Common.Events;
using wpf_app.Common.Models;
using wpf_app.Common.Models.Interfaces;
using wpf_app.Extensions;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using ScottPlot;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ScottPlot.WPF;
using Color = ScottPlot.Color;

namespace wpf_app.ViewModels
{
    public class ChartViewModel : NavigationViewModel, ITest
    {
        public string _sourcenamespace { get; set; }
        private readonly IDataHandler dataHandler;
        public DelegateCommand<string> ExecuteCommand { get; private set; }

       
        public ChartViewModel(IContainerProvider provider) : base(provider)
        {

          
            InputConfi = provider.Resolve<IInputInfo>();
            dataHandler = provider.Resolve<IDataHandler>();
            scottplot = new WpfPlot();
     
  

    
            scottplot.Refresh();

            MaxSignalCount = 5_400_000; //5400秒
            ExecuteCommand = new DelegateCommand<string>(Execute);

            aggregator.GetEvent<ChannelChangeEvent>().Subscribe(a =>
            {
                
            });

            aggregator.GetEvent<UpdataEvent>().Subscribe(a =>
            {
                if (a.Filter == _sourcenamespace)
                {
                    AddData(a.Data);
                }
            });

            RefreshTimerInit();

        
            SendToFFTDelayTimerInit();
        }




        /// <summary>
        /// 当导航到该页面时
        /// </summary>
        /// <param name="navigationContext"></param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            if (navigationContext.Parameters.ContainsKey("NameSpace"))
            {
                //取出传过来的值
                _sourcenamespace = navigationContext.Parameters.GetValue<string>("NameSpace");
            }

            if (navigationContext.Parameters.ContainsKey("Title"))
            {

                Title = navigationContext.Parameters.GetValue<string>("Title");
            }

            if (navigationContext.Parameters.ContainsKey("StopAutoRefresh"))
            {
                if (navigationContext.Parameters.GetValue<bool>("StopAutoRefresh"))
                {
                    stopAutoRefresh = true;
                    refreshTimer.Stop();
                }
            }
            if (navigationContext.Parameters.ContainsKey("HideAutoRefresh"))
            {
                if (navigationContext.Parameters.GetValue<bool>("HideAutoRefresh"))
                {
                    AutoRefreshVisibility = Visibility.Collapsed;
                }
            }

        }




        private IInputInfo inputConfi;
        private ObservableCollection<double> autoRefreshIntervals = new ObservableCollection<double> { 1, 2, 5, 10, 15, 20, 30 };
        private ObservableCollection<double> scaleZooms = new ObservableCollection<double> { 0.1, 0.2, 0.5, 1, 2, 5, 10 };
        private bool stopAutoRefresh = false;
        private Visibility autoRefreshVisibility = Visibility.Visible;
        private string title;



        /// <summary>
        /// 输入配置文件的一些参数
        /// </summary>
        public IInputInfo InputConfi { get => inputConfi; set { inputConfi = value; RaisePropertyChanged(); } }
        /// <summary>
        /// 波形刷新周期
        /// </summary>
        public ObservableCollection<double> AutoRefreshIntervals { get => autoRefreshIntervals; set { autoRefreshIntervals = value; RaisePropertyChanged(); } }
        /// <summary>
        /// 显示 区间时长
        /// </summary>
        public ObservableCollection<double> ScaleZooms
        {
            get { return scaleZooms; }
            set { scaleZooms = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 自动刷新可见属性
        /// </summary>
        public Visibility AutoRefreshVisibility { get => autoRefreshVisibility; set { autoRefreshVisibility = value; RaisePropertyChanged(); } }

        public string Title { get => title; set { title = value; RaisePropertyChanged(); } }

        /// <summary>
        /// 时间轴缓存
        /// </summary>
        public List<double> timeAxisTempBuffer { get; set; } = new List<double> { };
  
        /// <summary>
        /// 最大的数据点，超出部分从index 向左溢出
        /// </summary>
        public int? MaxSignalCount { get; set; } = null;
        public WpfPlot scottplot { get; set; }
        /// <summary>
        /// 经过转换后的串口实时数据（或读取文件时得到的数据），还未添加到波形显示里
        /// </summary>
        public List<DetData> DataBuffer = new List<DetData>();


  


        /// <summary>
        /// 添加数据 等待转换成波形图
        /// </summary>
        /// <param name="data"></param>
        public async void AddData(List<DetData> data)
        {
            await Task.Run(() =>
            {
                var convertedData = dataHandler.StainToValidStress(data);
                lock (DataBuffer)
                {
                    DataBuffer.AddRange(convertedData);
                    var AddCount = DataBuffer.Count;
                    if (AddCount > (MaxSignalCount ?? AddCount))
                    {
                        DataBuffer.RemoveRange(0, (int)(AddCount - MaxSignalCount));
                    }
                }
                if (stopAutoRefresh)
                {
                    DataToPlot();
                }
            });
        }


        /// <summary>
        /// 一些命令
        /// </summary>
        /// <param name="obj"></param>
        private void Execute(string obj)
        {
            switch (obj)
            {

                case "定时刷新":/* AutoRefresh();*/ break;
                case "刷新区间改变": refreshTimer.Interval = InputConfi.AutoRefreshInterval * 1000;   /*checkTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)InputConfi.AutoRefreshInterval * 1000);*/ break;


                case "开启区间显示":
                case "区间显示改变": /*ZoomAndVisibility(); RenderGraph();*/ break;
              

                case "截图":/* scottplot.ScreenShot();*/ break;
                case "清空波形":  break;
                case "添加X轴辅助线": /*scottplot.AddAxisLine(HorizontalLine: true, VerticalLine: false); RenderGraph();*/ break;
                case "添加Y轴辅助线":/* scottplot.AddAxisLine(HorizontalLine: false, VerticalLine: true); RenderGraph();*/ break;
                case "删除辅助线": /*scottplot.ReMoveAxisLine(); RenderGraph();*/ break;


                default: break;
            }
        }

        /// <summary>
        /// 清除波形
    


    


        public void DataToPlot()
        {
            lock (DataBuffer)
            {
                if (DataBuffer.Count == 0)
                {
                    return;
                }

                var dataBufferCount = DataBuffer.Count;


                DataBuffer.Clear();
            }


       
        }



        #region 定时刷新

        public void AutoRefresh()
        {
            if (InputConfi.ChartAutoRefresh)
            {
                //checkTimer.Start();
                refreshTimer.Start();
            }
            else
            {
                //checkTimer.Stop();
                refreshTimer.Stop();
            }

        }


        System.Timers.Timer refreshTimer;


        private void RefreshTimerInit()
        {
            //设置定时间隔(毫秒为单位)
            int interval = 1000;
            refreshTimer = new System.Timers.Timer(interval);
            //设置执行一次（false）还是一直执行(true)
            refreshTimer.AutoReset = true;
            //设置是否执行System.Timers.Timer.Elapsed事件
            //refreshTimer.Enabled = true;
            //绑定Elapsed事件
            refreshTimer.Elapsed += RefreshTimer_Elapsed;
            //if (InputConfi.ChartAutoRefresh)
            //{
            refreshTimer.Start();
            //}
        }

        private async void RefreshTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            refreshTimer.Stop();
            await Task.Run(new Action(() => DataToPlot()));
            refreshTimer.Start();

        }


        #endregion

        #region 发送给FFT数据

        System.Timers.Timer sendToFFTDelayTimer;

        private void SendToFFTDelayTimerInit()
        {
            //设置定时间隔(毫秒为单位)
            int interval = 500;
            sendToFFTDelayTimer = new System.Timers.Timer(interval);
            //设置执行一次（false）还是一直执行(true)
            sendToFFTDelayTimer.AutoReset = false;
            //绑定Elapsed事件
            sendToFFTDelayTimer.Elapsed += SendToFFTDelayTimer_Elapsed;
        }

        private void SendToFFTDelayTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
          
            List<List<double>> FFTdata = new List<List<double>>();
   

            aggregator.GetEvent<FFTUpDataEvent>().Publish(new FFTRawData { Filter = _sourcenamespace, Data = FFTdata });
        }

        private void ScaleChanged(object? sender, EventArgs e)
        {
            sendToFFTDelayTimer.Stop();

            sendToFFTDelayTimer.Start();
        }

        #endregion


    }

    public interface ITest
    {
        public string _sourcenamespace { get; set; }
    }




}
