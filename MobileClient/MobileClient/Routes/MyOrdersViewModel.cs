using FairSquares.Measurement.Core.Models;
using MobileClient.Authentication;
using MobileClient.Models;
using MobileClient.Services;
using MobileClient.Utilities;
using MobileClient.Utility;
using MobileClient.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MobileClient.Routes
{
    public class MyOrdersViewModel : INotifyPropertyChanged
    {
        private readonly IOrderService _orderService;
        private readonly ICache<Models.Order> _orderCache;
        private readonly ICache<PropertyModel> _propertyCache;
        private readonly ICache<ImageModel> _imageCache;
        private readonly ILogger<MyOrdersViewModel> _logger;
        private readonly Action<Action> _uiInvoke;
        private readonly ICacheRefresher _cacheRefresher;
        private readonly IOrderValidationService _validationService;
        private readonly IPageFactory _pageFactory;
        private readonly ICurrentUserService _userService;
        private readonly IMessagingCenter _messagingCenter;
        private readonly MainThreadNavigator _nav;
        private readonly Action<BaseNavPageType> _baseNavAction;
        private bool _loadingLayoutVisible;
        private bool _loadingAnimVisible;
        private bool _loadingAnimRunning;
        private bool _noOrderLayoutVisible;
        private bool _loginLayoutVisible;
        private bool _freeReportLayoutVisible;
        private bool _mainLayoutVisible;
        private bool _orderListRefreshing;
        private List<OrderGroup> _ordersSource;
        private OrderViewCell _orderListSelectedItem;
        public ICommand OnAppearingBehavior;
        private bool _pmEventSubscribed;

        public MyOrdersViewModel(IOrderService orderSvc,
                                 ICache<Models.Order> orderCache,
                                 ICache<PropertyModel> propertyCache,
                                 ICache<ImageModel> imageCache,
                                 ILogger<MyOrdersViewModel> logger,
                                 ICacheRefresher cacheRefresher,
                                 IOrderValidationService validator,
                                 IPageFactory pageFactory,
                                 ICurrentUserService userService,
                                 LaunchedFromPushModel pushModel,
                                 MainThreadNavigator nav,
                                 IMessagingCenter messagingCenter,
                                 Action<Action> uiInvoke,
                                 Action<BaseNavPageType> baseNavAction)
        {
            _orderService = orderSvc;
            _orderCache = orderCache;
            _propertyCache = propertyCache;
            _imageCache = imageCache;
            _logger = logger;
            _uiInvoke = uiInvoke;
            _cacheRefresher = cacheRefresher;
            _validationService = validator;
            _pageFactory = pageFactory;
            _userService = userService;
            _messagingCenter = messagingCenter;
            _nav = nav;
            _baseNavAction = baseNavAction;
            ExampleReportCommand = new Command(() =>
            {
                try
                {
                    var order = JsonConvert.DeserializeObject<Models.Order>(Examples.ExampleOrder);
                    _imageCache.Put(order.OrderId, new ImageModel()
                    {
                        OrderId = order.OrderId,
                        Image = Convert.FromBase64String(Examples.ExampleImage)
                    });
                    _propertyCache.Put(order.OrderId, JsonConvert.DeserializeObject<PropertyModel>(Examples.ExampleProperty));
                    _nav.Push(_pageFactory.GetPage(PageType.OrderDetail, order));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occurred while trying to open example order.", ex);
                }
            });
            Action refreshAction = async () =>
            {
                try
                {
                    OrderListRefreshing = true;
                    var fresh = await _orderService.GetMemberOrders(_userService.GetLoggedInAccount()?.UserId);
                    _orderCache.Put(fresh.ToDictionary(x => x.OrderId, x => x));
                    SetListViewSource(fresh.ToList());
                    OrderListRefreshing = false;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to refresh order list.", ex);
                }
            };
            OrderListRefreshCommand = new Command(refreshAction);
            _messagingCenter.Subscribe<App>(this, "CacheInvalidated", async x =>
            {
                await this.SetViewState();
            });
            OnAppearingBehavior = new Command(async () =>
            {
                if (!_pmEventSubscribed)
                {
                    _pmEventSubscribed = true;
                    pushModel.PropertyChanged += async (s, e) =>
                    {
                        if (!string.IsNullOrWhiteSpace(pushModel.OrderId))
                        {
                            var order = await _orderService.GetOrder(pushModel.OrderId);
                            _orderCache.Put(order.OrderId, order);
                            _nav.Push(_pageFactory.GetPage(PageType.OrderDetail, order));
                        }
                    };
                }
                await SetViewState();
            });
        }

        private async Task SetViewState()
        {
            var orders = new List<Models.Order>();
            try
            {
                var user = _userService.GetLoggedInAccount();
                if (user != null)
                {
                    if (_cacheRefresher.Invalidated)
                    {
                        MainLayoutVisible = false;
                        FreeReportLayoutVisible = false;
                        LoginLayoutVisible = false;
                        NoOrderLayoutVisible = false;
                        LoadingLayoutVisible = true;
                        LoadingAnimVisible = true;
                        LoadingAnimRunning = true;
                        // Just refresh order cache if cache refresh hasn't been started yet.
                        if (_cacheRefresher.RefreshTask == null || _cacheRefresher.RefreshTask.IsCompleted)
                        {
                            var fresh = await _orderService.GetMemberOrders(user.UserId);
                            _orderCache.Put(fresh.ToDictionary(x => x.OrderId, x => x));
                        }
                        else
                        {
                            await _cacheRefresher.RefreshTask;
                        }
                        MainLayoutVisible = true;
                        LoadingLayoutVisible = false;
                        LoadingAnimRunning = false;
                        LoadingAnimVisible = false;
                    }
                    orders = _orderCache.GetAll().Select(x => x.Value).ToList();
                    var anyOrders = orders.Any();
                    MainLayoutVisible = anyOrders;
                    NoOrderLayoutVisible = !anyOrders;
                    var validation = await _validationService.ValidateOrderRequest(user);
                    FreeReportLayoutVisible = validation.State == ValidationState.FreeReportValid;
                    LoginLayoutVisible = false;
                }
                else
                {
                    FreeReportLayoutVisible = false;
                    MainLayoutVisible = false;
                    LoginLayoutVisible = true;
                    NoOrderLayoutVisible = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to set view state. {ex.Message}", ex);
            }
            finally
            {
                LoadingLayoutVisible = false;
                LoadingAnimRunning = false;
                LoadingAnimVisible = false;
            }
            SetListViewSource(orders);
        }

        private void SetListViewSource(List<Models.Order> orders)
        {
            try
            {
                var fulGroup = new OrderGroup() { Title = "Completed" };
                fulGroup.AddRange(orders.Where(x => x.Fulfilled).Select(x => new OrderViewCell()
                {
                    Text = $"(#{x.OrderId}) {x.StreetAddress.Split('\n')[0]}",
                    TextColor = Color.Black,
                    OrderId = x.OrderId
                }));
                var penGroup = new OrderGroup() { Title = "Pending" };
                penGroup.AddRange(orders.Where(x => !x.Fulfilled).Select(x => new OrderViewCell()
                {
                    Text = $"(#{x.OrderId}) {x.StreetAddress.Split('\n')[0]}",
                    TextColor = Color.Black,
                    OrderId = x.OrderId
                }));
                _uiInvoke(() => OrdersSource = new List<OrderGroup>() { fulGroup, penGroup });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to set list view source", ex);
            }
        }

        private void HandleSelectedItemChange(OrderViewCell selected)
        {
            try
            {
                if (selected == null)
                    return;
                var id = selected.OrderId;
                _nav.Push(_pageFactory.GetPage(PageType.OrderDetail, _orderCache.Get(id)));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to handle selected item change event.", ex);
            }
            OrderListSelectedItem = null;
        }

        // Bound properties
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand ToolbarInfoCommand => new Command(() => _nav.Push(_pageFactory.GetPage(PageType.Instruction, false)));
        public bool LoadingLayoutVisible
        {
            get
            {
                return _loadingLayoutVisible;
            }
            set
            {
                _loadingLayoutVisible = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LoadingLayoutVisible)));
            }
        }
        public bool LoadingAnimVisible
        {
            get
            {
                return _loadingAnimVisible;
            }
            set
            {
                _loadingAnimVisible = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LoadingAnimVisible)));
            }
        }
        public bool LoadingAnimRunning
        {
            get
            {
                return _loadingAnimRunning;
            }
            set
            {
                _loadingAnimRunning = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LoadingAnimRunning)));
            }
        }
        public bool NoOrderLayoutVisible
        {
            get
            {
                return _noOrderLayoutVisible;
            }
            set
            {
                _noOrderLayoutVisible = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NoOrderLayoutVisible)));
            }
        }
        public bool LoginLayoutVisible
        {
            get
            {
                return _loginLayoutVisible;
            }
            set
            {
                _loginLayoutVisible = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LoginLayoutVisible)));
            }
        }
        public ICommand LoginCommand => new Command(() => _nav.Push(_pageFactory.GetPage(PageType.Landing)));
        public bool FreeReportLayoutVisible
        {
            get
            {
                return _freeReportLayoutVisible;
            }
            set
            {
                _freeReportLayoutVisible = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FreeReportLayoutVisible)));
            }
        }
        public ICommand FreeReportCommand => new Command(() => _baseNavAction(BaseNavPageType.Order));
        public ICommand ExampleReportCommand { get; set; }
        public bool MainLayoutVisible
        {
            get
            {
                return _mainLayoutVisible;
            }
            set
            {
                _mainLayoutVisible = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MainLayoutVisible)));
            }
        }
        public bool OrderListRefreshing
        {
            get
            {
                return _orderListRefreshing;
            }
            set
            {
                _orderListRefreshing = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OrderListRefreshing)));
            }
        }
        public List<OrderGroup> OrdersSource
        {
            get
            {
                return _ordersSource;
            }
            set
            {
                _ordersSource = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OrdersSource)));
            }
        }
        public ICommand OrderListRefreshCommand { get; set; }
        public OrderViewCell OrderListSelectedItem
        {
            get
            {
                return _orderListSelectedItem;
            }
            set
            {
                if (value?.OrderId == _orderListSelectedItem?.OrderId)
                    return;
                _orderListSelectedItem = value;
                HandleSelectedItemChange(value);
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OrderListSelectedItem)));
            }
        }
    }

    public class OrderViewCell
    {
        public string Image { get; set; }
        public string Text { get; set; }
        public Color TextColor { get; set; }
        public string Detail { get; set; }
        public Color DetailColor { get; set; }
        public string OrderId { get; set; }
    }
    public class OrderGroup : List<OrderViewCell>
    {
        public string Title { get; set; }
    }
}
