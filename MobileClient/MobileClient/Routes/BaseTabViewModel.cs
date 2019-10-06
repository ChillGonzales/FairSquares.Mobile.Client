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
        private readonly ILogger<BaseTabViewModel> _logger;
        private readonly MainThreadNavigator _nav;
        private bool _dialogShown;

        public BaseTabViewModel( ICache<SettingsModel> settings,
                                IPageFactory pageFactory,
                                ILogger<BaseTabViewModel> logger,
                                MainThreadNavigator nav)
        {
            _settings = settings;
            _pageFactory = pageFactory;
            _logger = logger;
            _nav = nav;
            OnAppearingBehavior = new Command(() =>
            {
                try
                {
                    if (!_dialogShown && (!_settings.GetAll().Any() || _settings.Get("").DisplayWelcomeMessage))
                    {
                        _dialogShown = true;
                        _nav.Push(_pageFactory.GetPage(PageType.Instruction, true));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to check settings on load.", ex);
                }
            });
        }

        // Bound properties
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
