using MobileClient.Models;
using MobileClient.Utilities;
using MobileClient.Utility;
using MobileClient.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace MobileClient.Routes
{
    public class BaseTabViewModel : INotifyPropertyChanged
    {
        public readonly ICommand OnAppearingBehavior;
        private readonly ICache<SettingsModel> _settings;
        private readonly IPageFactory _pageFactory;
        private readonly INavigation _nav;
        private readonly IDictionary<BaseNavPageType, Page> _pages;
        private bool _dialogShown;
        private Page _currentPage;

        public BaseTabViewModel(ICache<SettingsModel> settings,
                                IPageFactory pageFactory,
                                INavigation nav,
                                IDictionary<BaseNavPageType, Page> pages)
        {
            _settings = settings;
            _pageFactory = pageFactory;
            _nav = nav;
            _pages = pages;
            OnAppearingBehavior = new Command(async () =>
            {
                if (!_dialogShown && (!_settings.GetAll().Any() || _settings.Get("").DisplayWelcomeMessage))
                {
                    _dialogShown = true;
                    await _nav.PushAsync(_pageFactory.GetPage(PageType.Instruction, true));
                }
            });
            NavigateFromMenu(BaseNavPageType.MyOrders);
        }


        public void NavigateFromMenu(BaseNavPageType pageType)
        {
            switch (pageType)
            {
                case BaseNavPageType.Account:
                    CurrentPage = _pages[BaseNavPageType.Account];
                    break;
                case BaseNavPageType.MyOrders:
                    CurrentPage = _pages[BaseNavPageType.MyOrders];
                    break;
                case BaseNavPageType.Order:
                    CurrentPage = _pages[BaseNavPageType.Order];
                    break;
                default:
                    CurrentPage = _pages[BaseNavPageType.MyOrders];
                    break;
            }
        }

        // Bound properties
        public event PropertyChangedEventHandler PropertyChanged;
        public Page CurrentPage
        {
            get
            {
                return _currentPage;
            }
            set
            {
                _currentPage = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentPage)));
            }
        }
    }
}
