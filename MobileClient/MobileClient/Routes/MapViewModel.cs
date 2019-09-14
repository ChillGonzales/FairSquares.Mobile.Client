using MobileClient.Services;
using MobileClient.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace MobileClient.Routes
{
    public class MapViewModel : INotifyPropertyChanged
    {
        Position? point1;
        Pin pin1;
        Position? point2;
        Pin pin2;
        bool measure;
        private string _searchBarText;
        private string _actionButtonText;
        private bool _actionButtonVisible;
        private IList<Pin> _pins;

        public MapViewModel(IToastService toast,
                            Action<Pin> addPin,
                            Action<Pin> removePin,
                            Action<MapSpan> setMapSpan,
                            MainThreadNavigator navigation,
                            IPageFactory pageFactory)
        {
            _toast = toast;
            _addPin = addPin;
            _removePin = removePin;
            _setMapSpan = setMapSpan;
            _navigation = navigation;
            _pageFactory = pageFactory;
            _pins = new List<Pin>();
            ActionButtonText = "Measure It!";
            MapTapCommand = new Command(x =>
            {
                var e = (TapEventArgs)x;
                Console.WriteLine($"Latitude: {e.Position.Latitude.ToString()} Longitude: {e.Position.Longitude.ToString()}");
                if (point1 == null)
                {
                    if (_pins.Any())
                    {
                        RemovePin(pin1);
                        RemovePin(pin2);
                    }
                    point1 = e.Position;
                    pin1 = new Pin() { Position = e.Position, Type = PinType.Generic, Label = "Point 1" };
                    AddPin(pin1);
                }
                else if (point2 == null)
                {
                    point2 = e.Position;
                    pin2 = new Pin() { Position = e.Position, Type = PinType.Generic, Label = "Point 2" };
                    AddPin(pin2);
                    measure = true;
                }
                if (measure)
                {
                    var R = 6371e3; // metres
                    var φ1 = ToRadians(point1.Value.Latitude);
                    var φ2 = ToRadians(point2.Value.Latitude);
                    var Δφ = ToRadians(point2.Value.Latitude - point1.Value.Latitude);
                    var Δλ = ToRadians(point2.Value.Longitude - point1.Value.Longitude);

                    var a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                            Math.Cos(φ1) * Math.Cos(φ2) *
                            Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
                    var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                    var d = R * c;
                    var feet = d * 3.28084;
                    App.Container.GetInstance<IToastService>().LongToast($"The distance between the two points is {feet} feet.");
                    point1 = null;
                    point2 = null;
                    measure = false;
                }
            });
            SearchCommand = new Command(async x =>
            {
                if (string.IsNullOrWhiteSpace(SearchBarText))
                    return;
                var locations = await Geocoding.GetLocationsAsync(SearchBarText);
                if (locations.Any())
                {
                    var loc = locations.First();
                    var pos = new Position(loc.Latitude, loc.Longitude);
                    var pl = await Geocoding.GetPlacemarksAsync(locations.FirstOrDefault());
                    var pin = new Pin() { Position = pos, Type = PinType.Place, Address = pl.FirstOrDefault()?.PostalCode, Label = pl.FirstOrDefault()?.PostalCode };
                    _setMapSpan(MapSpan.FromCenterAndRadius(new Position(loc.Latitude, loc.Longitude), Distance.FromMeters(10)));
                    AddPin(pin);
                    ActionButtonVisible = true;
                }
            });
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
        private double ToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }

        #region Binding
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IToastService _toast;
        private readonly Action<Pin> _addPin;
        private readonly Action<Pin> _removePin;
        private readonly Action<MapSpan> _setMapSpan;
        private readonly MainThreadNavigator _navigation;
        private readonly IPageFactory _pageFactory;

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
        public ICommand ActionButtonCommand { get; private set; } = new Command(() => { });
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
        #endregion
    }
}
