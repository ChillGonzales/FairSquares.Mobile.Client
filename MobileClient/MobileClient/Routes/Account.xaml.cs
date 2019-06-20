using MobileClient.Authentication;
using MobileClient.Models;
using MobileClient.Services;
using MobileClient.Utilities;
using MobileClient.Utility;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileClient.Routes
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Account : ContentPage
    {
        private readonly ICommand _onAppearing;
        public Account()
        {
            InitializeComponent();
            var vm = new AccountViewModel(App.Container.GetInstance<ICurrentUserService>(),
                                          App.Container.GetInstance<IOrderValidationService>(),
                                          new MainThreadNavigator(this.Navigation),
                                          App.Container.GetInstance<IPageFactory>(),
                                          s => LogOutButton.StyleClass = new List<string>() { s },
                                          s => SubButton.StyleClass = new List<string>() { s },
                                          App.Container.GetInstance<ILogger<AccountViewModel>>());
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