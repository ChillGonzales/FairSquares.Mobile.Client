using MobileClient.Models;
using MobileClient.Services;
using MobileClient.Utilities;
using Moq;
using NUnit.Framework;
using Plugin.InAppBilling.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestFixture]
    public class PurchasingServiceTests
    {
        private Mock<IInAppBilling> _billing;
        private Mock<ILogger<PurchasingService>> _logger;
        
        private PurchasingService GetService()
        {
            _billing = new Mock<IInAppBilling>();
            _billing.Setup(x => x.ConnectAsync(ItemType.Subscription)).Returns(Task.FromResult(true));
            _billing.Setup(x => x.DisconnectAsync()).Returns(Task.Delay(10));
            _billing.Setup(x => x.PurchaseAsync(It.IsAny<string>(), ItemType.Subscription, It.IsAny<string>(), null))
                .Returns(Task.FromResult(
                    new InAppBillingPurchase() { State = PurchaseState.Purchased, PurchaseToken = "token", TransactionDateUtc = DateTime.Now }));
            _billing.Setup(x => x.GetPurchasesAsync(ItemType.Subscription, null))
                .Returns(Task.FromResult( Enumerable.Empty<InAppBillingPurchase>() ));
            _logger = new Mock<ILogger<PurchasingService>>();

            return new PurchasingService(_billing.Object, _logger.Object);
        }

        [Test]
        [TestCase(SubscriptionType.Basic)]
        [TestCase(SubscriptionType.Premium)]
        [TestCase(SubscriptionType.Enterprise)]
        public async Task WhenPurchasingSubscription_PurchasesCorrect(SubscriptionType type)
        {
            var svc = GetService();
            var name = SubscriptionUtility.GetInfoFromSubType(type).SubscriptionCode;
            var purchase = await svc.PurchaseItem(name, ItemType.Subscription, "");
            _billing.Verify(x => x.ConnectAsync(ItemType.Subscription), Times.Once);
            _billing.Verify(x => x.DisconnectAsync(), Times.Once);
            _billing.Verify(x => x.PurchaseAsync(name, ItemType.Subscription, "", null), Times.Once);
            _logger.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }

        public async Task WhenPurchasingSubscriptionWithoutConnection_ThrowsError()
        {
        }
    }
}
