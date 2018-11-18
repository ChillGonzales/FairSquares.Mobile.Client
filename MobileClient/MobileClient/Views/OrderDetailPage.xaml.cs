using FairSquares.Measurement.Core.Models;
using FairSquares.Measurement.Core.Utilities;
using MobileClient.Models;
using MobileClient.Services;
using MobileClient.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly ILogger<OrderDetailPage> _logger;

        public OrderDetailPage()
        {
            InitializeComponent();
        }
        public OrderDetailPage(Order order)
        {
            InitializeComponent();
            _logger = App.Container.GetInstance<ILogger<OrderDetailPage>>();
            _propertyCache = App.Container.GetInstance<ICache<PropertyModel>>();
            _order = order;
            OrderId.Text = $"Order #{order.OrderId}";
            var prop = _propertyCache.GetAll().FirstOrDefault(x => x.Value.OrderId == order.OrderId).Value;
            if (prop == null)
            {
                // TODO: Handle error of no property found
                _logger.LogError($"Property with order id '{order.OrderId}' was not found in cache.");
                return;
            }
            _property = prop;
            foreach (var roof in _property.Roofs)
            {
                var response = RoofUtilities.CalculateTotals(roof);
                var rise = RoofUtilities.GetPredominantPitchFromSections(roof.Sections);
                roof.TotalArea = Math.Round(response.TotalArea, 0);
                roof.TotalSquares = Math.Round(response.TotalSquaresCount, 0);
                roof.PredominantPitchRise = rise;
            }
            var serv = App.Container.GetInstance<IImageService>();
            var img = serv.GetImages(new List<string>() { prop.OrderId }).Result;
            var stream = new StreamImageSource();
            stream.Stream = (x) =>
            {
                return Task.FromResult<Stream>(new MemoryStream(img.First().Value));
            };
            Address.Text = $"Address: {_property.Address}";
            Area.Text = $"Total Area: {_property.Roofs.First().TotalArea.ToString()} sq. ft.";
            Squares.Text = $"Total Squares: {_property.Roofs.First().TotalSquares.ToString()} squares";
        }
    }
}