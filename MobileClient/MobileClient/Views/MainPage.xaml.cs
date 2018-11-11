using MobileClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MobileClient.Views
{
    public partial class MainPage : MasterDetailPage
    {
        private readonly Dictionary<MenuItemType, NavigationPage> MenuPages = new Dictionary<MenuItemType, NavigationPage>();

        public MainPage()
        {
            InitializeComponent();
            MasterBehavior = MasterBehavior.Popover;

            // Adds default detail page (new order) to dictionary
            MenuPages.Add(MenuItemType.Order, (NavigationPage) Detail);
        }
        public async Task NavigateFromMenu(MenuItemType pageType)
        {
            if (!MenuPages.Keys.Contains(pageType))
            {
                switch (pageType)
                {
                    case (MenuItemType.Order):
                        MenuPages.Add(MenuItemType.Order, new NavigationPage(new OrderPage()));
                        break;
                    case (MenuItemType.MyOrders):
                        MenuPages.Add(MenuItemType.MyOrders, new NavigationPage(new MyOrdersPage()));
                        break;
                    case (MenuItemType.Account):
                        MenuPages.Add(MenuItemType.Account, new NavigationPage(new AccountPage()));
                        break;
                }
            }

            var newPage = MenuPages[pageType];
            if (newPage != null && Detail != newPage)
            {
                Detail = newPage;

                if (Device.RuntimePlatform == Device.Android)
                    await Task.Delay(100);

                IsPresented = false;
            }
        }
    }
}
