﻿using FairSquares.Measurement.Core.Models;
using MobileClient.Models;
using MobileClient.Services;
using MobileClient.Utilities;
using MobileClient.Views;
using Newtonsoft.Json;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Auth;
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
        private const string _clientID = "81761642488-ovc2vh5394h1ebd1d6jusqv7q2jefbs2.apps.googleusercontent.com";
        private static string _orderEndpoint = @"https://fairsquares-order-management-api.azurewebsites.net/api/orders";
        private static string _propertyEndpoint = @"https://property-measurements.azurewebsites.net/api/properties";
        private static string _blobEndpoint = @"https://fairsquaresapplogging.blob.core.windows.net/roof-images";
        private const string _scope = "plus.login";
        private const string _googleDiscoveryDoc = @"https://accounts.google.com/.well-known/openid-configuration";
        private static string _googleRedirectUrl = @"81761642488-ovc2vh5394h1ebd1d6jusqv7q2jefbs2.apps.googleusercontent.com";
        private static string _googleAuthUrl = @"";
        private static string _googleTokenUrl = @"";

        static App()
        {
            try
            {
                var http = new HttpClient();
                var response = http.GetAsync(_googleDiscoveryDoc).Result;
                var content = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content.ReadAsStringAsync().Result);
                _googleAuthUrl = content["authorization_endpoint"].ToString();
                _googleTokenUrl = content["token_endpoint"].ToString();
                Container = new Container();
                var orderService = new AzureOrderService(_orderEndpoint, _apiKey);
                var propertyService = new PropertyService(_propertyEndpoint, new DebugLogger<PropertyService>());
                var imageService = new BlobImageService(_blobEndpoint, new DebugLogger<BlobImageService>());
                var authenticator = new OAuth2Authenticator(_clientID, null, _scope, new Uri(_googleAuthUrl), 
                                                            new Uri(_googleRedirectUrl), new Uri(_googleTokenUrl), null, true);

                // Register services
                Container.Register<IOrderService>(() => orderService, Lifestyle.Singleton);
                Container.Register<IPropertyService>(() => propertyService, Lifestyle.Singleton);
                Container.Register<IImageService>(() => imageService, Lifestyle.Singleton);
                Container.Register(typeof(ILogger<>), typeof(DebugLogger<>), Lifestyle.Transient);

                // Setup caches and begin process of filling them.
                var propertyCache = new MemoryCache<PropertyModel>(new DebugLogger<MemoryCache<PropertyModel>>());
                var orderCache = new MemoryCache<Order>(new DebugLogger<MemoryCache<Order>>());
                var imageCache = new MemoryCache<ImageModel>(new DebugLogger<MemoryCache<ImageModel>>());
                Task.Run(async () =>
                {
                    try
                    {
                        var orders = await orderService.GetMemberOrders(MemberId);
                        orderCache.Put(orders.ToDictionary(x => x.OrderId, x => x));
                        var propTask = Task.Run(async () =>
                        {
                            var properties = await propertyService.GetProperties(orders.Select(x => x.OrderId).ToList());
                            propertyCache.Put(properties);
                        });
                        var imgTask = Task.Run(() =>
                        {
                            var images = imageService.GetImages(orders.Select(x => x.OrderId).ToList());
                            imageCache.Put(images);
                        });
                        await Task.WhenAll(new[] { propTask, imgTask });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to fill caches.\n{ex.ToString()}");
                    }
                });

                // Finish registering created caches
                Container.Register<ICache<PropertyModel>>(() => propertyCache, Lifestyle.Singleton);
                Container.Register<ICache<Order>>(() => orderCache, Lifestyle.Singleton);
                Container.Register<ICache<ImageModel>>(() => imageCache, Lifestyle.Singleton);
                Container.RegisterConditional(typeof(ICache<>), typeof(MemoryCache<>), c => !c.Handled);

                // Login user
                var presenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();
                presenter.Login(authenticator);
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
