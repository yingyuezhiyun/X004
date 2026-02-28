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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using wpfApp.Common.Dialog;
using wpfApp.Common.Events;

using static MaterialDesignThemes.Wpf.Theme;

namespace wpfApp.ViewModels
{
   
    public class PLDChartViewModel : BindableBase, IDialogAware
    {
        private readonly IContainerExtension containerProvider;
        public readonly IEventAggregator aggregator;
        public WpfPlot scottplot { get; set; }

        public DialogCloseListener RequestClose { get; set; }

        public DelegateCommand ClearDataCommand { get; private set; }

        public PLDChartViewModel(IContainerExtension containerProvider, IEventAggregator aggregator)
        {
            this.containerProvider = containerProvider;
            this.aggregator = aggregator;
            ClearDataCommand = new DelegateCommand(ClearData);
            ChartIni();
            aggregator.GetEvent<PLDUpdateEvent>().Subscribe(OnPLDUpdate);
        }


        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
           
        }

        object sig1, sig2;

        // UI-bindable lists and selections
        public ObservableCollection<string> SigNames { get; set; } = new ObservableCollection<string>();

        private int _selectedSig1Index = 0;
        public int SelectedSig1Index
        {
            get => _selectedSig1Index;
            set
            {
                if (_selectedSig1Index == value) return;
                _selectedSig1Index = value;
                RaisePropertyChanged();
                // update left axis label to reflect selected signal
                scottplot.Plot.Axes.Left.Label.Text = SigNames.Count > value ? SigNames[value] : "";
                // do not set series label (no legend required)
                // do not change series color
                ChartRender();
            }
        }

        private int _selectedSig2Index = 1;
        public int SelectedSig2Index
        {
            get => _selectedSig2Index;
            set
            {
                if (_selectedSig2Index == value) return;
                _selectedSig2Index = value;
                RaisePropertyChanged();
                // update right axis label to reflect selected signal
                scottplot.Plot.Axes.Right.Label.Text = SigNames.Count > value ? SigNames[value] : "";
                // do not set series label (no legend required)
                // do not change series color
                ChartRender();
            }
        }

        private bool _autoScaleX = true;
        public bool AutoScaleX
        {
            get => _autoScaleX;
            set { _autoScaleX = value; RaisePropertyChanged(); }
        }

        private bool _autoScaleY1 = true;
        public bool AutoScaleY1
        {
            get => _autoScaleY1;
            set { _autoScaleY1 = value; RaisePropertyChanged(); }
        }

        private bool _autoScaleY2 = true;
        public bool AutoScaleY2
        {
            get => _autoScaleY2;
            set { _autoScaleY2 = value; RaisePropertyChanged(); }
        }

        // compute virtual signals (TEC voltages)
        double GetSignalValue(int sigIndex, List<double> data_list)
        {
            // direct-mapped signals: indices 0..20
            if (sigIndex >= 0 && sigIndex <= 20)
            {
                if (sigIndex < data_list.Count) return data_list[sigIndex];
                return 0;
            }
            // virtual TEC voltages appended at indices 21..24
            // mapping: TEC1 voltage = power(8) / curr(9)
            try
            {
                switch (sigIndex)
                {
                    case 21: // TEC1 电压
                        if (data_list.Count > 9 && Math.Abs(data_list[9]) > 1e-6) return data_list[8] / data_list[9];
                        return 0;
                    case 22: // TEC2 电压
                        if (data_list.Count > 12 && Math.Abs(data_list[12]) > 1e-6) return data_list[11] / data_list[12];
                        return 0;
                    case 23: // TEC3 电压
                        if (data_list.Count > 15 && Math.Abs(data_list[15]) > 1e-6) return data_list[14] / data_list[15];
                        return 0;
                    case 24: // TEC4 电压
                        if (data_list.Count > 18 && Math.Abs(data_list[18]) > 1e-6) return data_list[17] / data_list[18];
                        return 0;
                    default:
                        return 0;
                }
            }
            catch
            {
                return 0;
            }
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
            scottplot.Plot.Axes.Right.TickLabelStyle.FontSize = 16;
            //添加曲线数据
            sig1 = scottplot.Plot.Add.DataLogger();
            sig2 = scottplot.Plot.Add.DataLogger();

            //设置坐标系
            ScottPlot.Plottables.DataLogger char_sig1 = sig1 as DataLogger;
            char_sig1.Axes.YAxis = scottplot.Plot.Axes.Left;
            char_sig1.ManageAxisLimits = false;

            //设置坐标系
            ScottPlot.Plottables.DataLogger char_sig2 = sig2 as DataLogger;
            char_sig2.Axes.YAxis = scottplot.Plot.Axes.Right;
            char_sig2.ManageAxisLimits = false;
         

            // define available signal names (keeps mapping consistent with PLDUpdateEvent publishing order)
            var names = new List<string>() {
                "LD1电流", // idx 0
                "LD2电流", // idx 1
                "LD1电压", // idx 2
                "LD2电压", // idx 3
                "电源温度", // idx 4
                "PD出光功率", // idx 5
                "PD温度", // idx 6
                "TEC1温度", "TEC1功率", "TEC1电流",
                "TEC2温度", "TEC2功率", "TEC2电流",
                "TEC3温度", "TEC3功率", "TEC3电流",
                "TEC4温度", "TEC4功率", "TEC4电流",
                // virtual TEC voltages (computed as power / current)
                "TEC1电压", "TEC2电压", "TEC3电压", "TEC4电压",
                "系统电压", "系统电流"
            };
            SigNames.Clear();
            foreach (var nm in names) SigNames.Add(nm);

            // default selections
            SelectedSig1Index = 0;
            SelectedSig2Index = Math.Min(1, SigNames.Count - 1);

            // Ensure axis labels are set for both sides on init
            scottplot.Plot.Axes.Left.Label.Text = SigNames.Count > SelectedSig1Index ? SigNames[SelectedSig1Index] : "";
            scottplot.Plot.Axes.Right.Label.Text = SigNames.Count > SelectedSig2Index ? SigNames[SelectedSig2Index] : "";
            // 
            char_sig1.Axes.YAxis.Label.FontName = ScottPlot.Fonts.Detect(scottplot.Plot.Axes.Left.Label.Text);
            char_sig2.Axes.YAxis.Label.FontName = ScottPlot.Fonts.Detect(scottplot.Plot.Axes.Right.Label.Text);
            char_sig1.Axes.YAxis.Label.ForeColor = char_sig1.Color;
            char_sig1.Axes.YAxis.TickLabelStyle.ForeColor = char_sig1.Color;
            char_sig1.Axes.YAxis.FrameLineStyle.Color = char_sig1.Color;
            char_sig1.Axes.YAxis.MajorTickStyle.Color = char_sig1.Color;
            char_sig1.Axes.YAxis.MinorTickStyle.Color = char_sig1.Color;

            char_sig2.Axes.YAxis.Label.ForeColor = char_sig2.Color;
            char_sig2.Axes.YAxis.TickLabelStyle.ForeColor = char_sig2.Color;
            char_sig2.Axes.YAxis.FrameLineStyle.Color = char_sig2.Color;
            char_sig2.Axes.YAxis.MajorTickStyle.Color = char_sig2.Color;
            char_sig2.Axes.YAxis.MinorTickStyle.Color = char_sig2.Color;

            ChartRender();
        }


        
         void ChartRender()
         {
            if (AutoScaleX)
            {
                scottplot.Plot.Axes.AutoScaleX();
            }
            if (AutoScaleY1)
            {
                scottplot.Plot.Axes.AutoScaleY(scottplot.Plot.Axes.Left);
            }
            if (AutoScaleY2)
            {
                scottplot.Plot.Axes.AutoScaleY(scottplot.Plot.Axes.Right);
            }
             Application.Current.Dispatcher.Invoke(() => scottplot.Refresh());
         }

        void OnPLDUpdate(PLDUpdateModel model)
        {
            if (model == null) return;
            if (model.Type != PLDUpdateType.MeasureData) return;
            var d = model.plData;
            if (d == null) return;

            ScottPlot.Plottables.DataLogger dataLogger1 = sig1 as DataLogger;
            ScottPlot.Plottables.DataLogger dataLogger2 = sig2 as DataLogger;

            // add data if index available
            DateTime t = d.time;
            double x = t.ToOADate();
            try
            {
                int idx1 = SelectedSig1Index;
                int idx2 = SelectedSig2Index;
                double v1 = GetSignalValue(idx1, d.data_list);
                double v2 = GetSignalValue(idx2, d.data_list);
                if (!double.IsNaN(v1)) dataLogger1.Add(x, v1);
                if (!double.IsNaN(v2)) dataLogger2.Add(x, v2);
            }
            catch (Exception)
            {
                // ignore
            }

            ChartRender();
        }

        void ClearData()
        {
            try
            {
                var dl1 = sig1 as DataLogger;
                var dl2 = sig2 as DataLogger;
                dl1?.Clear();
                dl2?.Clear();
                ChartRender();
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}
