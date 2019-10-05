﻿using MobileClient.Models;
using MobileClient.Services;
using MobileClient.Utilities;
using MobileClient.Utility;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace MobileClient.Routes
{
    public class MapViewModel : INotifyPropertyChanged
    {
        private string _searchBarText;
        private string _actionButtonText;
        private bool _actionButtonVisible;
        private IList<Pin> _pins;
        private bool _isShowingUser;
        private readonly IToastService _toast;
        private readonly Action<Pin> _addPin;
        private readonly Action<Pin> _removePin;
        private readonly Action<MapSpan> _setMapSpan;
        private readonly MainThreadNavigator _navigation;
        private readonly IPageFactory _pageFactory;
        private readonly ILogger<MapViewModel> _logger;
        private readonly Action _resetMapLocation;
        private string _movePinText;
        private LocationModel _foundLocation;
        private bool _movePinVisible;
        private bool _movePinToggled;
        private Action<string> _movePinStyleAction;
        private bool _actionButtonEnabled;


        public MapViewModel(IToastService toast,
                            Action<Pin> addPin,
                            Action<Pin> removePin,
                            Action<MapSpan> setMapSpan,
                            MainThreadNavigator navigation,
                            IPageFactory pageFactory,
                            Action<string> movePinStyleAction,
                            Action resetMapLocation,
                            ILogger<MapViewModel> logger)
        {
            _toast = toast;
            _addPin = addPin;
            _removePin = removePin;
            _setMapSpan = setMapSpan;
            _navigation = navigation;
            _pageFactory = pageFactory;
            _logger = logger;
            _movePinStyleAction = movePinStyleAction;
            _resetMapLocation = resetMapLocation;
            _pins = new List<Pin>();
            ActionButtonText = "Measure It";
            MovePinText = "Move Pin";
            SearchCommand = new Command(async x => await this.Search(SearchBarText));
            MovePinCommand = new Command(() => ToggleMovePinMode());
            ActionButtonCommand = new Command(() => ConfirmOrder());
        }
        private void ConfirmOrder()
        {
            ActionButtonEnabled = false;
            try
            {
                _navigation.Push(_pageFactory.GetPage(PageType.OrderConfirmation, _foundLocation));
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to navigate to order confirmation page.", ex);
            }
            finally
            {
                ActionButtonEnabled = true;
            }
        }
        private void ToggleMovePinMode()
        {
            MovePinToggled = !MovePinToggled;
            _movePinStyleAction(MovePinToggled ? "btn-outline-secondary" : "btn-secondary");
        }
        private async Task Search(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return;
            var locations = await Geocoding.GetLocationsAsync(searchText);
            if (!locations.Any())
            {
                _toast.LongToast($"Address was not found.");
                return;
            }
            var loc = locations.First();
            var pos = new Position(loc.Latitude, loc.Longitude);
            var pl = (await Geocoding.GetPlacemarksAsync(locations.FirstOrDefault())).FirstOrDefault();
            _foundLocation = new LocationModel()
            {
                Position = pos,
                Placemark = pl
            };
            var address = $"{pl.SubThoroughfare} {pl.Thoroughfare}";
            var pin = new Pin() { Position = pos, Type = PinType.SearchResult, Address = address, Label = address };
            _setMapSpan(MapSpan.FromCenterAndRadius(new Position(loc.Latitude, loc.Longitude), Distance.FromMeters(20)));
            AddPin(pin);
            ActionButtonVisible = true;
            MovePinVisible = true;
        }

        private void RemovePin(Pin pin)
        {
            _pins.Remove(pin);
            _removePin(pin);
        }
        private void AddPin(Pin pin)
        {
            _pins.Add(pin);
            _addPin(pin);
        }

        #region Binding
        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand MapTapCommand { get; private set; }
        public string SearchBarText
        {
            get
            {
                return _searchBarText;
            }
            set
            {
                _searchBarText = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchBarText)));
            }
        }
        public ICommand SearchCommand { get; private set; }
        public string ActionButtonText
        {
            get
            {
                return _actionButtonText;
            }
            set
            {
                _actionButtonText = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActionButtonText)));
            }
        }
        public ICommand ActionButtonCommand { get; private set; }
        public bool ActionButtonVisible
        {
            get
            {
                return _actionButtonVisible;
            }
            set
            {
                _actionButtonVisible = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActionButtonVisible)));
            }
        }
        public bool ActionButtonEnabled
        {
            get
            {
                return _actionButtonEnabled;
            }
            set
            {
                _actionButtonEnabled = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActionButtonEnabled)));
            }
        }
        public bool IsShowingUser
        {
            get
            {
                return _isShowingUser;
            }
            private set
            {
                _isShowingUser = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsShowingUser)));
            }
        }
        public string MovePinText
        {
            get
            {
                return _movePinText;
            }
            set
            {
                _movePinText = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MovePinText)));
            }
        }
        public ICommand MovePinCommand { get; private set; }
        public bool MovePinVisible
        {
            get
            {
                return _movePinVisible;
            }
            set
            {
                _movePinVisible = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MovePinVisible)));
            }
        }
        // This is not currently bound to anything in the view.
        public bool MovePinToggled
        {
            get
            {
                return _movePinToggled;
            }
            set
            {
                _movePinToggled = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MovePinToggled)));
            }
        }
        #endregion
    }
}
