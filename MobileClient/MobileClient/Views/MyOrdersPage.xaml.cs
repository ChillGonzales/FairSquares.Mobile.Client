﻿using FairSquares.Measurement.Core.Models;
using MobileClient.Authentication;
using MobileClient.Models;
using MobileClient.Services;
using MobileClient.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyOrdersPage : ContentPage
    {
        private readonly IOrderService _orderService;
        private readonly ICache<Order> _orderCache;
        private readonly ICache<PropertyModel> _propertyCache;
        private readonly ICache<ImageModel> _imageCache;
        private readonly ILogger<MyOrdersPage> _logger;
        private readonly ICacheRefresher _cacheRefresher;
        private readonly ICurrentUserService _userService;
        public static IList<OrderGroup> All { private set; get; }

        public MyOrdersPage()
        {
            InitializeComponent();
            try
            {
                _userService = App.Container.GetInstance<ICurrentUserService>();
                _orderService = App.Container.GetInstance<IOrderService>();
                _orderCache = App.Container.GetInstance<ICache<Order>>();
                _propertyCache = App.Container.GetInstance<ICache<PropertyModel>>();
                _imageCache = App.Container.GetInstance<ICache<ImageModel>>();
                _logger = App.Container.GetInstance<ILogger<MyOrdersPage>>();
                _cacheRefresher = App.Container.GetInstance<ICacheRefresher>();
                SetViewState();
                ExampleReportButton.Clicked += async (s, e) =>
                {
                    try
                    {
                        var order = JsonConvert.DeserializeObject<Order>(Examples.ExampleOrder);
                        _imageCache.Put(order.OrderId, new ImageModel()
                        {
                            OrderId = order.OrderId,
                            Image = Convert.FromBase64String(Examples.ExampleImage)
                        });
                        _propertyCache.Put(order.OrderId, JsonConvert.DeserializeObject<PropertyModel>(Examples.ExampleProperty));
                        await Navigation.PushAsync(new OrderDetailPage(order));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"An error occurred while trying to open example order.", ex);
                    }
                };
                Action refreshAction = async () =>
                {
                    try
                    {
                        OrderListView.IsRefreshing = true;
                        var user = _userService.GetLoggedInAccount();
                        if (user == null)
                            return;
                        await _cacheRefresher.RefreshCaches(user.UserId);
                        SetListViewSource(_orderCache.GetAll().Select(x => x.Value).ToList());
                        OrderListView.IsRefreshing = false;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to refresh order list. \n{ex.ToString()}");
                    }
                };
                _userService.OnLoggedIn += (s, e) => refreshAction();
                OrderListView.RefreshCommand = new Command(refreshAction);
                OrderListView.ItemSelected += async (s, e) =>
                {
                    if (e.SelectedItem == null)
                        return;
                    var id = ((OrderViewCell)e.SelectedItem).OrderId;
                    await Navigation.PushAsync(new OrderDetailPage(_orderCache.Get(id)));
                    OrderListView.SelectedItem = null;
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to initialize orders page. \n{ex.ToString()}");
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            SetViewState();
        }

        private void SetViewState()
        {
            var orders = _orderCache.GetAll().Select(x => x.Value).ToList();
            var anyOrders = orders.Any();
            MainLayout.IsVisible = anyOrders;
            NoOrderLayout.IsVisible = !anyOrders;
            SetListViewSource(orders);
        }

        private void SetListViewSource(List<Order> orders)
        {
            var fulGroup = new OrderGroup() { Title = "Completed" };
            fulGroup.AddRange(_orderCache.GetAll().Select(x => x.Value).Where(x => x.Fulfilled).Select(x => new OrderViewCell()
            {
                Text = x.StreetAddress.Split('\n')[0],
                TextColor = Color.Black,
                OrderId = x.OrderId
            }));
            var penGroup = new OrderGroup() { Title = "Pending" };
            penGroup.AddRange(_orderCache.GetAll().Select(x => x.Value).Where(x => !x.Fulfilled).Select(x => new OrderViewCell()
            {
                Text = x.StreetAddress.Split('\n')[0],
                TextColor = Color.Black,
                OrderId = x.OrderId
            }));
            All = new List<OrderGroup>() { fulGroup, penGroup };
            OrderListView.ItemsSource = All;
        }

        private async void ToolbarItem_Activated(object sender, EventArgs e)
        {
            await this.Navigation.PushAsync(new InstructionPage(null, false));
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