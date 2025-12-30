using Microsoft.Win32;
using Prism.Dialogs;
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
using wpfApp.Common.CtrlProtocol;

namespace wpfApp.ViewModels
{
    public partial class TabDevViewModel
    {
        object sig1, sig2;
        public class SigTheme
        {
            public int id;
            public string name;
            public string description;
            public string color;
            public string label;
        }
        List<SigTheme> SigThemes = new List<SigTheme>();

        bool chartXAutoSize = true;
        bool chartY1AutoSize = true;
        bool chartY2AutoSize = true;

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

            //dataLogger.Color = ScottPlot.Color.FromHtml(sig_Theme_s[data_id].color);
            dataLogger.Axes.YAxis.Label.Text = SigThemes[data_id].label;
            dataLogger.Axes.YAxis.Label.FontName = ScottPlot.Fonts.Detect(SigThemes[data_id].label);
            dataLogger.Axes.YAxis.Label.ForeColor = dataLogger.Color;
            dataLogger.Axes.YAxis.TickLabelStyle.ForeColor = dataLogger.Color;
            dataLogger.Axes.YAxis.FrameLineStyle.Color = dataLogger.Color;
            dataLogger.Axes.YAxis.MajorTickStyle.Color = dataLogger.Color;
            dataLogger.Axes.YAxis.MinorTickStyle.Color = dataLogger.Color;
        }


        void ChartIni()
        {
            scottplot = new WpfPlot();
            scottplot.Plot.ScaleFactor = 1;
           
            ///设置透明背景
            PlotStyle plotStyle = new PlotStyle();
            plotStyle.DataBackgroundColor = Color.FromColor(System.Drawing.Color.Transparent);
            plotStyle.FigureBackgroundColor = Color.FromColor(System.Drawing.Color.Transparent);
            scottplot.Plot.SetStyle(plotStyle);

            ///禁止DPI缩放 以获得更高的清晰度  
            //scottplot.Configuration.DpiStretch = false;

            //设置横坐标为时间轴
            scottplot.Plot.Axes.DateTimeTicksBottom();
           
            
            scottplot.Plot.Axes.Bottom.TickLabelStyle.FontSize = 16;
            scottplot.Plot.Axes.Left.TickLabelStyle.FontSize = 16;
            scottplot.Plot.Axes.Right.TickLabelStyle.FontSize= 16;
            //添加曲线数据
            sig1 = scottplot.Plot.Add.DataLogger();
            sig2 = scottplot.Plot.Add.DataLogger();

            //设置坐标系
            ScottPlot.Plottables.DataLogger char_sig1 = sig1 as DataLogger;
            //char_sig1.Color = ScottPlot.Color.FromColor(System.Drawing.Color.Red);
            char_sig1.Axes.YAxis = scottplot.Plot.Axes.Left;
            char_sig1.ManageAxisLimits = false;


            //设置坐标系
            ScottPlot.Plottables.DataLogger char_sig2 = sig2 as DataLogger;
            //char_sig2.Color = ScottPlot.Color.FromColor(System.Drawing.Color.Red);
            char_sig2.Axes.YAxis = scottplot.Plot.Axes.Right;
            char_sig2.ManageAxisLimits = false;
            SigThemeCreate(DevType.X002_001);
            SigChlSelect(0, 0);
            SigChlSelect(1, 2);
            ChartCustomContextMenu();
        
        }


        void SigThemeCreate(DevType devType)
        {
            SigThemes.Clear();
            SigThemes.Add(new SigTheme { id = 0, color = "#c76437", label = "LD温度/℃", name = "LD温度" });
            SigThemes.Add(new SigTheme { id = 1, color = "#8463cb", label = "LD出光功率/mW", name = "LD出光功率" });
            SigThemes.Add(new SigTheme { id = 2, color = "#77b341", label = "LD电流/mA", name = "LD电流" });
            SigThemes.Add(new SigTheme { id = 3, color = "#c65eae", label = "PIN背光电流/uA", name = "PIN背光电流" });
            SigThemes.Add(new SigTheme { id = 4, color = "#51ad82", label = "LD电压/mV", name = "LD电压" });
            SigThemes.Add(new SigTheme { id = 5, color = "#d4405b", label = "NCT阻值/Ω", name = "NTC阻值" });
            SigThemes.Add(new SigTheme { id = 6, color = "#688dcd", label = "TEC电流/mA", name = "TEC电流" });
           
            switch (devType)
            {
                case DevType.X002_001:
                    break;
                case DevType.X002_002:
                    SigThemes.Add(new SigTheme { id = 7, color = "#cda148", label = "R-温度/℃", name = "R-温度" });
                    SigThemes.Add(new SigTheme { id = 8, color = "#c0697e", label = "AHT20湿度/%", name = "AHT20湿度" });
                    SigThemes.Add(new SigTheme { id = 9, color = "#787a34", label = "AHT20温度/℃", name = "AHT20温度" });
                    break;
                default:
                    break;
            }
            
        }

        void ChartCustomContextMenu2()
        {
            scottplot.Menu?.Clear();           
            scottplot.Menu?.Add("清空数据", (plot) =>
            {
                ScottPlot.Plottables.DataLogger dataLogger = sig1 as DataLogger;
                dataLogger.Clear();
                dataLogger = sig2 as DataLogger;
                dataLogger.Clear();               
                plot.PlotControl?.Refresh();

                //if (FormIsOpen(typeof(datachart)))
                //{
                //    ChartCleanEvent();
                //}
            });
            scottplot.Menu?.Add("取消自动缩放X轴", (plot) =>
            {
                
                plot.Axes.AutoScaleX();
                plot.PlotControl?.Refresh();
            });

            scottplot.Menu?.AddSeparator();

            var tt=  scottplot.ContextMenu;
        }

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
                devMeasureDatas.Clear();    
                ScottPlot.Plottables.DataLogger dataLogger = sig1 as DataLogger;
                dataLogger.Clear();
                dataLogger = sig2 as DataLogger;
                dataLogger.Clear();               
                scottplot.Refresh();
                //if (FormIsOpen(typeof(datachart)))
                //{
                //    ChartCleanEvent();
                //}
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
            var ScaleY2MenuItem = new MenuItem() { Header = "取消自动缩放Y2轴" };
            ScaleY2MenuItem.Click += (sender, e) =>
            {
                var menuitem = (sender as MenuItem);
                var contextMenu = menuitem.Parent as ContextMenu;
                var scottplot = contextMenu.PlacementTarget as WpfPlot;
                if (menuitem.Header.ToString() == "取消自动缩放Y2轴")
                {
                    menuitem.Header = "自动缩放Y2轴";
                    chartY2AutoSize = false;
                }
                else
                {
                    menuitem.Header = "取消自动缩放Y2轴";
                    chartY2AutoSize = true;
                    scottplot.Plot.Axes.AutoScaleY(scottplot.Plot.Axes.Right);
                    scottplot.Refresh();
                }
            };
            chartMenu.Items.Add(ScaleY2MenuItem);
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
                    scottplot.Plot.Save(sfd.FileName,(int) scottplot.ActualWidth, (int)scottplot.ActualHeight);
                
            };
            chartMenu.Items.Add(SaveImageMenuItem);
            chartMenu.Items.Add(new Separator());

            var CH1MenuItem = new MenuItem() { Header = "数据通道1" };
            var CH2MenuItem = new MenuItem() { Header = "数据通道2" };
            foreach (var item in SigThemes)
            {
                var chx1 = new MenuItem() { Header = item.name };
                chx1.Click += (sender, e) =>
                {
                    var menuitem = (sender as MenuItem);
                    var menuitem2 = (menuitem.Parent as MenuItem);
                    var contextMenu = menuitem2.Parent as ContextMenu;
                    var scottplot = contextMenu.PlacementTarget as WpfPlot;
                    ChartChlChangeData(menuitem2.Header.ToString(), menuitem.Header.ToString());
                };
                CH1MenuItem.Items.Add(chx1);

                var chx2 = new MenuItem() { Header = item.name };
                chx2.Click += (sender, e) =>
                {
                    var menuitem = (sender as MenuItem);
                    var menuitem2 = (menuitem.Parent as MenuItem);
                    var contextMenu = menuitem2.Parent as ContextMenu;
                    var scottplot = contextMenu.PlacementTarget as WpfPlot;
                    ChartChlChangeData(menuitem2.Header.ToString(), menuitem.Header.ToString());
                };
                CH2MenuItem.Items.Add(chx2);
            }

            chartMenu.Items.Add(CH1MenuItem);
            chartMenu.Items.Add(CH2MenuItem);




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
                        if (Math.Abs(e.GetPosition(e.Source as FrameworkElement).X - plotMouseDownLocation.X) <50 &&
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
      
        void ChartChlChangeData(string chl, string data)
        {
            ScottPlot.Plottables.DataLogger dataLogger;
            if (chl == "数据通道1")
            {
                dataLogger = sig1 as DataLogger;
            }
            else
            {
                dataLogger = sig2 as DataLogger;
            }
            dataLogger.Clear();

            for (int i = 0; i < SigThemes.Count; i++)
            {
                if (SigThemes[i].name == data)
                {
                    SigChlSelect(dataLogger, i);
                }
            }

            int idx = get_sig_data_idx(dataLogger);



            for (int i = 0; i < devMeasureDatas.Count; i++)
            {
                dataLogger.Add(devMeasureDatas[i].time.ToOADate(), devMeasureDatas[i].data_list[idx]);
            }

            ChartRender();

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
       
        void sig_invert(object sig, DevMeasureData data)
        {
            ScottPlot.Plottables.DataLogger dataLogger = sig as DataLogger;
            int idx = get_sig_data_idx(dataLogger);
            dataLogger.Add(data.time.ToOADate(), data.data_list[idx]);
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
            if (chartY2AutoSize)
            {
                scottplot.Plot.Axes.AutoScaleY(scottplot.Plot.Axes.Right);
            }
            Application.Current.Dispatcher.Invoke(() => scottplot.Refresh());
        }
    }
}
