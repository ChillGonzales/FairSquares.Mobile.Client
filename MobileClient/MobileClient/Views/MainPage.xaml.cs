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
        private readonly Dictionary<PageType, NavigationPage> MenuPages = new Dictionary<PageType, NavigationPage>();

        public MainPage()
        {
            InitializeComponent();
            MasterBehavior = MasterBehavior.Popover;

            // Adds default detail page (new order) to dictionary
            MenuPages.Add(PageType.Order, (NavigationPage) Detail);
        }
        public async Task NavigateFromMenu(PageType pageType)
        {
            if (!MenuPages.Keys.Contains(pageType))
            {
                switch (pageType)
                {
                    case (PageType.Order):
                        MenuPages.Add(PageType.Order, new NavigationPage(new OrderPage()));
                        break;
                    case (PageType.MyOrders):
                        MenuPages.Add(PageType.MyOrders, new NavigationPage(new MyOrdersPage()));
                        break;
                    case (PageType.Account):
                        MenuPages.Add(PageType.Account, new NavigationPage(new AccountPage()));
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
        public async Task NavigateToPage(NavigationPage page)
        {
            Detail = page;
            if (Device.RuntimePlatform == Device.Android)
                await Task.Delay(100);

            IsPresented = false;
        }
    }
}
