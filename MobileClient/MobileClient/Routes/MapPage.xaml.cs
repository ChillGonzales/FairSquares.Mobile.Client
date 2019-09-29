using MobileClient.Services;
using MobileClient.Utilities;
using MobileClient.Utility;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileClient.Routes
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        public MapPage()
        {
            InitializeComponent();
            bool locationPermissions = false;
            try
            {
                var status = CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location).Result;
                if (status != PermissionStatus.Granted)
                {
                    if (CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Location).Result)
                    {
                        DisplayAlert("LocationPermission", "Your location is used to center the map before searching.", "OK");
                    }
                    var results = CrossPermissions.Current.RequestPermissionsAsync(Permission.Location).Result;
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
            BindingContext = new MapViewModel(App.Container.GetInstance<IToastService>(),
                                      p =>
                                      {
                                          Map.Pins.Add(p);
                                      },
                                      p => Map.Pins.Remove(p),
                                      s => Map.MoveToRegion(s),
                                      new MainThreadNavigator(x => Device.BeginInvokeOnMainThread(x), this.Navigation),
                                      App.Container.GetInstance<IPageFactory>(),
                                      App.Container.GetInstance<ILogger<MapViewModel>>());
        }
    }
}