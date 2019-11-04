using FairSquares.Measurement.Core.Models;
using MobileClient.Authentication;
using MobileClient.Models;
using MobileClient.Routes;
using MobileClient.Services;
using MobileClient.Utilities;
using MobileClient.Utility;
using Newtonsoft.Json;
using Plugin.InAppBilling;
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
        private static string _purchasedReportsEndpoint = @"https://fairsquares-order-management-api.azurewebsites.net/api/purchasedreports";
        private static string _propertyEndpoint = @"https://property-measurements.azurewebsites.net/api/properties";
        private static string _blobEndpoint = @"https://fairsquaresapplogging.blob.core.windows.net/roof-images";
        private const string GoogleAuthorizeUrl = "https://accounts.google.com/o/oauth2/v2/auth";
        private const string GoogleAccessTokenUrl = "https://www.googleapis.com/oauth2/v4/token";
        public const string TopicPrefix = "v1-";
        public static LaunchedFromPushModel PushModel = new LaunchedFromPushModel();
        public static void SendEmail(string body)
        {
            var notify = Container.GetInstance<INotificationService>();
            notify.Notify(new NotificationRequest()
            {
                To = "colin.monroe@fairsquarestech.com",
                Message = $"Push from iOS{Environment.NewLine}{body}",
                MessageType = MessageType.Email,
                From = "test@fairsquarestech.com",
                Subject = "Push Notification Body"
            });
        }

        static App()
        {
            try
            {
                Container = new Container();
                var analyticsSvc = DependencyService.Get<IAnalyticsService>();
                var userService = new CurrentUserService();
                var orderService = new AzureOrderService(new HttpClient(), _orderEndpoint, _apiKey);
                var propertyService = new PropertyService(new HttpClient(), _propertyEndpoint, new AnalyticsLogger<PropertyService>(analyticsSvc, userService));
                var imageService = new BlobImageService(new HttpClient(), _blobEndpoint, new AnalyticsLogger<BlobImageService>(analyticsSvc, userService));
                var subService = new SubscriptionService(new HttpClient(), _subEndpoint, new AnalyticsLogger<SubscriptionService>(analyticsSvc, userService));
                var prService = new PurchasedReportService(new HttpClient(), _purchasedReportsEndpoint, new AnalyticsLogger<PurchasedReportService>(analyticsSvc, userService));
                var authenticator = new OAuth2Authenticator(Configuration.ClientId,
                                                            null,
                                                            Configuration.Scope,
                                                            new Uri(GoogleAuthorizeUrl),
                                                            new Uri(Configuration.RedirectNoPath + ":" + Configuration.RedirectPath),
                                                            new Uri(GoogleAccessTokenUrl),
                                                            null,
                                                            true);
                var notifyService = new NotificationService(new HttpClient(), _notifyEndpoint, _apiKey);
                var purchaseEmailLogger = new EmailLogger<PurchasingService>(notifyService, userService);
                var purchaseService = new PurchasingService(CrossInAppBilling.Current, purchaseEmailLogger);
                authenticator.Completed += (s, e) =>
                {
                    if (e.IsAuthenticated)
                    {
                        userService.LogIn(e.Account);
                    }
                };

                // Setup caches and begin process of filling them.
                var dbBasePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var propertyCache = new LocalSqlCache<PropertyModel>(Path.Combine(dbBasePath, "property.db3"),
                    new AnalyticsLogger<LocalSqlCache<PropertyModel>>(analyticsSvc, userService));
                var orderCache = new LocalSqlCache<Models.Order>(Path.Combine(dbBasePath, "order.db3"),
                    new AnalyticsLogger<LocalSqlCache<Models.Order>>(analyticsSvc, userService));
                var imageCache = new LocalSqlCache<ImageModel>(Path.Combine(dbBasePath, "images.db3"),
                    new AnalyticsLogger<LocalSqlCache<ImageModel>>(analyticsSvc, userService));
                var subCache = new LocalSqlCache<SubscriptionModel>(Path.Combine(dbBasePath, "subs.db3"),
                    new AnalyticsLogger<LocalSqlCache<SubscriptionModel>>(analyticsSvc, userService));
                var settingsCache = new LocalSqlCache<SettingsModel>(Path.Combine(dbBasePath, "sets.db3"),
                    new AnalyticsLogger<LocalSqlCache<SettingsModel>>(analyticsSvc, userService));
                var prCache = new LocalSqlCache<PurchasedReportModel>(Path.Combine(dbBasePath, "purchasedreports.db3"),
                    new AnalyticsLogger<LocalSqlCache<PurchasedReportModel>>(analyticsSvc, userService));

                Action ClearCaches = () =>
                {
                    try
                    {
                        orderCache.Clear();
                        propertyCache.Clear();
                        imageCache.Clear();
                        subCache.Clear();
                        prCache.Clear();
                    }
                    catch { }
                };
                Func<AccountModel, Task> RefreshCaches = user =>
                {
                    var userId = user.UserId;
                    var prTask = Task.Run(() =>
                    {
                        try
                        {
                            var prs = prService.GetPurchasedReports(userId);
                            prCache.Put(prs.ToDictionary(x => x.PurchaseId, x => x));
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Failed to get purchased reports. {ex.ToString()}");
                        }
                    });
                    var orderTask = Task.Run(async () =>
                    {
                        try
                        {
                            var orders = await orderService.GetMemberOrders(userId);
                            var unCached = orders.Except(orderCache.GetAll().Select(x => x.Value).ToList(), new OrderEqualityComparer()).ToList();
                            orderCache.Put(orders.ToDictionary(x => x.OrderId, x => x));
                            var subTask = Task.Run(() =>
                            {
                                try
                                {
                                    DependencyService.Get<IMessagingSubscriber>().Subscribe(orders.Select(x => $"{(Device.RuntimePlatform == Device.Android ? App.TopicPrefix : "")}{x.OrderId}").ToList());
                                }
                                catch { }
                            });
                            var propTask = Task.Run(async () =>
                            {
                                if (!orders.Any())
                                {
                                    propertyCache.Clear();
                                    return;
                                }
                                var properties = await propertyService.GetProperties(unCached.Select(x => x.OrderId).ToList());
                                propertyCache.Update(properties);
                            });
                            var imgTask = Task.Run(() =>
                            {
                                if (!orders.Any())
                                {
                                    imageCache.Clear();
                                    return;
                                }
                                var images = imageService.GetImages(unCached.Select(x => x.OrderId).ToList());
                                imageCache.Update(images);
                            });
                            var subscriptionTask = Task.Run(async () =>
                            {
                                // TODO: Refactor this so it can be tested.
                                var allSubs = subService.GetSubscriptions(userId).OrderBy(x => x.StartDateTime).ToList();
                                var recentSub = allSubs.LastOrDefault();
                                var purchases = new List<InAppBillingPurchase>();
                                SubscriptionModel newSub = null;

                                // Check app store purchases to see if they auto-renewed
                                if (recentSub != null && !SubscriptionUtility.SubscriptionActive(recentSub))
                                {
                                    try
                                    {
                                        purchases = (await purchaseService.GetPurchases(ItemType.Subscription)).ToList();
                                        App.SendEmail(JsonConvert.SerializeObject(purchases));
                                    }
                                    catch (Exception ex)
                                    {
                                        purchaseEmailLogger.LogError($"Error occurred while getting purchases.", ex);
                                    }
                                    var mostRecent = purchases.OrderBy(x => x.TransactionDateUtc)?.LastOrDefault();
                                    if (mostRecent != null)
                                    {
                                        newSub = SubscriptionUtility.GetModelFromIAP(mostRecent, user, recentSub);
                                        App.SendEmail($"Resolved subscription after business logic: '{JsonConvert.SerializeObject(newSub)}'");
                                        if (newSub != null)
                                        {
                                            allSubs.Add(newSub);
                                            subService.AddSubscription(newSub);
                                        }
                                    }
                                }
                                if (!allSubs.Any())
                                {
                                    subCache.Clear();
                                }
                                else
                                {
                                    subCache.Put(allSubs.ToDictionary(x => x.PurchaseId, x => x));
                                }
                            });
                            await Task.WhenAll(new[] { propTask, imgTask, subTask, subscriptionTask });
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Failed to fill caches.\n{ex.ToString()}");
                        }
                    });
                    return Task.WhenAll(new[] { prTask, orderTask });
                };

                var refresher = new CacheRefresher(new AnalyticsLogger<CacheRefresher>(analyticsSvc, userService), RefreshCaches);
                refresher.Invalidate();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                refresher.RefreshCaches(userService.GetLoggedInAccount());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                userService.OnLoggedIn += async (s, e) =>
                {
                    ClearCaches();
                    await refresher.RefreshCaches(e.Account);
                };
                userService.OnLoggedOut += (s, e) => ClearCaches();

                // Register services
                Container.Register<IOrderService>(() => orderService, Lifestyle.Singleton);
                Container.Register<IPropertyService>(() => propertyService, Lifestyle.Singleton);
                Container.Register<IImageService>(() => imageService, Lifestyle.Singleton);
                Container.Register<INotificationService>(() => notifyService, Lifestyle.Singleton);
                Container.Register<ILogger<SingleReportPurchaseViewModel>>(() =>
                    new EmailLogger<SingleReportPurchaseViewModel>(notifyService, userService), Lifestyle.Singleton);
                Container.RegisterConditional(typeof(ILogger<>), typeof(AnalyticsLogger<>), c => !c.Handled);
                Container.Register<OAuth2Authenticator>(() => authenticator, Lifestyle.Singleton);
                Container.Register<AccountStore>(() => AccountStore.Create(), Lifestyle.Singleton);
                Container.Register<ICurrentUserService>(() => userService, Lifestyle.Singleton);
                Container.Register<IPurchasingService>(() => purchaseService, Lifestyle.Singleton);
                Container.Register<ICacheRefresher>(() => refresher, Lifestyle.Singleton);
                Container.Register<ISubscriptionService>(() => subService, Lifestyle.Singleton);
                Container.Register<IOrderValidationService, OrderValidationService>();
                Container.Register<IPageFactory, PageFactory>(Lifestyle.Singleton);
                Container.Register<IToastService>(() => DependencyService.Get<IToastService>(), Lifestyle.Singleton);
                Container.Register<IMessagingSubscriber>(() => DependencyService.Get<IMessagingSubscriber>(), Lifestyle.Singleton);
                Container.Register<IMessagingCenter>(() => MessagingCenter.Instance, Lifestyle.Singleton);
                Container.Register<IPurchasedReportService>(() => prService, Lifestyle.Singleton);
                Container.Register<IAnalyticsService>(() => analyticsSvc, Lifestyle.Singleton);
                Container.Register<LaunchedFromPushModel>(() => App.PushModel ?? new LaunchedFromPushModel(), Lifestyle.Singleton);

                // Finish registering created caches
                Container.Register<ICache<PropertyModel>>(() => propertyCache, Lifestyle.Singleton);
                Container.Register<ICache<Models.Order>>(() => orderCache, Lifestyle.Singleton);
                Container.Register<ICache<ImageModel>>(() => imageCache, Lifestyle.Singleton);
                Container.Register<ICache<SubscriptionModel>>(() => subCache, Lifestyle.Singleton);
                Container.Register<ICache<SettingsModel>>(() => settingsCache, Lifestyle.Singleton);
                Container.Register<ICache<PurchasedReportModel>>(() => prCache, Lifestyle.Singleton);
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
            try
            {
                InitializeComponent();
                MainPage = new BaseTab();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
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
            // Invalidate cache so views know they need to refresh orders
            try
            {
                var refresher = Container.GetInstance<ICacheRefresher>();
                refresher.Invalidate();
                if (Device.RuntimePlatform == Device.iOS)
                    MessagingCenter.Send<App>(this, "CacheInvalidated");
                refresher.Revalidate();
            }
            catch { }
        }
    }
}
