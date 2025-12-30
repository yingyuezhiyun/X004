using MaterialDesignThemes.Wpf;
using Prism.Dialogs;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using wpfApp.Common;
using wpfApp.Common.Dialog;
using wpfApp.Extensions;

namespace wpfApp.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : Window
    {
        private readonly IDialogHostService dialogHostService;
        public IDialogService DialogService { get; set; }
        public MainView(IEventAggregator aggregator, IDialogHostService dialogHostService)
        {
            InitializeComponent();

            //注册提示消息
            aggregator.ResgiterMessage(arg =>
            {
                //Snackbar.MessageQueue.Enqueue(arg.Message);

             
                Snackbar.Background = (Brush)new BrushConverter().ConvertFrom(arg.Color);
                Snackbar.MessageQueue.Enqueue(
                  arg.Message,
                  null,
                  null,
                  null,
                  false,
                  true,
                  TimeSpan.FromSeconds(arg.TimeSpan));
            });
            // btnMin.Visibility = Visibility.Collapsed;
            //注册等待消息窗口
            aggregator.Resgiter(arg =>
            {

                DialogHost.IsOpen = arg.IsOpen;

                if (DialogHost.IsOpen)
                    DialogHost.DialogContent = new ProgressView(arg.Msg);
            });

            btnMin.Click += (s, e) => { this.WindowState = WindowState.Minimized; };
            btnMax.Click += (s, e) =>
            {
                if (this.WindowState == WindowState.Maximized)
                    this.WindowState = WindowState.Normal;
                else
                    this.WindowState = WindowState.Maximized;
            };

            btnClose.Click += async (s, e) =>
            {

                //DialogHost.IsOpen = true;
                //DialogHost.DialogContent = new ProgressView();

                var dialogResult = await dialogHostService.Question("温馨提示", "确认退出系统?");
                //if (dialogResult.Result != Prism.Services.Dialogs.ButtonResult.OK) return;
                if (dialogResult.Result != Prism.Dialogs.ButtonResult.Yes) return;
                System.Environment.Exit(0);
                this.Close();

            };
            ColorZone.MouseMove += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                    this.DragMove();
            };

            ColorZone.MouseDoubleClick += (s, e) =>
            {
                if (this.WindowState == WindowState.Normal)
                    this.WindowState = WindowState.Maximized;
                else
                    this.WindowState = WindowState.Normal;
            };
            this.dialogHostService = dialogHostService;
        }
    }
}
