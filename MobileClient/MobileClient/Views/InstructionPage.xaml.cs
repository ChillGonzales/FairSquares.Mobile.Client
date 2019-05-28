using MobileClient.Models;
using MobileClient.Utilities;
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
    public partial class InstructionPage : ContentPage
    {
        private const int _dismissButtonCol = 8;
        private const int _switchCol = 7;
        private const int _titleCol = 0;
        private Func<Task> _popAction;
        private readonly ICache<SettingsModel> _settings;

        public InstructionPage(Func<Task> popAction, bool? showDismissButton = false)
        {
            InitializeComponent();
            _popAction = popAction;
            _settings = App.Container.GetInstance<ICache<SettingsModel>>();
            if (!showDismissButton ?? false)
            {
                InstructionGrid.RowDefinitions[_dismissButtonCol].Height = 0;
                InstructionGrid.RowDefinitions[_switchCol].Height = 0;
                InstructionGrid.RowDefinitions[_titleCol].Height = 0;
            }
            else
            {
                DismissButton.Clicked += (s, e) => DismissButtonClicked();
            }
        }

        private async void DismissButtonClicked()
        {
            if (NotShowAgainToggle.IsToggled)
            {
                _settings.Put("", new SettingsModel() { DisplayWelcomeMessage = false });
            }
            await _popAction();
        }
    }
}