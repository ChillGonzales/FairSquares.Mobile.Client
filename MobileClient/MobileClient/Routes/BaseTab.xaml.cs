using MobileClient.Models;
using MobileClient.Utilities;
using MobileClient.Utility;
using MobileClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileClient.Routes
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BaseTab : TabbedPage
    {
        private readonly ICommand _onAppearing;

        public BaseTab()
        {
            InitializeComponent();
            var vm = new BaseTabViewModel(App.Container.GetInstance<ICache<SettingsModel>>(),
                                          App.Container.GetInstance<IPageFactory>(),
                                          this.Navigation,
                                          new Dictionary<BaseNavPageType, Page>()
                                          {
                                              { BaseNavPageType.Account, AccountTab },
                                              { BaseNavPageType.MyOrders, MyOrdersTab },
                                              { BaseNavPageType.Order, OrderTab }
                                          });
            _onAppearing = vm.OnAppearingBehavior;
            BindingContext = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _onAppearing.Execute(null);
        }
    }
}