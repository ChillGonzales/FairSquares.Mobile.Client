﻿using MobileClient.Services;
using MobileClient.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileClient.Routes
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ManageSubscription : ContentPage
    {
        public ManageSubscription(ValidationModel model)
        {
            InitializeComponent();
            BindingContext = new ManageSubscriptionViewModel(model, 
                                                             Device.RuntimePlatform, 
                                                             x => Device.OpenUri(x),
                                                             new MainThreadNavigator(this.Navigation),
                                                             App.Container.GetInstance<IPageFactory>());
        }
    }
}