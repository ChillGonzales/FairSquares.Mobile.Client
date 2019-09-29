using MobileClient.Services;
using MobileClient.Utility;
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
        private string _movePinText;
        private Placemark _foundLocation;
        private bool _movePinVisible;
        private Color _movePinColor;

        public MapViewModel(IToastService toast,
                            Action<Pin> addPin,
                            Action<Pin> removePin,
                            Action<MapSpan> setMapSpan,
                            MainThreadNavigator navigation,
                            IPageFactory pageFactory,
                            bool hasLocationPermission)
        {
            _toast = toast;
            _addPin = addPin;
            _removePin = removePin;
            _setMapSpan = setMapSpan;
            _navigation = navigation;
            _pageFactory = pageFactory;
            _pins = new List<Pin>();
            _isShowingUser = hasLocationPermission;
            ActionButtonText = "Measure It";
            MovePinText = "Move Pin";
            SearchCommand = new Command(async x => await this.Search(SearchBarText));
            MovePinCommand = new Command(() => ToggleMovePinMode());
            ActionButtonCommand = new Command(() => _navigation.Push(_pageFactory.GetPage(PageType.OrderConfirmation, _foundLocation)));
        }

        private void ToggleMovePinMode()
        {

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
            _foundLocation = (await Geocoding.GetPlacemarksAsync(locations.FirstOrDefault())).FirstOrDefault();
            var address = $"{_foundLocation.SubThoroughfare} {_foundLocation.Thoroughfare}";
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
        #endregion
    }
}
