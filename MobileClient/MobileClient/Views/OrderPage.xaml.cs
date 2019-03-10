using FairSquares.Measurement.Core.Models;
using MobileClient.Authentication;
using MobileClient.Models;
using MobileClient.Services;
using MobileClient.Utilities;
using MobileClient.ViewModels;
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
    public partial class OrderPage : ContentPage
    {
        private BaseTabPage RootPage { get => Application.Current.MainPage as BaseTabPage; }
        private readonly int _errorIndex = 7;
        private readonly IOrderValidationService _orderValidator;
        private readonly ICurrentUserService _userService;
        private readonly IOrderService _orderService;
        private readonly IAlertService _toast;
        private readonly ICache<Order> _orderCache;
        private PurchasePage _puchasePage;

        public OrderPage()
        {
            InitializeComponent();
            _userService = App.Container.GetInstance<ICurrentUserService>();
            _orderService = App.Container.GetInstance<IOrderService>();
            _orderCache = App.Container.GetInstance<ICache<Order>>();
            _orderValidator = App.Container.GetInstance<IOrderValidationService>();
            _toast = DependencyService.Get<IAlertService>();
            Grid.RowDefinitions[_errorIndex].Height = 0;
            StatePicker.ItemsSource = States.Select(x => x.Text).ToList();
            OptionPicker.ItemsSource = Options.Select(x => x.Text).ToList();
            OptionPicker.SelectedIndex = 0;
            SubmitButton.Clicked += async (s, e) => await HandleSubmitClick(s, e);
            SetVisualStateForValidation();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await SetVisualStateForValidation();
        }

        private async Task SetVisualStateForValidation()
        {
            var validation = await _orderValidator.ValidateOrderRequest(_userService.GetLoggedInAccount());
            if (!validation.Success && validation.UserHasSubscription)
            {
                MainLayout.IsVisible = false;
                CannotSubmitLabel.Text = $"Sorry, you have used all of your reports for this month.";
                CannotSubmitLayout.IsVisible = true;
                return;
            }
            if (!validation.Success && !validation.UserHasSubscription)
            {
                MainLayout.IsVisible = false;
                CannotSubmitLabel.Text = $"Please purchase a subscription before continuing.";
                CannotSubmitLayout.IsVisible = true;
                return;
            }
            MainLayout.IsVisible = true;
            CannotSubmitLayout.IsVisible = false;
        }

        private async Task HandleSubmitClick(object sender, EventArgs e)
        {
            try
            {
                SubmitButton.IsEnabled = false;
                ErrorMessage.Text = "";
                if (string.IsNullOrWhiteSpace(AddressLine1.Text)
                    || string.IsNullOrWhiteSpace(City.Text)
                    || StatePicker.SelectedIndex < 0
                    || string.IsNullOrWhiteSpace(Zip.Text))
                {
                    Grid.RowDefinitions[_errorIndex].Height = GridLength.Star;
                    ErrorMessage.Text = "Please fill out all fields before submitting.";
                    SubmitButton.IsEnabled = true;
                    return;
                }
                var user = _userService.GetLoggedInAccount();
                if (user == null)
                {
                    Grid.RowDefinitions[_errorIndex].Height = GridLength.Star;
                    ErrorMessage.Text = "You must be logged in to submit an order.";
                    SubmitButton.IsEnabled = true;
                    return;
                }

                var validation = await _orderValidator.ValidateOrderRequest(_userService.GetLoggedInAccount());
                // Show purchase page once for very first order or every time if they don't have a subscription
                if ((validation.Success && !validation.UserHasSubscription && _puchasePage == null) ||
                    (!validation.Success) && !validation.UserHasSubscription)
                {
                    SubmitButton.IsEnabled = true;
                    _puchasePage = new PurchasePage(validation.Success);
                    await Navigation.PushAsync(_puchasePage);
                    return;
                }
                if (!validation.Success && validation.UserHasSubscription)
                {
                    SubmitButton.IsEnabled = true;
                    Grid.RowDefinitions[_errorIndex].Height = GridLength.Star;
                    ErrorMessage.Text = "You have used all of the orders you purchased in your subscription. Please upgrade your subscription for more orders.";
                    return;
                }

                // Submit order
                var newOrder = new Models.Order()
                {
                    StreetAddress = $"{AddressLine1.Text}\n{(string.IsNullOrWhiteSpace(AddressLine2.Text) ? "" : AddressLine2.Text + "\n")}\n" +
                                    $"{City.Text}, {States[StatePicker.SelectedIndex].Code} {Zip.Text}",
                    ReportType = ReportType.Basic,
                    MemberId = user.UserId,
                    MemberEmail = user.Email,
                    RoofOption = Options[OptionPicker.SelectedIndex].RoofOption,
                    Comments = Comments.Text
                };
                newOrder.OrderId = await _orderService.AddOrder(newOrder);
                DependencyService.Get<IMessagingSubscriber>().Subscribe(new List<string>() { newOrder.OrderId });
                _orderCache.Put(newOrder.OrderId, newOrder);
                _toast.ShortAlert($"Your address has been submitted!");
                // Clear all fields
                AddressLine1.Text = "";
                AddressLine2.Text = "";
                City.Text = "";
                Grid.RowDefinitions[_errorIndex].Height = 0;
                StatePicker.SelectedIndex = -1;
                OptionPicker.SelectedIndex = 0;
                Zip.Text = "";
                SubmitButton.IsEnabled = true;
                Comments.Text = "";
                RootPage.NavigateFromMenu(PageType.MyOrders);
            }
            catch (Exception ex)
            {
                Grid.RowDefinitions[_errorIndex].Height = GridLength.Star;
                SubmitButton.IsEnabled = true;
                ErrorMessage.Text = $"Failed to submit order with error {ex.ToString()}";
                Debug.WriteLine("Failed to submit order with error '" + ex.ToString() + "'");
            }
        }

        private async void ToolbarItem_Activated(object sender, EventArgs e)
        {
            await this.Navigation.PushAsync(new InstructionPage(null, false));
        }

        private readonly List<OptionViewModel> Options = new List<OptionViewModel>()
        {
            new OptionViewModel() { RoofOption = Models.RoofOption.PrimaryOnly, Text = "Primary Roof Only" },
            new OptionViewModel() { RoofOption = Models.RoofOption.RoofDetachedGarage, Text = "Include Detached Garage" },
            new OptionViewModel() { RoofOption = Models.RoofOption.RoofShedBarn, Text = "Include Barn/Shed" }
        };
        private readonly List<StateViewModel> States = new List<StateViewModel>()
        {
            new StateViewModel() {Code = "AL", Text = "Alabama"},
            new StateViewModel() {Code = "AK", Text = "Alaska"},
            new StateViewModel() {Code = "AZ", Text = "Arizona"},
            new StateViewModel() {Code = "AR", Text = "Arkansas"},
            new StateViewModel() {Code = "CA", Text = "California"},
            new StateViewModel() {Code = "CO", Text = "Colorado"},
            new StateViewModel() {Code = "CT", Text = "Connecticut"},
            new StateViewModel() {Code = "DE", Text = "Delaware"},
            new StateViewModel() {Code = "FL", Text = "Florida"},
            new StateViewModel() {Code = "GA", Text = "Georgia"},
            new StateViewModel() {Code = "HI", Text = "Hawaii"},
            new StateViewModel() {Code = "ID", Text = "Idaho"},
            new StateViewModel() {Code = "IL", Text = "Illinois"},
            new StateViewModel() {Code = "IN", Text = "Indiana"},
            new StateViewModel() {Code = "IA", Text = "Iowa"},
            new StateViewModel() {Code = "KS", Text = "Kansas"},
            new StateViewModel() {Code = "KY", Text = "Kentucky"},
            new StateViewModel() {Code = "LA", Text = "Louisiana"},
            new StateViewModel() {Code = "ME", Text = "Maine"},
            new StateViewModel() {Code = "MD", Text = "Maryland"},
            new StateViewModel() {Code = "MA", Text = "Massachusetts"},
            new StateViewModel() {Code = "MI", Text = "Michigan"},
            new StateViewModel() {Code = "MN", Text = "Minnesota"},
            new StateViewModel() {Code = "MS", Text = "Mississippi"},
            new StateViewModel() {Code = "MO", Text = "Missouri"},
            new StateViewModel() {Code = "MT", Text = "Montana"},
            new StateViewModel() {Code = "NE", Text = "Nebraska"},
            new StateViewModel() {Code = "NV", Text = "Nevada"},
            new StateViewModel() {Code = "NH", Text = "New Hampshire"},
            new StateViewModel() {Code = "NJ", Text = "New Jersey"},
            new StateViewModel() {Code = "NM", Text = "New Mexico"},
            new StateViewModel() {Code = "NY", Text = "New York"},
            new StateViewModel() {Code = "NC", Text = "North Carolina"},
            new StateViewModel() {Code = "ND", Text = "North Dakota"},
            new StateViewModel() {Code = "OH", Text = "Ohio"},
            new StateViewModel() {Code = "OK", Text = "Oklahoma"},
            new StateViewModel() {Code = "OR", Text = "Oregon"},
            new StateViewModel() {Code = "PA", Text = "Pennsylvania"},
            new StateViewModel() {Code = "RI", Text = "Rhode Island"},
            new StateViewModel() {Code = "SC", Text = "South Carolina"},
            new StateViewModel() {Code = "SD", Text = "South Dakota"},
            new StateViewModel() {Code = "TN", Text = "Tennessee"},
            new StateViewModel() {Code = "TX", Text = "Texas"},
            new StateViewModel() {Code = "UT", Text = "Utah"},
            new StateViewModel() {Code = "VT", Text = "Vermont"},
            new StateViewModel() {Code = "VA", Text = "Virginia"},
            new StateViewModel() {Code = "WA", Text = "Washington"},
            new StateViewModel() {Code = "WV", Text = "West Virginia"},
            new StateViewModel() {Code = "WI", Text = "Wisconsin"},
            new StateViewModel() {Code = "WY", Text = "Wyoming"}
        };
    }
}