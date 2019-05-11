using MobileClient.Routes.Account;
using MobileClient.Routes.Instruction;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace MobileClient.Utility
{
    public static class PageFactory
    {
        public static Page GetPage(PageType page)
        {
            switch (page)
            {
                case PageType.Account:
                    return new Account();
                case PageType.Instruction:
                    return new Instruction();
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
