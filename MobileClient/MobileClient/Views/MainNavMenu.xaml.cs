using MobileClient.Authentication;
using MobileClient.ViewModels;
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
    public partial class MainNavMenu : ContentPage
    {
        private MainPage RootPage { get => Application.Current.MainPage as MainPage; }
        private readonly List<MainNavMenuItem> _menuItems;
        private readonly ICurrentUserService _userService;

        public MainNavMenu()
        {
            InitializeComponent();
            // Fill out nav bar menu
            _menuItems = new List<MainNavMenuItem>()
            {
                new MainNavMenuItem() { PageType = PageType.Order, Title = "Enter Address" },
                new MainNavMenuItem() { PageType = PageType.MyOrders, Title = "My Roofs" },
                new MainNavMenuItem() { PageType = PageType.Account, Title = "Account" }
            };

            ListViewMenu.ItemsSource = _menuItems;
            ListViewMenu.SelectedItem = _menuItems[0];
            _userService = App.Container.GetInstance<ICurrentUserService>();

            // Hook up event for when menu item gets selected
            ListViewMenu.ItemSelected += async (s, e) =>
            {
                if (e.SelectedItem == null)
                    return;
                var selectedType = ((MainNavMenuItem)e.SelectedItem).PageType;
                if (_userService.GetLoggedInAccount() == null)
                {
                    await RootPage.NavigateFromMenu(PageType.Account);
                }
                else
                {
                    await RootPage.NavigateFromMenu(selectedType);
                }
            };
        }
    }
}