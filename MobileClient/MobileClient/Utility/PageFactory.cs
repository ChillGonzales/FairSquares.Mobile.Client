using MobileClient.Models;
using MobileClient.Routes;
using MobileClient.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;
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
                case PageType.BaseTab:
                    return new BaseTab();
                case PageType.ImagePopup:
                    return new ImagePopup(stateArgs[0] as StreamImageSource);
                case PageType.Instruction:
                    return new Instruction((stateArgs[0] as bool?) ?? false);
                case PageType.Landing:
                    return new Landing();
                case PageType.ManageSubscription:
                    return new ManageSubscription(stateArgs[0] as ValidationModel);
                case PageType.MyOrders:
                    return new MyOrders();
                case PageType.OrderDetail:
                    return new OrderDetail(stateArgs[0] as Models.Order);
                case PageType.Purchase:
                    return new Purchase(stateArgs[0] as ValidationModel);
                case PageType.Order:
                    return new Routes.Order();
                case PageType.PurchaseOptions:
                    return new PurchaseOptions(stateArgs[0] as ValidationModel);
                case PageType.SingleReportPurchase:
                    return new SingleReportPurchase(stateArgs[0] as ValidationModel);
                case PageType.Feedback:
                    return new Feedback();
                case PageType.OrderConfirmation:
                    return new OrderConfirmation(stateArgs[0] as LocationModel);
                default:
                    return new Account();
            }
        }
    }

    public enum PageType
    {
        Account,
        BaseTab,
        Feedback,
        ImagePopup,
        Instruction,
        Landing,
        ManageSubscription,
        Map,
        MyOrders,
        OrderDetail,
        Order,
        OrderConfirmation,
        PaymentConfirmation,
        Purchase,
        PurchaseOptions,
        SingleReportPurchase
    }
}
