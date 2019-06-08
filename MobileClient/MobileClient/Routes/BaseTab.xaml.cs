using MobileClient.Models;
using MobileClient.Utilities;
using MobileClient.Utility;
using MobileClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileClient.Routes
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BaseTab : TabbedPage
    {
        private readonly ICommand _onAppearing;

        public BaseTab()
        {
            InitializeComponent();
            CurrentPage = MyOrdersTab;
            var vm = new BaseTabViewModel(App.Container.GetInstance<ICache<SettingsModel>>(),
                                          App.Container.GetInstance<IPageFactory>(),
                                          CurrentPage.Navigation);
            _onAppearing = vm.OnAppearingBehavior;
            BindingContext = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _onAppearing.Execute(null);
        }

        public void NavigateToTab(BaseNavPageType pageType)
        {
            switch (pageType)
            {
                case BaseNavPageType.Account:
                    CurrentPage = AccountTab;
                    break;
                case BaseNavPageType.MyOrders:
                    CurrentPage = MyOrdersTab;
                    break;
                case BaseNavPageType.Order:
                    CurrentPage = OrderTab;
                    break;
                default:
                    CurrentPage = MyOrdersTab;
                    break;
            }
        }
    }
}