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
    public partial class BaseTabPage : TabbedPage
    {
        public BaseTabPage ()
        {
            InitializeComponent();
        }

        public void NavigateFromMenu(PageType pageType)
        {
            Page newPage = null;
            switch (pageType)
            {
                case PageType.Account:
                    newPage = new AccountPage();
                    break;
                case PageType.MyOrders:
                    newPage = new MyOrdersPage();
                    break;
                case PageType.Order:
                    newPage = new OrderPage();
                    break;
                default:
                    newPage = new OrderPage();
                    break;
            }
            this.CurrentPage = newPage;
        }
    }
}