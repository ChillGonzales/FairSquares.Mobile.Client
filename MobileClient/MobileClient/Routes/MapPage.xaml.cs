using MobileClient.Services;
using MobileClient.Utilities;
using MobileClient.Utility;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace MobileClient.Routes
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        private bool _started;
        public MapPage()
        {
            InitializeComponent();
            BindingContext = new MapViewModel(App.Container.GetInstance<IToastService>(),
                                      p =>
                                      {
                                          Map.Pins.Add(p);
                                      },
                                      p => Map.Pins.Remove(p),
                                      s => Map.MoveToRegion(s),
                                      App.Container.GetInstance<MainThreadNavigator>(),
                                      App.Container.GetInstance<IPageFactory>(),
                                      x => this.MovePinButton.StyleClass = new[] { x },
                                      async () => await SetMapLocationToUser(),
                                      App.Container.GetInstance<ILogger<MapViewModel>>());
            App.Container.GetInstance<PageRegistry>().RegisterPage(this);
        }
        private async Task SetMapLocationToUser()
        {
            bool locationPermissions = false;
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
                if (status != PermissionStatus.Granted)
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Location))
                    {
                        await DisplayAlert("LocationPermission", "Your location is used to center the map before searching.", "OK");
                    }
                    var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Location);
                    status = results[Permission.Location];
                }
                if (status == PermissionStatus.Granted)
                {
                    locationPermissions = true;
                }
                else if (status != PermissionStatus.Unknown)
                {
                    locationPermissions = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to finish permissions check." + Environment.NewLine + ex.ToString());
                // Failed, default to false.
                locationPermissions = false;
            }
            Map.IsShowingUser = locationPermissions;
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.High);
                var location = await Geolocation.GetLocationAsync(request);
                if (location != null)
                {
                    Map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(location.Latitude, location.Longitude), new Distance(75)));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to set user's initial location." + Environment.NewLine + ex.ToString());
            }
        }
        protected override async void OnAppearing()
        {
            if (_started)
                return;
            await SetMapLocationToUser();
            _started = true;
        }
    }
}