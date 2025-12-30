using wpfApp.Extensions;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Navigation.Regions;

namespace wpfApp.ViewModels
{
    public class NavigationViewModel : BindableBase, INavigationAware
    {
        private readonly IContainerExtension containerProvider;
        public readonly IEventAggregator aggregator;

        public NavigationViewModel(IContainerExtension containerProvider)
        {
            this.containerProvider = containerProvider;
            aggregator = containerProvider.Resolve<IEventAggregator>();
        }
        public NavigationViewModel()
        {

        }

        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {

        }

        public void UpdateLoading(bool IsOpen)
        {
            aggregator.UpdateLoading(new Common.Events.UpdateModel()
            {
                IsOpen = IsOpen
            });
        }

        public void UpdateLoading(bool IsOpen, string Msg)
        {
            aggregator.UpdateLoading(new Common.Events.UpdateModel()
            {
                IsOpen = IsOpen,
                Msg = Msg
            });
        }
    }
}
