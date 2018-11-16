using System;
using System.Collections.Generic;
using System.Text;

namespace MobileClient.ViewModels
{
    public class MainNavMenuItem
    {
        public PageType PageType { get; set; }
        public string Title { get; set; }
    }
    public enum PageType
    {
        Order = 0,
        MyOrders = 1,
        Account = 2,
        OrderDetail = 3
    }
}
