﻿using FairSquares.Measurement.Core.Models;
using MobileClient.Authentication;
using MobileClient.Models;
using MobileClient.Services;
using MobileClient.Utilities;
using MobileClient.Views;
using Newtonsoft.Json;
using Plugin.InAppBilling.Abstractions;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private static string _apiKey = "30865dc7-8e15-4fab-a777-0b795370a9d7";
        private static string _orderEndpoint = @"https://fairsquares-order-management-api.azurewebsites.net/api/orders";
        private static string _notifyEndpoint = @"https://fairsquares-order-management-api.azurewebsites.net/api/notification";
        private static string _subEndpoint = @"https://fairsquares-order-management-api.azurewebsites.net/api/subscriptions";
        private static string _userEndpoint = @"https://fairsquares-order-management-api.azurewebsites.net/api/users";
        private static string _propertyEndpoint = @"https://property-measurements.azurewebsites.net/api/properties";
        private static string _blobEndpoint = @"https://fairsquaresapplogging.blob.core.windows.net/roof-images";
        private const string GoogleAuthorizeUrl = "https://accounts.google.com/o/oauth2/v2/auth";
        private const string GoogleAccessTokenUrl = "https://www.googleapis.com/oauth2/v4/token";

        static App()
        {
            try
            {
                var http = new HttpClient();
                Container = new Container();
                var orderService = new AzureOrderService(_orderEndpoint, _apiKey);
                var propertyService = new PropertyService(_propertyEndpoint, new DebugLogger<PropertyService>());
                var imageService = new BlobImageService(_blobEndpoint, new DebugLogger<BlobImageService>());
                var subService = new SubscriptionService(_subEndpoint, new DebugLogger<SubscriptionService>());
                var authenticator = new OAuth2Authenticator(Configuration.ClientId,
                                                            null,
                                                            Configuration.Scope,
                                                            new Uri(GoogleAuthorizeUrl),
                                                            new Uri(Configuration.RedirectNoPath + ":" + Configuration.RedirectPath),
                                                            new Uri(GoogleAccessTokenUrl),
                                                            null,
                                                            true);
                var userService = new CurrentUserService(AccountStore.Create());
                var notifyService = new NotificationService(new HttpClient(), _notifyEndpoint, _apiKey);
                var emailLogger = new EmailLogger<PurchasingService>(notifyService, userService);
                var purchaseService = new PurchasingService(emailLogger);
                authenticator.Completed += (s, e) =>
                {
                    if (e.IsAuthenticated)
                    {
                        userService.LogIn(e.Account);
                    }
                };

                // Setup caches and begin process of filling them.
                var dbBasePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var propertyCache = new LocalSqlCache<PropertyModel>(Path.Combine(dbBasePath, "property.db3"), new DebugLogger<LocalSqlCache<PropertyModel>>());
                var orderCache = new LocalSqlCache<Order>(Path.Combine(dbBasePath, "order.db3"), new DebugLogger<LocalSqlCache<Order>>());
                var imageCache = new LocalSqlCache<ImageModel>(Path.Combine(dbBasePath, "images.db3"), new DebugLogger<LocalSqlCache<ImageModel>>());
                var subCache = new LocalSqlCache<SubscriptionModel>(Path.Combine(dbBasePath, "subs.db3"), new DebugLogger<LocalSqlCache<SubscriptionModel>>());

                Action ClearCaches = () =>
                {
                    try
                    {
                        orderCache.Clear();
                        propertyCache.Clear();
                        imageCache.Clear();
                        subCache.Clear();
                    }
                    catch { }
                };
                Func<string, Task> RefreshCaches = userId => Task.Run(async () =>
                {
                    try
                    {
                        var orders = await orderService.GetMemberOrders(userId);
                        orderCache.Update(orders.ToDictionary(x => x.OrderId, x => x));
                        var subTask = Task.Run(() =>
                        {
                            try
                            {
                                DependencyService.Get<IMessagingSubscriber>().Subscribe(orders.Select(x => x.OrderId).ToList());
                            }
                            catch { }
                        });
                        var propTask = Task.Run(async () =>
                        {
                            var properties = await propertyService.GetProperties(orders.Select(x => x.OrderId).ToList());
                            propertyCache.Update(properties);
                        });
                        var imgTask = Task.Run(() =>
                        {
                            var images = imageService.GetImages(orders.Select(x => x.OrderId).ToList());
                            imageCache.Update(images);
                        });
                        var subscriptionTask = Task.Run(async () =>
                        {
                            // TODO: This should be somewhere else, not in the client.
                            var sub = subService.GetSubscription(userId);
                            // Check app store purchases to see if they auto-renewed
                            if (sub != null && !SubscriptionUtilities.SubscriptionActive(sub))
                            {
                                var purchases = new List<InAppBillingPurchase>();
                                try
                                {
                                    purchases = (await purchaseService.GetPurchases()).ToList();
                                }
                                catch { }
                                var mostRecent = purchases?.OrderByDescending(x => x.TransactionDateUtc)?.FirstOrDefault();
                                if (mostRecent != null)
                                {
                                    var newSub = SubscriptionUtilities.GetModelFromIAP(mostRecent, userId, sub);
                                    if (newSub != null)
                                        sub = newSub;
                                }
                            }
                            if (sub == null)
                            {
                                subCache.Clear();
                            }
                            else
                            {
                                subCache.Update(new Dictionary<string, SubscriptionModel>() { { userId, sub } });
                            }
                        });
                        await Task.WhenAll(new[] { propTask, imgTask, subTask, subscriptionTask });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to fill caches.\n{ex.ToString()}");
                    }
                });
                var user = userService.GetLoggedInAccount();
                if (user != null)
                    RefreshCaches(user.UserId);

                userService.OnLoggedIn += (s, e) =>
                {
                    ClearCaches();
                    RefreshCaches(e.Account.UserId);
                };
                userService.OnLoggedOut += (s, e) => ClearCaches();

                // Register services
                Container.Register<IOrderService>(() => orderService, Lifestyle.Singleton);
                Container.Register<IPropertyService>(() => propertyService, Lifestyle.Singleton);
                Container.Register<IImageService>(() => imageService, Lifestyle.Singleton);
                Container.Register<INotificationService>(() => notifyService, Lifestyle.Singleton);
                Container.Register(typeof(ILogger<>), typeof(DebugLogger<>), Lifestyle.Transient);
                Container.Register<OAuth2Authenticator>(() => authenticator, Lifestyle.Singleton);
                Container.Register<AccountStore>(() => AccountStore.Create(), Lifestyle.Singleton);
                Container.Register<ICurrentUserService>(() => userService, Lifestyle.Singleton);
                Container.Register<IPurchasingService>(() => purchaseService);
                Container.Register<ICacheRefresher>(() => new CacheRefresher(new DebugLogger<CacheRefresher>(), RefreshCaches), Lifestyle.Singleton);
                Container.Register<ISubscriptionService>(() => subService, Lifestyle.Singleton);
                Container.Register<IOrderValidationService, OrderValidationService>();
                Container.Register<IUserService>(() => new UserService(new HttpClient()
                {
                    BaseAddress = new Uri(_userEndpoint)
                }, new DebugLogger<UserService>()), Lifestyle.Singleton);

                // Finish registering created caches
                Container.Register<ICache<PropertyModel>>(() => propertyCache, Lifestyle.Singleton);
                Container.Register<ICache<Order>>(() => orderCache, Lifestyle.Singleton);
                Container.Register<ICache<ImageModel>>(() => imageCache, Lifestyle.Singleton);
                Container.Register<ICache<SubscriptionModel>>(() => subCache, Lifestyle.Singleton);
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
            var userService = Container.GetInstance<ICurrentUserService>();
            userService.OnLoggedIn += (s, e) => Device.BeginInvokeOnMainThread(() => MainPage = new BaseTabPage());
            userService.OnLoggedOut += (s, e) => Device.BeginInvokeOnMainThread(() => MainPage = new LandingPage());

            if (userService.GetLoggedInAccount() == null)
                MainPage = new LandingPage();
            else
                MainPage = new BaseTabPage();
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
