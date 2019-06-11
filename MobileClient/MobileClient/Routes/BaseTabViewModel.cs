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
        public readonly ICommand NavigateTo;
        private readonly ICache<SettingsModel> _settings;
        private readonly IPageFactory _pageFactory;
        private readonly INavigation _nav;
        private bool _dialogShown;

        public BaseTabViewModel(ICache<SettingsModel> settings,
                                IPageFactory pageFactory,
                                INavigation nav)
        {
            _settings = settings;
            _pageFactory = pageFactory;
            _nav = nav;
            OnAppearingBehavior = new Command(async () =>
            {
                if (!_dialogShown && (!_settings.GetAll().Any() || _settings.Get("").DisplayWelcomeMessage))
                {
                    _dialogShown = true;
                    await _nav.PushAsync(_pageFactory.GetPage(PageType.Instruction, true));
                }
            });
        }

        // Bound properties
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
