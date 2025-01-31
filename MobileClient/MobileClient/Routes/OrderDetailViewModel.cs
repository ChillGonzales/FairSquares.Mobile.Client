﻿using FairSquares.Measurement.Core.Models;
using FairSquares.Measurement.Core.Utility;
using MobileClient.Authentication;
using MobileClient.Models;
using MobileClient.Services;
using MobileClient.Utilities;
using MobileClient.Utility;
using MobileClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MobileClient.Routes
{
    public class OrderDetailViewModel : INotifyPropertyChanged
    {
        private readonly Models.Order _order;
        private PropertyModel _property;
        private ImageModel _image;
        private readonly ICache<PropertyModel> _propertyCache;
        private readonly ICache<ImageModel> _imageCache;
        private readonly IPropertyService _propertyService;
        private readonly IPageFactory _pageFactory;
        private readonly IImageService _imageService;
        private readonly ICurrentUserService _userService;
        private readonly IToastService _toastService;
        private readonly MainThreadNavigator _nav;
        private readonly Func<string, string, string, Task> _alertAction;
        private RecalculatedPropertyModel _recalculated;
        private bool _loadingAnimRunning;
        private bool _loadingAnimVisible;
        private bool _statusMessageVisible;
        private bool _mainLayoutVisible;
        private string _address;
        private StreamImageSource _imageSource;
        private bool _imageEnabled = true;
        private string _squares;
        private string _predominantPitch;
        private string _area;
        private string _orderId;
        private string _numberOfPitches;
        private ObservableCollection<WasteViewModel> _safetyStockSource;
        private string _submittedDateText;
        private string _statusText;
        private string _statusMessageText;
        private bool _timingDisclaimerVisible;
        private List<string> _roofsSource;
        private int _selectedRoofIndex;
        private bool _roofSelectionVisible;
        private readonly ILogger<OrderDetailViewModel> _logger;
        public readonly ICommand OnAppearingBehavior;

        public OrderDetailViewModel(Models.Order order,
                                    ICache<PropertyModel> propertyCache,
                                    ICache<ImageModel> imgCache,
                                    IPropertyService propService,
                                    IImageService imgService,
                                    IToastService toast,
                                    MainThreadNavigator nav,
                                    IPageFactory pageFactory,
                                    Func<string, string, string, Task> alertAction,
                                    ICurrentUserService userService,
                                    ILogger<OrderDetailViewModel> logger)
        {
            _order = order;
            _propertyCache = propertyCache;
            _imageCache = imgCache;
            _propertyService = propService;
            _pageFactory = pageFactory;
            _imageService = imgService;
            _userService = userService;
            _toastService = toast;
            _nav = nav;
            _alertAction = alertAction;
            _logger = logger;
            OnAppearingBehavior = new Command(async () => await LoadPropertyAndImage());

            // Display message if order isn't fulfilled yet.
            if (!order.Fulfilled)
            {
                StatusMessageVisible = true;
                MainLayoutVisible = false;
                // Have to do this because some dummy decided to store dates in EST
                DateTime displayTime = DateTime.Now;
                if (order.DateReceived != null)
                {
                    var timeInfo = TimeZoneInfo.FindSystemTimeZoneById("America/Detroit");
                    var utc = TimeZoneInfo.ConvertTimeToUtc(order.DateReceived.Value.DateTime);
                    displayTime = TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.Local);
                }
                SubmittedDateText = $"Order #{order.OrderId} was submitted on {displayTime.ToString("dddd, MMMM dd yyyy")}" +
                    $" at {displayTime.ToString("h:mm tt")}";
                if (order.Status == null)
                {
                    order.Status = new StatusModel()
                    {
                        Status = Status.Pending
                    };
                }
                switch (order.Status.Status)
                {
                    case (Status.ActionRequired):
                        StatusMessageText = $"Please respond to the message sent to {_userService.GetLoggedInAccount()?.Email ?? "your logged in email"} to continue with this order.";
                        StatusText = "Action Required";
                        TimingDisclaimerVisible = false;
                        break;
                    default:
                        StatusMessageText = "Your order has been received and is being processed.";
                        StatusText = "Pending";
                        TimingDisclaimerVisible = true;
                        break;
                }
                return;
            }
            else
            {
                StatusMessageVisible = false;
            }
            _property = _propertyCache.Get(order.OrderId);
            _image = _property != null ? _imageCache.Get(_property.OrderId) : null;
            SelectedRoofChangedCommand = new Command(() => SetUIMeasurements(_selectedRoofIndex));
            if ((_property == null || _image == null) && _order.Fulfilled)
            {
                MainLayoutVisible = false;
                StatusMessageVisible = false;
                LoadingAnimVisible = true;
                LoadingAnimRunning = true;
            }
            else
            {
                MainLayoutVisible = true;
                if (_property.Roofs.Count > 1)
                {
                    RoofSelectionVisible = true;
                    RoofsSource = _property.Roofs.Select(x => x.Name).Distinct().ToList();
                    RoofsSource = new[] { "All" }.Concat(RoofsSource).ToList();
                }
                else
                    RoofSelectionVisible = false;
                SetUIMeasurements(0);
            }
        }

        private void SetUIMeasurements(int selectedRoofIndex)
        {
            try
            {
                if (_property == null || _image == null)
                    return;
                // Recalculate roof totals for current property
                CalculateCurrentTotals(_property);

                // Materialize view
                _recalculated = GetViewModelFromProperty(_property, selectedRoofIndex > 0 ?
                    _property.Roofs.FirstOrDefault(x => x.RoofId == (selectedRoofIndex - 1).ToString()) : null);

                // Set GUI and event handlers
                var stream = new StreamImageSource();
                stream.Stream = (x) =>
                {
                    return Task.FromResult<Stream>(new MemoryStream(_image.Image));
                };
                OrderId = $"Order ID: {_order.OrderId}";
                PredominantPitch = $"{_recalculated.CurrentPitch}:12";
                NumberOfPitches = $"Number of Distinct Pitches Measured: {_recalculated.PitchCount}";
                ImageSource = stream;
                Address = StringUtility.RemoveEmptyLines(_property.Address);
                Area = $"{Convert.ToInt64(_recalculated.Roofs.Sum(x => x.TotalArea)).ToString()} sq. ft.";
                Squares = $"Total Squares: {_recalculated.Roofs.Sum(x => x.TotalSquares).ToString()} squares";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to set UI to new measurements.", ex);
            }
            RefreshTableView(_recalculated.Roofs.Sum(x => x.TotalArea));
        }
        private async Task LoadPropertyAndImage()
        {
            try
            {
                if ((_property == null || _image == null) && _order.Fulfilled)
                {
                    // Try to download measurements when order is marked as fulfilled but measurements aren't found.
                    MainLayoutVisible = false;
                    StatusMessageVisible = false;
                    LoadingAnimVisible = true;
                    LoadingAnimRunning = true;
                    if (_property == null)
                    {
                        PropertyModel newModel = null;
                        try
                        {
                            newModel = (await _propertyService.GetProperties(new List<string>() { _order.OrderId })).First().Value;
                        }
                        catch { }
                        if (newModel == null || string.IsNullOrWhiteSpace(newModel.OrderId))
                        {
                            _toastService.LongToast($"Your order is marked as completed but the measurements cannot be found. Please check your internet connection and try again.");
                            _logger.LogError($"Order is marked as completed but no measurements found.", null, $"Order id: {newModel.OrderId}");
                            return;
                        }
                        _propertyCache.Put(newModel.OrderId, newModel);
                        _property = newModel;
                    }
                    if (_image == null)
                    {
                        ImageModel image = null;
                        try
                        {
                            image = _imageService.GetImages(new List<string>() { _order.OrderId }).First().Value;
                        }
                        catch { }
                        if (image == null)
                        {
                            _toastService.LongToast($"Your order is marked as completed but the image cannot be found. Please check your internet connection.");
                            _logger.LogError($"Order is marked as completed but no image found.", null, $"Order id: {_order.OrderId}");
                        }
                        else
                        {
                            _imageCache.Put(_order.OrderId, image);
                            _image = image;
                        }
                    }
                    LoadingAnimVisible = false;
                    LoadingAnimRunning = false;
                    MainLayoutVisible = true;
                    if (_property.Roofs.Count > 1)
                    {
                        RoofSelectionVisible = true;
                        RoofsSource = _property.Roofs.Select(x => x.Name).Distinct().ToList();
                        RoofsSource = new[] { "All" }.Concat(RoofsSource).ToList();
                    }
                    else
                    {
                        RoofSelectionVisible = false;
                    }
                    SetUIMeasurements(0);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load property and image.", ex);
            }
        }
        private void CalculateCurrentTotals(PropertyModel property)
        {
            try
            {
                foreach (var roof in property.Roofs)
                {
                    var response = RoofUtility.CalculateTotals(roof);
                    var rise = RoofUtility.GetPredominantPitchFromSections(roof.Sections);
                    roof.TotalArea = Math.Round(response.TotalArea, 0);
                    roof.TotalSquares = Math.Ceiling(response.TotalSquaresCount);
                    roof.PredominantPitchRise = rise;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to calculate current totals.", ex);
            }
        }
        private RecalculatedPropertyModel GetViewModelFromProperty(PropertyModel property, RoofModel roof = null)
        {
            try
            {
                List<SectionModel> sections = null;
                if (roof != null)
                    sections = roof.Sections;
                else
                    sections = _property.Roofs.SelectMany(x => x.Sections).ToList();
                var overallPitch = RoofUtility.GetPredominantPitchFromSections(sections);
                var count = RoofUtility.GetPitchCount(sections)?.PitchCount ?? 1;
                var recalc = new RecalculatedPropertyModel(property)
                {
                    RecalculatedSections = property.Roofs.SelectMany(x => x.Sections).Where(x => x.PitchRise == overallPitch).ToList(),
                    CurrentPitch = overallPitch,
                    OriginalPitch = overallPitch,
                    PitchCount = count
                };
                recalc.Roofs = recalc.Roofs.Where(x => x.Name == (roof?.Name ?? x.Name)).ToList();
                return recalc;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get view model from property.", ex, property);
                return null;
            }
        }
        private void OnPitchValueChanged(int oldValue, int newValue)
        {
            try
            {
                if (newValue > 35 || newValue < 0)
                    return;
                PredominantPitch = $"{newValue}:12";
                _recalculated.CurrentPitch = newValue;
                foreach (var section in _recalculated.RecalculatedSections)
                {
                    section.PitchRise = newValue;
                    var totals = SectionUtility.CalculateAreaAndSquares(new CalculateSectionModelRequest()
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
                    var totals = RoofUtility.CalculateTotals(roof);
                    roof.TotalArea = totals.TotalArea;
                    roof.TotalSquares = totals.TotalSquaresCount;
                    roof.PredominantPitchRise = RoofUtility.GetPredominantPitchFromSections(roof.Sections);
                }
                Area = $"{Convert.ToInt64(_recalculated.Roofs.Sum(x => x.TotalArea)).ToString()} sq. ft.";
                Squares = $"Total Squares: {Math.Ceiling(_recalculated.Roofs.Sum(x => x.TotalSquares)).ToString()} squares";
                RefreshTableView(_recalculated.Roofs.Sum(x => x.TotalArea));
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to calculate pitch. " + ex.ToString(), ex);
            }
        }
        private void RefreshTableView(double totalArea)
        {
            try
            {
                var pcts = new[] { 0.05, 0.1, 0.15, 0.2 };
                SafetyStockSource = new ObservableCollection<WasteViewModel>(pcts.Select(x => new WasteViewModel()
                {
                    WasteName = $"{x.ToString("P0")} Waste",
                    WasteAmount = $"{Math.Ceiling((totalArea * (1 + x)) / 100).ToString()} Squares"
                }).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to refresh safety stock table view.", ex);
            }
        }

        // Bound properties
        public event PropertyChangedEventHandler PropertyChanged;
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
        public bool StatusMessageVisible
        {
            get
            {
                return _statusMessageVisible;
            }
            set
            {
                _statusMessageVisible = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusMessageVisible)));
            }
        }
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
        public string Address
        {
            get
            {
                return _address;
            }
            set
            {
                _address = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Address)));
            }
        }
        public StreamImageSource ImageSource
        {
            get
            {
                return _imageSource;
            }
            set
            {
                _imageSource = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageSource)));
            }
        }
        public bool ImageEnabled
        {
            get
            {
                return _imageEnabled;
            }
            set
            {
                _imageEnabled = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageEnabled)));
            }
        }
        public ICommand ImageTapCommand => new Command(() =>
        {
            ImageEnabled = false;
            _nav.Push(_pageFactory.GetPage(PageType.ImagePopup, (StreamImageSource)ImageSource));
            ImageEnabled = true;
        });
        public string Squares
        {
            get
            {
                return _squares;
            }
            set
            {
                _squares = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Squares)));
            }
        }
        public ICommand PredomPitchInfoCommand => new Command(async () => await _alertAction("Predominant Pitch Information",
                "The predominant pitch is the pitch with the largest measured area on the roof. \n\n" +
                "For example: A Ranch-style house has a pitch of 4:12 on the main roof, has two dormers that are 6:12 each, and " +
                "an attached garage that is 3:12. The predominant pitch of the house will be 4:12, because that is the pitch that " +
                "covers the most roof area.\n\n" +
                "Predominant pitch is the only pitch that is adjustable with Fair Squares.", "Close"));
        public ICommand DownButtonCommand => new Command(() => OnPitchValueChanged(_recalculated.CurrentPitch, _recalculated.CurrentPitch - 1));
        public string PredominantPitch
        {
            get
            {
                return _predominantPitch;
            }
            set
            {
                _predominantPitch = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PredominantPitch)));
            }
        }
        public ICommand UpButtonCommand => new Command(() => OnPitchValueChanged(_recalculated.CurrentPitch, _recalculated.CurrentPitch + 1));
        public string Area
        {
            get
            {
                return _area;
            }
            set
            {
                _area = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Area)));
            }
        }
        public string OrderId
        {
            get
            {
                return _orderId;
            }
            set
            {
                _orderId = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OrderId)));
            }
        }
        public string NumberOfPitches
        {
            get
            {
                return _numberOfPitches;
            }
            set
            {
                _numberOfPitches = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NumberOfPitches)));
            }
        }
        public ICommand PitchesInfoCommand => new Command(async () => await _alertAction("Pitch Count Information",
                "This is the number of distinct pitches Fair Squares used to measure this roof(s). \n\n" +
                "NOTE: Only the pitch with the largest square footage (the predominant pitch) can be adjusted.", "Close"));
        public ObservableCollection<WasteViewModel> SafetyStockSource
        {
            get
            {
                return _safetyStockSource;
            }
            set
            {
                _safetyStockSource = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SafetyStockSource)));
            }
        }
        public string SubmittedDateText
        {
            get
            {
                return _submittedDateText;
            }
            set
            {
                _submittedDateText = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubmittedDateText)));
            }
        }
        public string StatusText
        {
            get
            {
                return _statusText;
            }
            set
            {
                _statusText = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusText)));
            }
        }
        public string StatusMessageText
        {
            get
            {
                return _statusMessageText;
            }
            set
            {
                _statusMessageText = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusMessageText)));
            }
        }
        public bool TimingDisclaimerVisible
        {
            get
            {
                return _timingDisclaimerVisible;
            }
            set
            {
                _timingDisclaimerVisible = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimingDisclaimerVisible)));
            }
        }
        public List<string> RoofsSource
        {
            get
            {
                return _roofsSource;
            }
            private set
            {
                _roofsSource = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RoofsSource)));
            }
        }
        public int SelectedRoofIndex
        {
            get
            {
                return _selectedRoofIndex;
            }
            set
            {
                if (value == _selectedRoofIndex)
                    return;
                _selectedRoofIndex = value;
                this.SelectedRoofChangedCommand?.Execute(null);
            }
        }
        public bool RoofSelectionVisible
        {
            get
            {
                return _roofSelectionVisible;
            }
            set
            {
                _roofSelectionVisible = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RoofSelectionVisible)));
            }
        }
        public ICommand SelectedRoofChangedCommand { get; private set; }
    }

    public class WasteViewModel
    {
        public string WasteName { get; set; }
        public string WasteAmount { get; set; }
    }
}
