﻿using MobileClient.ViewModels;
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
    public partial class OrderPage : ContentPage
    {
        private MainPage RootPage { get => Application.Current.MainPage as MainPage; }
        public OrderPage()
        {
            InitializeComponent();
            StatePicker.ItemsSource = States.Select(x => x.Text).ToList();
            SubmitButton.Clicked += async (s, e) =>
            {
                await RootPage.NavigateFromMenu(MenuItemType.MyOrders);
            };
        }

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