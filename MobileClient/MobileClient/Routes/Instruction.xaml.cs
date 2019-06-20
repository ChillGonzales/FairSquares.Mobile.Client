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
        public Instruction(bool showDismissButton)
        {
            InitializeComponent();
            BindingContext = new InstructionViewModel(new MainThreadNavigator(this.Navigation), App.Container.GetInstance<ICache<SettingsModel>>(), showDismissButton);
        }
    }
}