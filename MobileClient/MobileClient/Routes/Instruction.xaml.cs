using MobileClient.Models;
using MobileClient.Utilities;
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
    public partial class Instruction : ContentPage
    {
        private readonly bool _showDismissButton;
        public Instruction(bool showDismissButton)
        {
            InitializeComponent();
            _showDismissButton = showDismissButton;
            BindingContext = new InstructionViewModel(new MainThreadNavigator(this.Navigation), 
                                                      App.Container.GetInstance<ICache<SettingsModel>>(),
                                                      App.Container.GetInstance<ILogger<InstructionViewModel>>(),
                                                      showDismissButton);
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (_showDismissButton)
                Device.BeginInvokeOnMainThread(async () => await ScrollView.ScrollToAsync(SubmitButton, ScrollToPosition.End, true));
        }
    }
}