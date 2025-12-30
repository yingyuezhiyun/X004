using wpfApp.Common;
using wpfApp.Common.Confi;

using wpfApp.Common.Dialog;

using wpfApp.ViewModels;
using wpfApp.Views;
using Prism.DryIoc;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace wpfApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            // return Container.Resolve<Views.MainView2>();
            return Container.Resolve<Views.MainView>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {

            containerRegistry.RegisterSingleton<IConfiManager, ConfiManager>();
            containerRegistry.Register<IDialogHostService, DialogHostService>();
            containerRegistry.RegisterForNavigation<MsgView, MsgViewModel>();

            containerRegistry.RegisterForNavigation<SkinView, SkinViewModel>();
            containerRegistry.RegisterForNavigation<HomePageView, HomePageViewModel>();

            containerRegistry.RegisterForNavigation<HomeTabView, HomeTabViewModel>();

            //containerRegistry.RegisterForNavigation<TabStatusView, TabStatusViewModel>();

            containerRegistry.RegisterForNavigation<TabStatusView, TabStatusViewModel>("TabStatus");

            for (int i = 0; i < 8; i++)
            {
                containerRegistry.RegisterForNavigation<TabDevView, TabDevViewModel>("TabDev" + (i + 1));
            }

            containerRegistry.RegisterForNavigation<DataChartView2, DataChartViewModel>();

            containerRegistry.RegisterForNavigation<PLDMainView, PLDMainViewModel>();



        }
        protected override void OnInitialized()
        {

            var ConfiInit = Container.Resolve<IConfiManager>();
            if (ConfiInit != null)
            {
                ConfiInit.RegionConfiInit();

            }
            base.OnInitialized();
        }
    }
}
