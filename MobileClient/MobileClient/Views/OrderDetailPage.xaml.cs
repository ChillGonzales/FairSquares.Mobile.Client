using FairSquares.Measurement.Core.Models;
using MobileClient.Models;
using MobileClient.Utilities;
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
    public partial class OrderDetailPage : ContentPage
    {
        private Order _order;
        private PropertyModel _property;
        private ICache<PropertyModel> _propertyCache;

        public OrderDetailPage()
        {
            InitializeComponent();
        }
        public OrderDetailPage(Order order)
        {
            InitializeComponent();
            _propertyCache = App.Container.GetInstance<ICache<PropertyModel>>();
            _order = order;
            OrderId.Text = $"Order #{order.OrderId}";
            var prop = _propertyCache.GetAll().FirstOrDefault(x => x.Value.OrderId == order.OrderId).Value;
            if (prop == null)
            {
                // TODO: Handle error of no property found
            }
            _property = prop;
            Address.Text = $"Address: {_property.Address}";
            Area.Text = $"Total Area: {_property.Roofs.First().TotalArea.ToString()} sq. ft.";
            Squares.Text = $"Total Squares: {_property.Roofs.First().TotalSquares.ToString()} squares";
        }
    }
}