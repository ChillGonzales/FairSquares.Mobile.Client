using FairSquares.Measurement.Core.Models;
using FairSquares.Measurement.Core.Utilities;
using MobileClient.Models;
using MobileClient.Services;
using MobileClient.Utilities;
using MobileClient.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private RecalculatedPropertyModel _recalculated;
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

            // Recalculate roof totals for current property
            CalculateCurrentTotals(_property);

            // Materialize view
            _recalculated = GetViewModelFromProperty(_property);

            Pitch.Text = $"Pitch: {_recalculated.CurrentPitch}:12";
            PitchStepper.Value = _recalculated.CurrentPitch;
            PitchStepper.ValueChanged += OnPitchStepperValueChanged;

            // TODO: refactor this and cache images
            var serv = App.Container.GetInstance<IImageService>();
            var img = serv.GetImages(new List<string>() { prop.OrderId });
            var stream = new StreamImageSource();
            stream.Stream = (x) =>
            {
                return Task.FromResult<Stream>(new MemoryStream(img.First().Value));
            };
            TopImage.Source = stream;
            Address.Text = $"{Regex.Replace(_property.Address, @"\n\n", @"\n")}";
            Area.Text = $"Total Area: {_property.Roofs.Sum(x => x.TotalArea).ToString()} sq. ft.";
            Squares.Text = $"Total Squares: {_property.Roofs.Sum(x => x.TotalSquares).ToString()} squares";
        }

        private void CalculateCurrentTotals(PropertyModel property)
        {
            foreach (var roof in property.Roofs)
            {
                var response = RoofUtilities.CalculateTotals(roof);
                var rise = RoofUtilities.GetPredominantPitchFromSections(roof.Sections);
                roof.TotalArea = Math.Round(response.TotalArea, 0);
                roof.TotalSquares = Math.Round(response.TotalSquaresCount, 0);
                roof.PredominantPitchRise = rise;
            }
        }
        private RecalculatedPropertyModel GetViewModelFromProperty(PropertyModel property)
        {
            var overallPitch = RoofUtilities.GetPredominantPitchFromSections(_property.Roofs.SelectMany(x => x.Sections).ToList());
            return new RecalculatedPropertyModel(property)
            {
                RecalculatedSections = property.Roofs.SelectMany(x => x.Sections).Where(x => x.PitchRise == overallPitch).ToList(),
                CurrentPitch = overallPitch,
                OriginalPitch = overallPitch
            };
        }
        private void OnPitchStepperValueChanged(object sender, ValueChangedEventArgs e)
        {
            try
            {
                var newVal = Math.Round(e.NewValue, 0);
                Pitch.Text = $"Pitch: {newVal}:12";
                _recalculated.CurrentPitch = (int)newVal;
                foreach (var section in _recalculated.RecalculatedSections)
                {
                    section.PitchRise = newVal;
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
                foreach (var roof in _recalculated.Roofs)
                {
                    var totals = RoofUtilities.CalculateTotals(roof);
                    roof.TotalArea = totals.TotalArea;
                    roof.TotalSquares = totals.TotalSquaresCount;
                    roof.PredominantPitchRise = RoofUtilities.GetPredominantPitchFromSections(roof.Sections);
                }
                Area.Text = $"Total Area: {Math.Round(_recalculated.Roofs.Sum(x => x.TotalArea), 2).ToString()} sq. ft.";
                Squares.Text = $"Total Squares: {Math.Ceiling(_recalculated.Roofs.Sum(x => x.TotalSquares)).ToString()} squares";
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to calculate pitch. " + ex.ToString());
            }
        }
    }
}