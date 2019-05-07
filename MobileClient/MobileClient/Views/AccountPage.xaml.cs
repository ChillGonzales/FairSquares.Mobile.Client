﻿using MobileClient.Authentication;
using MobileClient.Models;
using MobileClient.Services;
using MobileClient.Utilities;
using Plugin.InAppBilling;
using Plugin.InAppBilling.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Auth;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AccountPage : ContentPage
    {
        private readonly ICurrentUserService _userCache;
        private readonly ICache<SubscriptionModel> _subCache;
        private readonly IOrderValidationService _orderValidator;
        private readonly AccountModel _user;
        private readonly ILogger<AccountPage> _logger;

        public AccountPage()
        {
            InitializeComponent();
            _userCache = App.Container.GetInstance<ICurrentUserService>();
            _logger = App.Container.GetInstance<EmailLogger<AccountPage>>();
            _user = _userCache.GetLoggedInAccount();
            _subCache = App.Container.GetInstance<ICache<SubscriptionModel>>();
            _orderValidator = App.Container.GetInstance<IOrderValidationService>();
            _userCache.OnLoggedIn += (s, e) => SetUIToAccount(e.Account);
            SetUIToAccount(_user);
            SetSubUI();
            LogoutButton.Clicked += LogoutButton_Clicked;
            FeedbackButton.Clicked += FeedbackButton_Clicked;
            SubscribeButton.Clicked += SubscribeButton_Clicked;
        }

        private async void FeedbackButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new FeedbackPage());
        }

        protected override void OnAppearing()
        {
            SetSubUI();
        }

        private async void SubscribeButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var validation = await _orderValidator.ValidateOrderRequest(_userCache.GetLoggedInAccount());
                if (new[] { ValidationState.SubscriptionReportValid, ValidationState.NoReportsLeftInPeriod }.Contains(validation.State))
                {
                    await Navigation.PushAsync(new ManageSubscriptionPage(new ViewModels.ManageSubscriptionViewModel()
                    {
                        RemainingOrders = validation.RemainingOrders,
                        SubscriptionType = validation.Subscription.SubscriptionType,
                        EndDateTime = validation.Subscription.EndDateTime
                    }));
                }
                else
                {
                    await Navigation.PushAsync(new PurchasePage(DependencyService.Get<IAlertService>(),
                                                                          App.Container.GetInstance<IPurchasingService>(),
                                                                          App.Container.GetInstance<ICache<SubscriptionModel>>(),
                                                                          App.Container.GetInstance<ISubscriptionService>(),
                                                                          App.Container.GetInstance<ICurrentUserService>(),
                                                                          validation));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred attempting to purchase subscription.{ex.ToString()}");
            }
        }

        private void LogoutButton_Clicked(object sender, EventArgs e)
        {
            _userCache.LogOut();
            SetUIToAccount(null);
        }

        private async void ToolbarItem_Activated(object sender, EventArgs e)
        {
            await this.Navigation.PushAsync(new InstructionPage(null, false));
        }

        private void SetUIToAccount(AccountModel account)
        {
            if (account == null)
            {
                EmailLabel.Text = "Please Log In To Continue";
                LogoutButton.Text = "Log In";
                LogoutButton.StyleClass = new List<string>() { "Info" };
            }
            else
            {
                EmailLabel.Text = $"Email: {account.Email}";
                LogoutButton.Text = "Sign Out";
                LogoutButton.StyleClass = new List<string>() { "Danger" };
            }
        }

        private void SetSubUI()
        {
            var valid = Task.Run(async () => await _orderValidator.ValidateOrderRequest(_user)).Result;
            if (valid.State == ValidationState.NoReportsLeftInPeriod || valid.State == ValidationState.SubscriptionReportValid)
            {
                SubscriptionLabel.Text = $"Reports remaining this period: {valid.RemainingOrders.ToString()}";
                SubscribeButton.StyleClass.Clear();
                SubscribeButton.StyleClass.Add("Info");
                SubscribeButton.Text = "Manage";
            }
        }
    }
}