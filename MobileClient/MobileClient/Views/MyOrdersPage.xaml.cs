using MobileClient.Models;
using MobileClient.Services;
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
        private List<Order> _currentOrders;
        private MainPage RootPage { get => Application.Current.MainPage as MainPage; }
        public static IList<OrderGroup> All { private set; get; }

        public MyOrdersPage()
        {
            InitializeComponent();
            try
            {
                _orderService = App.Container.GetInstance<IOrderService>();
                _currentOrders = _orderService.GetMemberOrders(App.MemberId).ToList();
                SetListViewSource(_currentOrders);

                OrderListView.RefreshCommand = new Command(async () =>
                {
                    try
                    {
                        OrderListView.IsRefreshing = true;
                        _currentOrders = await Task.Run(() => _orderService.GetMemberOrders(App.MemberId).ToList());
                        SetListViewSource(_currentOrders);
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
                    await RootPage.NavigateToPage(new NavigationPage(new OrderDetailPage(_currentOrders.FirstOrDefault(x => x.OrderId == id))));
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
            fulGroup.AddRange(_currentOrders.Where(x => x.Fulfilled).Select(x => new OrderViewCell()
            {
                Text = x.StreetAddress.Split('\n')[0],
                TextColor = Color.Black,
                OrderId = x.OrderId
                //Detail = x.Fulfilled ? "Complete" : "Pending",
                //DetailColor = x.Fulfilled ? Color.DarkGreen : Color.DarkBlue
            }));
            var penGroup = new OrderGroup() { Title = "Pending" };
            penGroup.AddRange(_currentOrders.Where(x => !x.Fulfilled).Select(x => new OrderViewCell()
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