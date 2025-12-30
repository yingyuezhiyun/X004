using wpf_app.Extensions;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpf_app.ViewModels
{
    public class SettingsViewModel:NavigationViewModel
    {
        private readonly IRegionManager regionManager;
        public SettingsViewModel(IRegionManager regionManager, IContainerProvider provider):base(provider)
        {
            this.regionManager = regionManager;
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            regionManager.Regions[PrismManager.SettingsViewRegionName].RequestNavigate("SkinView");
        }






    }
}
