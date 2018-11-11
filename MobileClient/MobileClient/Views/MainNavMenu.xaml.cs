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

        public MainNavMenu()
        {
            InitializeComponent();
            // Fill out nav bar menu
            _menuItems = new List<MainNavMenuItem>()
            {
                new MainNavMenuItem() { MenuItemType = MenuItemType.Order, Title = "New Order" },
                new MainNavMenuItem() { MenuItemType = MenuItemType.MyOrders, Title = "My Orders" },
                new MainNavMenuItem() { MenuItemType = MenuItemType.Account, Title = "Account" }
            };

            ListViewMenu.ItemsSource = _menuItems;
            ListViewMenu.SelectedItem = _menuItems[0];

            // Hook up event for when menu item gets selected
            ListViewMenu.ItemSelected += async (s, e) =>
            {
                if (e.SelectedItem == null)
                    return;
                var selectedType = ((MainNavMenuItem)e.SelectedItem).MenuItemType;
                await RootPage.NavigateFromMenu(selectedType);
            };
        }
    }
}