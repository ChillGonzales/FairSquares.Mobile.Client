﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MobileClient.ViewModels
{
    public class MainNavMenuItem
    {
        public BaseNavPageType PageType { get; set; }
        public string Title { get; set; }
    }
    public enum BaseNavPageType
    {
        Order = 0,
        MyOrders = 1,
        Account = 2
    }
}
