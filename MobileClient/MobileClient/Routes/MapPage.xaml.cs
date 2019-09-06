using MobileClient.Services;
using MobileClient.Utility;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace MobileClient.Routes
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        Position? point1;
        Pin pin1;
        Position? point2;
        Pin pin2;
        bool measure;

        public MapPage()
        {
            InitializeComponent();
            var map = new ExtendedMap(
            MapSpan.FromCenterAndRadius(
                    new Position(42.938362, -85.627633), Distance.FromMiles(0.1)))
            {
                HeightRequest = 100,
                WidthRequest = 960,
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            map.MapType = MapType.Hybrid;
            map.Tap += (s, e) =>
            {
                Console.WriteLine($"Latitude: {e.Position.Latitude.ToString()} Longitude: {e.Position.Longitude.ToString()}");
                if (point1 == null)
                {
                    if (map.Pins.Any())
                    {
                        map.Pins.Remove(pin1);
                        map.Pins.Remove(pin2);
                    }
                    point1 = e.Position;
                    pin1 = new Pin() { Position = e.Position, Type = PinType.Generic, Label = "Point 1" };
                    map.Pins.Add(pin1);
                }
                else if (point2 == null)
                {
                    point2 = e.Position;
                    pin2 = new Pin() { Position = e.Position, Type = PinType.Generic, Label = "Point 2" };
                    map.Pins.Add(pin2);
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
            };
            var stack = new StackLayout { Spacing = 0 };
            stack.Children.Add(map);
            Content = stack;
        }
        private double ToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }
    }
}