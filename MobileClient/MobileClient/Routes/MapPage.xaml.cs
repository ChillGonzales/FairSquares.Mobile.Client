using MobileClient.Services;
using MobileClient.Utility;
using System;
using System.ComponentModel;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace MobileClient.Routes
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
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
                                      new MainThreadNavigator(x => Device.BeginInvokeOnMainThread(x), this.Navigation),
                                      App.Container.GetInstance<IPageFactory>(),
                                      true);
        }
    }
}