using DryIoc;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Dialogs;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Navigation.Regions;
//using Prism.Regions;
//using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using wpfApp.Common;
using wpfApp.Common.Dialog;
using wpfApp.Common.Events;
using wpfApp.Extensions;
using wpfApp.Views;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace wpfApp.ViewModels
{

    public class MainViewModel :  NavigationViewModel
    {
        private readonly IRegionManager regionManager;
        private readonly IContainerExtension container;
        private readonly IDialogHostService dialogHostService;
        
        public DelegateCommand<string> ExecuteCommand { get; private set; }
        public MainViewModel(IRegionManager regionManager, IContainerExtension container, IDialogHostService dialogHostService) : base(container)
        {
            ExecuteCommand = new DelegateCommand<string>(Execute);
            this.container = container;
            this.regionManager = regionManager;
            this.dialogHostService = dialogHostService;
           
            string temp;
            temp = OperateIniFile.ReadIniData("Dev", "Title");
            if (temp != null && temp != string.Empty)
            {
                Title = temp;
            }
        }


        private string _title = "多通道激光器驱动系统 V4.2";

        public string Title
        {
            get { return _title; }
            set { _title = value; RaisePropertyChanged(); }
        }


        private void Execute(string obj)
        {
            switch (obj)
            {
                case "主题": ModifyTheme(); break;
                case "查找设备":  break;
                default: break;
            }
        }


        async void ModifyTheme()
        {
            
            var dialogResult = await dialogHostService.MyShowDialog("SkinView");
            if (dialogResult.Result == ButtonResult.OK)
            {
            }

        }




        private void Navigate(string obj)
        {
            if (obj == null || string.IsNullOrWhiteSpace(obj))
                return;
            regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate(obj);
        }




    }


}
