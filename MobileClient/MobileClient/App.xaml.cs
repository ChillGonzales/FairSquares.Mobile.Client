using FairSquares.Measurement.Core.Models;
using MobileClient.Models;
using MobileClient.Services;
using MobileClient.Utilities;
using MobileClient.Views;
using SimpleInjector;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace MobileClient
{
    public partial class App : Application
    {
        public readonly static Container Container;
        public static string MemberId = "fc20a942-5dce-4a2b-a916-4b916a6d41d9";
        private static string _apiKey = "30865dc7-8e15-4fab-a777-0b795370a9d7";
        private static string _orderEndpoint = @"https://fairsquares-order-management-api.azurewebsites.net/api/orders";

        static App()
        {
            try
            {
                Container = new Container();
                var orderService = new AzureOrderService(_orderEndpoint, _apiKey);
                var propertyService = new MockPropertyService();

                // Register services
                Container.Register<IOrderService>(() => orderService, Lifestyle.Singleton);
                Container.Register<IPropertyService>(() => propertyService, Lifestyle.Singleton);
                Container.Register(typeof(ILogger<>), typeof(DebugLogger<>), Lifestyle.Transient);

                // Setup caches and begin process of filling them.
                var propertyCache = new MemoryCache<PropertyModel>(new DebugLogger<MemoryCache<PropertyModel>>());
                var orderCache = new MemoryCache<Order>(new DebugLogger<MemoryCache<Order>>());
                Task.Run(() =>
                {
                    var orders = orderService.GetMemberOrders(MemberId);
                    orderCache.Put(orders.ToDictionary(x => x.OrderId, x => x));
                    var properties = propertyService.GetProperties(orders.Select(x => x.OrderId).ToList());
                    propertyCache.Put(properties);
                });

                // Finish registering created caches
                Container.Register<ICache<PropertyModel>>(() => propertyCache, Lifestyle.Singleton);
                Container.Register<ICache<Order>>(() => orderCache, Lifestyle.Singleton);
                Container.RegisterConditional(typeof(ICache<>), typeof(MemoryCache<>), c => !c.Handled);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
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
