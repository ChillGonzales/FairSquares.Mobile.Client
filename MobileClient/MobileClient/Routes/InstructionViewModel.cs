using MobileClient.Models;
using MobileClient.Utilities;
using MobileClient.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace MobileClient.Routes
{
    public class InstructionViewModel : INotifyPropertyChanged
    {
        private readonly MainThreadNavigator _nav;
        private readonly ICache<SettingsModel> _settings;
        private readonly ILogger<InstructionViewModel> _logger;
        private GridLength _dismissBtnColHeight;
        private bool _notShowAgain;
        private GridLength _switchColHeight;
        private GridLength _titleColHeight;

        public InstructionViewModel(MainThreadNavigator nav,
                                    ICache<SettingsModel> settings,
                                    ILogger<InstructionViewModel> logger,
                                    bool showDismissButton)
        {
            _nav = nav;
            _settings = settings;
            _logger = logger;
            TitleColHeight = showDismissButton ? 50 : 0;
            SwitchColHeight = showDismissButton ? 50 : 0;
            DismissBtnColHeight = showDismissButton ? 50 : 0;
            DismissCommand = new Command(() =>
            {
                try
                {
                    if (NotShowAgain)
                    {
                        _settings.Put("", new SettingsModel() { DisplayWelcomeMessage = false });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to update settings cache.", ex);
                }
                _nav.Pop();
            });
        }

        // Bound properties
        public event PropertyChangedEventHandler PropertyChanged;
        public GridLength DismissBtnColHeight
        {
            get
            {
                return _dismissBtnColHeight;
            }
            set
            {
                _dismissBtnColHeight = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DismissBtnColHeight)));
            }
        }
        public GridLength SwitchColHeight
        {
            get
            {
                return _switchColHeight;
            }
            set
            {
                _switchColHeight = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SwitchColHeight)));
            }
        }
        public GridLength TitleColHeight
        {
            get
            {
                return _titleColHeight;
            }
            set
            {
                _titleColHeight = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TitleColHeight)));
            }
        }
        public ICommand DismissCommand { get; set; }
        public bool NotShowAgain
        {
            get
            {
                return _notShowAgain;
            }
            set
            {
                _notShowAgain = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NotShowAgain)));
            }
        }
    }
}
