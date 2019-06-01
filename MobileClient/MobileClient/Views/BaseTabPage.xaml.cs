using MobileClient.Models;
using MobileClient.Utilities;
using MobileClient.Utility;
using MobileClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BaseTabPage : TabbedPage
    {
        private readonly ICache<SettingsModel> _settings;
        private readonly IPageFactory _pageFactory;
        private bool _dialogShown;

        public BaseTabPage()
        {
            InitializeComponent();
            _settings = App.Container.GetInstance<ICache<SettingsModel>>();
            _pageFactory = App.Container.GetInstance<IPageFactory>();
            NavigateFromMenu(BaseNavPageType.MyOrders);
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            if (!_dialogShown && (!_settings.GetAll().Any() || _settings.Get("").DisplayWelcomeMessage))
            {
                _dialogShown = true;
                await CurrentPage.Navigation.PushAsync(_pageFactory.GetPage(PageType.Instruction, true));
            }
        }

        public void NavigateFromMenu(BaseNavPageType pageType)
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
                    CurrentPage = OrderTab;
                    break;
            }
        }
    }
}