using MobileClient.Services;
using MobileClient.Views;
using SimpleInjector;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace MobileClient
{
    public partial class App : Application
    {
        public readonly static Container Container;
        public static string MemberId = "fc20a942-5dce-4a2b-a916-4b916a6d41d9";
        private static string _orderEndpoint = @"https://fairsquares-order-management-api.azurewebsites.net/api/orders";
        private static string _customerEndpoint = @"https://fairsquares-order-management-api.azurewebsites.net/api/customers";
        private static string _apiKey = "30865dc7-8e15-4fab-a777-0b795370a9d7";

        static App()
        {
            Container = new Container();
            Container.Register<IOrderService>(() => new AzureOrderService(_orderEndpoint, _apiKey), Lifestyle.Singleton);
        }

        public App()
        {
            InitializeComponent();
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
