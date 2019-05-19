﻿using MobileClient.Routes;
using MobileClient.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace MobileClient.Utility
{
    public class PageFactory : IPageFactory
    {
        public Page GetPage(PageType page, params object[] stateArgs)
        {
            switch (page)
            {
                case PageType.Account:
                    return new Account();
                case PageType.Instruction:
                    return new Instruction((stateArgs[0] as bool?) ?? false);
                case PageType.Landing:
                    return new Landing();
                case PageType.ManageSubscription:
                    return new ManageSubscription(stateArgs[0] as ValidationModel);
                case PageType.Purchase:
                    return new Purchase();
                default:
                    return new Account();
            }
        }
    }

    public enum PageType
    {
        Account,
        Feedback,
        ImagePopup,
        Instruction,
        Landing,
        ManageSubscription,
        MyOrders,
        OrderDetail,
        Order,
        PaymentConfirmation,
        Purchase
    }
}
