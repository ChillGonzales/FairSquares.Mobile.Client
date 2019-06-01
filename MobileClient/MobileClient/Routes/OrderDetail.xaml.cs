using FairSquares.Measurement.Core.Models;
using MobileClient.Models;
using MobileClient.Services;
using MobileClient.Utilities;
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
    public partial class OrderDetail : ContentPage
    {
        private readonly ICommand _onAppearing;

        public OrderDetail(Models.Order order)
        {
            InitializeComponent();
            var vm = new OrderDetailViewModel(order,
                                              App.Container.GetInstance<ICache<PropertyModel>>(),
                                              App.Container.GetInstance<ICache<ImageModel>>(),
                                              App.Container.GetInstance<IPropertyService>(),
                                              App.Container.GetInstance<IImageService>(),
                                              App.Container.GetInstance<IToastService>(),
                                              this.Navigation,
                                              (s1, s2, s3) => DisplayAlert(s1, s2, s3),
                                              App.Container.GetInstance<ILogger<OrderDetailViewModel>>());
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