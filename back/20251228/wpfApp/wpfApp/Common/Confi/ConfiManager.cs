
using wpfApp.Extensions;
using Prism.Ioc;

//using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Navigation.Regions;

namespace wpfApp.Common.Confi
{
    public class ConfiManager : IConfiManager
    {
        private readonly IContainerExtension container;
        private readonly IRegionManager regionManager;

        public ConfiManager(IContainerExtension container, IRegionManager regionManager)
        {
            this.container = container;
            this.regionManager = regionManager;
        }


        public void RegionConfiInit()
        {
           
            //regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate("HomePageView");
            //regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate("HomeTabView");

            regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate("PLDMainView");

        }
    }
}
