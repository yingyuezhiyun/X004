using DryIoc;
using DryIoc.ImTools;
using Prism.Commands;
using Prism.Container.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Navigation.Regions;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml.Linq;
using wpfApp.Common.Dialog;
using wpfApp.Common.Events;
using wpfApp.Common.Models;
using wpfApp.Extensions;
using wpfApp.Views;

namespace wpfApp.ViewModels
{
    public class HomeTabViewModel : NavigationViewModel
    {
        private readonly IRegionManager regionManager;
        private readonly IContainerExtension container;
        private readonly IDialogHostService dialogHostService;

        //private readonly IContainerProvider provider;

        int DevCNT = 8;
        public ObservableCollection<CustomTab> CustomTabs { get; private set; }

        public DelegateCommand<CustomTab> NavigateCommand { get; private set; }

        private int DevIndex = 1;

        public HomeTabViewModel(IRegionManager regionManager, IContainerExtension container, IDialogHostService dialogHostService) : base(container)
        {
           
            this.container = container;
           
            this.regionManager = regionManager;
            this.dialogHostService = dialogHostService;

            NavigateCommand = new DelegateCommand<CustomTab>(Navigate);



            CreateCustomTabs();

            //CustomTabs[1].Visibility = Visibility.Visible;

            GoBackCommand = new DelegateCommand(() =>
            {
                if (journal != null && journal.CanGoBack)
                    journal.GoBack();
            });
            GoForwardCommand = new DelegateCommand(() =>
            {
                if (journal != null && journal.CanGoForward)
                    journal.GoForward();
            });

            aggregator.GetEvent<DevManageEvent>().Subscribe(async e => { await DevManageUpdata(e); });

        }


        async Task DevManageUpdata(DevManageUpdataModel devManageUpdataModel)
        {
            if (devManageUpdataModel.Type == DevManageUpdataType.Updata)
            {
                CustomTabs[devManageUpdataModel.CH + 1].Visibility = devManageUpdataModel.IsOpen ? Visibility.Visible : Visibility.Collapsed;
            }
        }




        private int _customTabSelectedIndex;

        public int CustomTabSelectedIndex
        {
            get { return _customTabSelectedIndex; }
            set { _customTabSelectedIndex = value; RaisePropertyChanged(); }
        }

       
        
        void CreateCustomTabs()
        {

            CustomTabs = new ObservableCollection<CustomTab>();

            CustomTabs.Add(new CustomTab() { TabTitle = "主页", NameSpace = "TabStatus",Visibility=Visibility.Visible });

            for (int i = 0; i < DevCNT; i++)
            {
                char c = (char)('A' + i);
                CustomTabs.Add(new CustomTab() { TabTitle = "通道" + c, NameSpace = "TabDev" + (i + 1) });
            }

            for (int i = 0; i < DevCNT; i++)
            {
                char c = (char)('A' + i);
                string temp;
                temp = OperateIniFile.ReadIniData("Channel"+c, "Title");
                if (temp != null && temp != string.Empty)
                {
                    CustomTabs[i+1].TabTitle = temp;
                }
            }


        }

        void regionAddView(string View)
        {
            var region = regionManager.Regions[PrismManager.HomeTabContentRegionName];
            regionManager.AddToRegion(PrismManager.HomeTabContentRegionName, View);
            PropertyInfo propertyInfo = region.GetType().GetProperty("ItemMetadataCollection", BindingFlags.NonPublic | BindingFlags.Instance);
            ObservableCollection<Prism.Navigation.Regions.ItemMetadata> itemMetadata = (ObservableCollection<Prism.Navigation.Regions.ItemMetadata>)propertyInfo.GetValue(region);
            itemMetadata.Last().Name = View;
        }

        private bool IsRegionIni =false;

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            var region = regionManager.Regions[PrismManager.HomeTabContentRegionName];
            if (IsRegionIni == false)
            {
                //regionAddView("TabStatus");
                //for (int i = 0; i < 8; i++)
                //{
                //    regionAddView("TabDev" + (i + 1));
                //}
                for (int i = 1; i < CustomTabs.Count; i++)
                {
                    var para = new NavigationParameters();
                    para.Add("NameSpace", CustomTabs[i].TabTitle);
                    para.Add("CH", i-1);
                    region.RequestNavigate(CustomTabs[i].NameSpace, para);
                }
                IsRegionIni = true;
            }
            region.RequestNavigate("TabStatus");

            //TabDevView tabDevView = new TabDevView();
            //tabDevView.DataContext = new TabDevViewModel();
            //region.Add(tabDevView, "TabDev1");



        }
        private IRegionNavigationJournal journal;
        public DelegateCommand GoBackCommand { get; private set; }
        public DelegateCommand GoForwardCommand { get; private set; }
        private void Navigate(CustomTab obj)
        {
            if (obj == null || string.IsNullOrWhiteSpace(obj.NameSpace))
            {
                return;
            }

            var region = regionManager.Regions[PrismManager.HomeTabContentRegionName];
            var para = new NavigationParameters();
            para.Add("NameSpace", obj.TabTitle);
            region.RequestNavigate(obj.NameSpace,back =>
            {
                journal = back.Context.NavigationService.Journal;
            }, para);

            //regionManager.RequestNavigate(PrismManager.HomeTabContentRegionName, obj.NameSpace);



            //var view = region.GetView(obj.NameSpace);
            ////activate
            //region.Activate(view);
            ////remove
            ////region.Remove(view);

        }
    }

    public class CustomTab : BindableBase
    {
     

   
        public CustomTab()
        {
          
        }

        private string _tabTitle;

        public string TabTitle
        {
            get { return _tabTitle; }
            set { _tabTitle = value; RaisePropertyChanged(); }
        }

        private Visibility _visibility = Visibility.Collapsed;

        public Visibility Visibility
        {
            get { return _visibility; }
            set { _visibility = value; RaisePropertyChanged(); }
        }

        private Visibility _tabIconVisibility = Visibility.Collapsed;

        public Visibility TabIconVisibility
        {
            get { return _tabIconVisibility; }
            set { _tabIconVisibility = value; RaisePropertyChanged(); }
        }


        private string _nameSpace;
        public string NameSpace
        {
            get { return _nameSpace; }
            set { _nameSpace = value; RaisePropertyChanged(); }
        }

        private string _tabIcon;
        public string TabIcon
        {
            get { return _tabIcon; }
            set { _tabIcon = value; RaisePropertyChanged(); }
        }


    }


}
