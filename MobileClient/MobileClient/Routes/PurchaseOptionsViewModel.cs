using MobileClient.Services;
using MobileClient.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace MobileClient.Routes
{
    public class PurchaseOptionsViewModel : INotifyPropertyChanged
    {
        private readonly INavigation _nav;
        private readonly ValidationModel _validation;
        private readonly IPageFactory _pageFactory;

        public PurchaseOptionsViewModel(ValidationModel validation, INavigation nav, IPageFactory pageFactory)
        {
            _validation = validation;
            _nav = nav;
            _pageFactory = pageFactory;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand SingleReportCommand => new Command(async () => await _nav.PushAsync(_pageFactory.GetPage(PageType.SingleReportPurchase)));
        public ICommand SubscriptionCommand => new Command(async () => await _nav.PushAsync(_pageFactory.GetPage(PageType.Purchase, _validation)));
    }
}
