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
            switch (pageType)
            {
                case PageType.Account:
                    CurrentPage = AccountTab;
                    break;
                case PageType.MyOrders:
                    CurrentPage = MyOrdersTab;
                    break;
                case PageType.Order:
                    CurrentPage = OrderTab;
                    break;
                default:
                    CurrentPage = OrderTab;
                    break;
            }
        }
    }
}