using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Dialogs;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using wpfApp.Common.Dialog;
using wpfApp.Common.Events;
using wpfApp.Common.Models;
using static wpfApp.ViewModels.TabDevViewModel;

namespace wpfApp.ViewModels
{
    public class DataChartViewModel : BindableBase, IDialogAware
    {

        private readonly IContainerExtension containerProvider;
        public readonly IEventAggregator aggregator;
      
        
        public WpfPlot scottplot { get; set; }
        public DataChartViewModel(IContainerExtension containerProvider)
        {
            this.containerProvider = containerProvider;
            aggregator = containerProvider.Resolve<IEventAggregator>();
            scottplot = new WpfPlot();
            ChartIni();
            aggregator.GetEvent<DevUpdateEvent>().Subscribe(x =>
            {
                DevUpdata(x);
            });
            //scottplot.Menu = new CutstomMenu(scottplot, chartXAutoSize, chartY1AutoSize);
        }

        public DialogCloseListener RequestClose { get; set; }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            SigName = parameters.GetValue<string>("SigName");
            if (parameters.ContainsKey("CH"))
            {
                CH = parameters.GetValue<int>("CH");
            }
            if (parameters.ContainsKey("DevName"))
            {
                DevName = parameters.GetValue<string>("DevName");
            }
            if (parameters.ContainsKey("DevMeasureDatas"))
            {
                SigAdd(parameters.GetValue<List<DevMeasureData>>("DevMeasureDatas"));
            }
            else
            {
                SigAdd();
            }
            
        }

        void DevUpdata(DevUpdateModel devUpdateModel)
        {
            if (devUpdateModel.Type == DevUpdateType.MeasureData && devUpdateModel.CH == CH)
            {
                ScottPlot.Plottables.DataLogger dataLogger = sig1 as DataLogger;
                dataLogger.Add(devUpdateModel.devMeasureData.time.ToOADate(), devUpdateModel.devMeasureData.data_list[data_idx]);

                
                ChartRender();
            }
        }
        void ChartRender()
        {
            if (chartXAutoSize)
            {
                scottplot.Plot.Axes.AutoScaleX();
            }
            if (chartY1AutoSize)
            {
                scottplot.Plot.Axes.AutoScaleY(scottplot.Plot.Axes.Left);
            }
            Application.Current.Dispatcher.Invoke(() => scottplot.Refresh());
        }

        private string _devName;

        public string DevName
        {
            get { return _devName; }
            set { _devName = value; RaisePropertyChanged(); }
        }





        private int CH = -1;
        string SigName { get; set; }

        object sig1, sig2;
        int data_idx;
        bool chartXAutoSize = true;
        bool chartY1AutoSize = true;


        Point plotMouseDownLocation;
        DateTime plotMouseDownTime;
        int plotMouseDownMaxHoldTime = 200; // 长按判定时间 (单位: 毫秒)      
        void ChartCustomContextMenu()
        {
            var chartMenu = new ContextMenu();
            chartMenu.FontSize = 14;
            var clearDataMenuItem = new MenuItem() { Header = "清空数据" };
            clearDataMenuItem.Click += (sender, e) =>
            {
                var menuitem = (sender as MenuItem);
                var contextMenu = menuitem.Parent as ContextMenu;
                var scottplot = contextMenu.PlacementTarget as WpfPlot;               
                ScottPlot.Plottables.DataLogger dataLogger = sig1 as DataLogger;
                dataLogger.Clear();
                scottplot.Refresh();
               
            };
            chartMenu.Items.Add(clearDataMenuItem);
            var ScaleXMenuItem = new MenuItem() { Header = "取消自动缩放X轴" };
            ScaleXMenuItem.Click += (sender, e) =>
            {
                var menuitem = (sender as MenuItem);
                var contextMenu = menuitem.Parent as ContextMenu;
                var scottplot = contextMenu.PlacementTarget as WpfPlot;
                if (menuitem.Header.ToString() == "取消自动缩放X轴")
                {
                    menuitem.Header = "自动缩放X轴";
                    chartXAutoSize = false;
                }
                else
                {
                    menuitem.Header = "取消自动缩放X轴";
                    chartXAutoSize = true;
                    scottplot.Plot.Axes.AutoScaleX();
                    scottplot.Refresh();
                }
            };
            chartMenu.Items.Add(ScaleXMenuItem);
            var ScaleY1MenuItem = new MenuItem() { Header = "取消自动缩放Y1轴" };
            ScaleY1MenuItem.Click += (sender, e) =>
            {
                var menuitem = (sender as MenuItem);
                var contextMenu = menuitem.Parent as ContextMenu;
                var scottplot = contextMenu.PlacementTarget as WpfPlot;
                if (menuitem.Header.ToString() == "取消自动缩放Y1轴")
                {
                    menuitem.Header = "自动缩放Y1轴";
                    chartY1AutoSize = false;
                }
                else
                {
                    menuitem.Header = "取消自动缩放Y1轴";
                    chartY1AutoSize = true;
                    scottplot.Plot.Axes.AutoScaleY(scottplot.Plot.Axes.Left);
                    scottplot.Refresh();
                }
            };
            chartMenu.Items.Add(ScaleY1MenuItem);
            chartMenu.Items.Add(new Separator());

            var SaveImageMenuItem = new MenuItem() { Header = "保存为图片" };
            SaveImageMenuItem.Click += (sender, e) =>
            {
                var menuitem = (sender as MenuItem);
                var contextMenu = menuitem.Parent as ContextMenu;
                var scottplot = contextMenu.PlacementTarget as WpfPlot;
                var sfd = new SaveFileDialog
                {
                    FileName = "Plot.png",
                    Filter = "PNG Files (*.png)|*.png;*.png" +
                        "|JPG Files (*.jpg, *.jpeg)|*.jpg;*.jpeg" +
                        "|BMP Files (*.bmp)|*.bmp;*.bmp" +
                        "|WebP Files (*.webp)|*.webp;*.webp" +
                        "|SVG Files (*.svg)|*.svg;*.svg" +
                        "|All files (*.*)|*.*"
                };
                if (sfd.ShowDialog() == true)
                    scottplot.Plot.Save(sfd.FileName, (int)scottplot.ActualWidth, (int)scottplot.ActualHeight);

            };
            chartMenu.Items.Add(SaveImageMenuItem);
            chartMenu.Items.Add(new Separator());


            chartMenu.PlacementTarget = scottplot;

            //chartMenu.Visibility = Visibility.Collapsed;

            scottplot.Menu?.Clear();
            scottplot.ContextMenu = chartMenu;
            scottplot.MouseDown += (s, e) =>
            {
                if (e.ChangedButton == System.Windows.Input.MouseButton.Right)
                {
                    plotMouseDownLocation = e.GetPosition(e.Source as FrameworkElement);
                    plotMouseDownTime = DateTime.Now;
                }
            };
            scottplot.MouseUp += (s, e) =>
            {
                var contextMenu = ((Control)s).ContextMenu;
                if (e.ChangedButton == System.Windows.Input.MouseButton.Right)
                {
                    TimeSpan holdTime = DateTime.Now - plotMouseDownTime;
                    if (holdTime.TotalMilliseconds < plotMouseDownMaxHoldTime)
                    {
                        // 判断鼠标是否未移动（防止拖拽）
                        if (Math.Abs(e.GetPosition(e.Source as FrameworkElement).X - plotMouseDownLocation.X) < 50 &&
                            Math.Abs(e.GetPosition(e.Source as FrameworkElement).Y - plotMouseDownLocation.Y) < 50)
                        {
                            contextMenu.IsOpen = true;
                            contextMenu.Visibility = Visibility.Visible;
                            return;
                        }
                    }
                    contextMenu.Visibility = Visibility.Collapsed;
                }
                contextMenu.IsOpen = false;
            };

        }

        public class SigTheme
        {
            public int id;
            public string name;
            public string description;
            public string color;
            public string label;
        }
        List<SigTheme> SigThemes = new List<SigTheme>();

        void SigChlSelect(int chl_id, int data_id)
        {
            ScottPlot.Plottables.DataLogger dataLogger;
            if (chl_id == 0)
            {
                dataLogger = sig1 as DataLogger;
            }
            else
            {
                dataLogger = sig2 as DataLogger;
            }
            //dataLogger.Color = ScottPlot.Color.FromHtml(SigThemes[data_id].color);
            dataLogger.Axes.YAxis.Label.Text = SigThemes[data_id].label;
            dataLogger.Axes.YAxis.Label.FontName = ScottPlot.Fonts.Detect(SigThemes[data_id].label);
            dataLogger.Axes.YAxis.Label.ForeColor = dataLogger.Color;
            dataLogger.Axes.YAxis.TickLabelStyle.ForeColor = dataLogger.Color;
            dataLogger.Axes.YAxis.FrameLineStyle.Color = dataLogger.Color;
            dataLogger.Axes.YAxis.MajorTickStyle.Color = dataLogger.Color;
            dataLogger.Axes.YAxis.MinorTickStyle.Color = dataLogger.Color;

        }
        void SigChlSelect(ScottPlot.Plottables.DataLogger dataLogger, int data_id)
        {

            dataLogger.Color = ScottPlot.Color.FromHtml(SigThemes[data_id].color);
            dataLogger.Axes.YAxis.Label.Text = SigThemes[data_id].label;
            dataLogger.Axes.YAxis.Label.FontName = ScottPlot.Fonts.Detect(SigThemes[data_id].label);
            dataLogger.Axes.YAxis.Label.ForeColor = dataLogger.Color;
            dataLogger.Axes.YAxis.TickLabelStyle.ForeColor = dataLogger.Color;
            dataLogger.Axes.YAxis.FrameLineStyle.Color = dataLogger.Color;
            dataLogger.Axes.YAxis.MajorTickStyle.Color = dataLogger.Color;
            dataLogger.Axes.YAxis.MinorTickStyle.Color = dataLogger.Color;
        }
        int get_sig_data_idx(ScottPlot.Plottables.DataLogger dataLogger)
        {
            for (int i = 0; i < SigThemes.Count; i++)
            {
                if (dataLogger.Axes.YAxis.Label.Text == SigThemes[i].label)
                {
                    return i;
                }
            }
            return 0;
        }


        void ChartIni()
        {
            scottplot = new WpfPlot();
           
            ///设置透明背景
            PlotStyle plotStyle = new PlotStyle();
            plotStyle.DataBackgroundColor = Color.FromColor(System.Drawing.Color.Transparent);
            plotStyle.FigureBackgroundColor = Color.FromColor(System.Drawing.Color.Transparent);
            scottplot.Plot.SetStyle(plotStyle);

            ///禁止DPI缩放 以获得更高的清晰度  
            //scottplot.Configuration.DpiStretch = false;

            //设置横坐标为时间轴
            scottplot.Plot.Axes.DateTimeTicksBottom();

            scottplot.Plot.ScaleFactor = 1;
            scottplot.Plot.Axes.Bottom.TickLabelStyle.FontSize = 16;
            scottplot.Plot.Axes.Left.TickLabelStyle.FontSize = 16;
            scottplot.Plot.Axes.Right.TickLabelStyle.FontSize = 16;

            //添加曲线数据
            sig1 = scottplot.Plot.Add.DataLogger();
            //设置坐标系
            ScottPlot.Plottables.DataLogger char_sig1 = sig1 as DataLogger;
            //char_sig1.Color = ScottPlot.Color.FromColor(System.Drawing.Color.Red);
            char_sig1.Axes.YAxis = scottplot.Plot.Axes.Left;
            char_sig1.ManageAxisLimits = false;
            SigThemes.Add(new SigTheme { id = 0, color = "#c76437", label = "LD温度/℃", name = "LD温度" });
            SigThemes.Add(new SigTheme { id = 1, color = "#8463cb", label = "LD出光功率/mW", name = "LD出光功率" });
            SigThemes.Add(new SigTheme { id = 2, color = "#77b341", label = "LD电流/mA", name = "LD电流" });
            SigThemes.Add(new SigTheme { id = 3, color = "#c65eae", label = "PIN背光电流/uA", name = "PIN背光电流" });
            SigThemes.Add(new SigTheme { id = 4, color = "#51ad82", label = "LD电压/mV", name = "LD电压" });
            SigThemes.Add(new SigTheme { id = 5, color = "#d4405b", label = "NCT阻值/Ω", name = "NTC阻值" });
            SigThemes.Add(new SigTheme { id = 6, color = "#688dcd", label = "TEC电流/mA", name = "TEC电流" });
            SigThemes.Add(new SigTheme { id = 7, color = "#cda148", label = "R-温度/℃", name = "R-温度" });
            SigThemes.Add(new SigTheme { id = 8, color = "#c0697e", label = "AHT20湿度/%", name = "AHT20湿度" });
            SigThemes.Add(new SigTheme { id = 9, color = "#787a34", label = "AHT20温度/℃", name = "AHT20温度" });

            //SigChlSelect(0, 0);
            //SigChlSelect(1, 2);
            ChartCustomContextMenu();

        }

        void SigAdd()
        {
            ScottPlot.Plottables.DataLogger char_sig1 = sig1 as DataLogger;
            for (int i = 0; i < SigThemes.Count; i++)
            {
                if (SigThemes[i].name == this.SigName)
                {
                    SigChlSelect(char_sig1, i);
                }
            }
            data_idx = get_sig_data_idx(char_sig1);
        }

        void SigAdd(List<DevMeasureData> devMeasureDatas)
        {
            ScottPlot.Plottables.DataLogger char_sig1 = sig1 as DataLogger;
            for (int i = 0; i < SigThemes.Count; i++)
            {
                if (SigThemes[i].name == this.SigName)
                {
                    SigChlSelect(char_sig1, i);
                }
            }
            data_idx = get_sig_data_idx(char_sig1);
            for (int i = 0; i < devMeasureDatas.Count; i++)
            {
                char_sig1.Add(devMeasureDatas[i].time.ToOADate(), devMeasureDatas[i].data_list[data_idx]);
            }
        }


        class CutstomMenu(WpfPlot scottplot, bool chartXAutoSize, bool chartY1AutoSize) : IPlotMenu
        {
            public void Add(string Label, Action<Plot> action)
            {
                throw new NotImplementedException();
            }

            public void AddSeparator()
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            public void ShowContextMenu(Pixel pixel)
            {
                var chartMenu = new ContextMenu();
                chartMenu.FontSize = 14;
                var ScaleXMenuItem = new MenuItem() { Header = (chartXAutoSize ? "取消自动缩放X轴" : "自动缩放X轴") };
                ScaleXMenuItem.Click += (sender, e) =>
                {
                    var menuitem = (sender as MenuItem);
                    var contextMenu = menuitem.Parent as ContextMenu;
                    var scottplot = contextMenu.PlacementTarget as WpfPlot;

                    if (menuitem.Header.ToString() == "取消自动缩放X轴")
                    {
                        menuitem.Header = "自动缩放X轴";
                        //chartXAutoSize = false;
                    }
                    else
                    {
                        menuitem.Header = "取消自动缩放X轴";
                        //chartXAutoSize = true;
                        scottplot.Plot.Axes.AutoScaleX();
                        scottplot.Refresh();
                    }
                };
                chartMenu.Items.Add(ScaleXMenuItem);
                //chartMenu.ShowDialog(scottplot);
                chartMenu.PlacementTarget = scottplot;
                chartMenu.IsOpen = true;
            }
        }

    }
}
