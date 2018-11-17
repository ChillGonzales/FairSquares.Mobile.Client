using MobileClient.Models;
using MobileClient.Services;
using MobileClient.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private MainPage RootPage { get => Application.Current.MainPage as MainPage; }
        public static IList<OrderGroup> All { private set; get; }

        public MyOrdersPage()
        {
            InitializeComponent();
            try
            {
                _orderService = App.Container.GetInstance<IOrderService>();
                _orderCache = App.Container.GetInstance<ICache<Order>>();
                SetListViewSource(_orderCache.GetAll().Select(x => x.Value).ToList());

                OrderListView.RefreshCommand = new Command(async () =>
                {
                    try
                    {
                        OrderListView.IsRefreshing = true;
                        _orderCache.Put(await Task.Run(() => _orderService.GetMemberOrders(App.MemberId).ToDictionary(x => x.OrderId, x => x)));
                        SetListViewSource(_orderCache.GetAll().Select(x => x.Value).ToList());
                        OrderListView.IsRefreshing = false;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }
                });

                OrderListView.ItemSelected += async (s, e) =>
                {
                    var id = ((OrderViewCell)e.SelectedItem).OrderId;
                    await Navigation.PushAsync(new OrderDetailPage(_orderCache.Get(id)));
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void SetListViewSource(List<Order> orders)
        {
            var fulGroup = new OrderGroup() { Title = "Completed" };
            fulGroup.AddRange(_orderCache.GetAll().Select(x => x.Value).Where(x => x.Fulfilled).Select(x => new OrderViewCell()
            {
                Text = x.StreetAddress.Split('\n')[0],
                TextColor = Color.Black,
                OrderId = x.OrderId
                //Detail = x.Fulfilled ? "Complete" : "Pending",
                //DetailColor = x.Fulfilled ? Color.DarkGreen : Color.DarkBlue
            }));
            var penGroup = new OrderGroup() { Title = "Pending" };
            penGroup.AddRange(_orderCache.GetAll().Select(x => x.Value).Where(x => !x.Fulfilled).Select(x => new OrderViewCell()
            {
                Text = x.StreetAddress.Split('\n')[0],
                TextColor = Color.Black,
                OrderId = x.OrderId
                //Detail = x.Fulfilled ? "Complete" : "Pending",
                //DetailColor = x.Fulfilled ? Color.DarkGreen : Color.DarkBlue
            }));
            All = new List<OrderGroup>() { fulGroup, penGroup };
            OrderListView.ItemsSource = All;
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