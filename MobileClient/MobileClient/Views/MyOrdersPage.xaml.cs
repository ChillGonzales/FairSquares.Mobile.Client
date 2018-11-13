using MobileClient.Models;
using MobileClient.Services;
using System;
using System.Collections.Generic;
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
        private readonly List<Order> _currentOrders;

        public MyOrdersPage()
        {
            InitializeComponent();
            _orderService = DependencyService.Get<IOrderService>();
            _currentOrders = _orderService.GetMemberOrders("1234").Result.ToList();
            OrderListView.ItemsSource = _currentOrders.Select(x => x.StreetAddress);
        }
    }
}