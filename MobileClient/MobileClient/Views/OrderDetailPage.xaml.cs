using FairSquares.Measurement.Core.Models;
using FairSquares.Measurement.Core.Utilities;
using MobileClient.Models;
using MobileClient.Services;
using MobileClient.Utilities;
using Newtonsoft.Json;
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
            var overallPitch = RoofUtilities.GetPredominantPitchFromSections(_property.Roofs.SelectMany(x => x.Sections).ToList());
            PitchSlider.Value = overallPitch;
            PitchSlider.ValueChanged += OnPitchSliderValueChanged;
            var serv = App.Container.GetInstance<IImageService>();
            var img = serv.GetImages(new List<string>() { prop.OrderId });
            var stream = new StreamImageSource();
            stream.Stream = (x) =>
            {
                return Task.FromResult<Stream>(new MemoryStream(img.First().Value));
            };
            TopImage.Source = stream;
            Address.Text = $"Address: {_property.Address}";
            Area.Text = $"Total Area: {_property.Roofs.Sum(x => x.TotalArea).ToString()} sq. ft.";
            Squares.Text = $"Total Squares: {_property.Roofs.Sum(x => x.TotalSquares).ToString()} squares";
        }
        private async void OnPitchSliderValueChanged(object sender, ValueChangedEventArgs e)
        {
            try
            {
                var copy = JsonConvert.DeserializeObject<PropertyModel>(JsonConvert.SerializeObject(_property));
                var sections = copy.Roofs.GroupJoin(copy.Roofs.SelectMany(x => x.Sections),
                    x => x.PredominantPitchRise,
                    y => (int)y.PitchRise,
                    (x, y) => new
                    {
                        Pitch = x.PredominantPitchRise,
                        Sections = y
                    }).Where(x => x.Pitch == e.OldValue).SelectMany(x => x.Sections);
                foreach (var section in sections)
                {
                    section.PitchRise = e.NewValue;
                    var totals = SectionUtilities.CalculateAreaAndSquares(new CalculateSectionModelRequest()
                    {
                        Length = section.Length,
                        NumberOfSections = section.NumberOfSections,
                        PitchRise = section.PitchRise,
                        PitchRun = section.PitchRun,
                        SecondLength = section.SecondLength,
                        ShapeType = section.ShapeType,
                        Width = section.Width
                    });
                    section.CalculatedX = totals.CalculatedX;
                    section.Area = totals.Area;
                    section.SquaresCount = totals.SquaresCount;
                }
                foreach (var roof in copy.Roofs)
                {
                    var totals = RoofUtilities.CalculateTotals(roof);
                    roof.TotalArea = totals.TotalArea;
                    roof.TotalSquares = totals.TotalSquaresCount;
                }
                Area.Text = $"Total Area: {copy.Roofs.Sum(x => x.TotalArea).ToString()} sq. ft.";
                Area.Text = $"Total Squares: {copy.Roofs.Sum(x => x.TotalSquares).ToString()} sq. ft.";
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}