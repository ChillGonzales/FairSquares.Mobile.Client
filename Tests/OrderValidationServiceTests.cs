using MobileClient.Models;
using MobileClient.Services;
using MobileClient.Utilities;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tests
{
    public class OrderValidationServiceTests
    {
        private Mock<IOrderService> _orderService;
        private Mock<ISubscriptionService> _subService;
        private Mock<IPurchasedReportService> _prService;
        private Mock<ICache<Order>> _orderCache;
        private Mock<ICache<PurchasedReportModel>> _prCache;
        private Mock<ICache<SubscriptionModel>> _subCache;
        private Mock<ILogger<OrderValidationService>> _logger;

        [SetUp]
        public void Setup()
        {
            _orderService = new Mock<IOrderService>();
            _subService = new Mock<ISubscriptionService>();
            _prService = new Mock<IPurchasedReportService>();
            _orderCache = new Mock<ICache<Order>>();
            _subCache = new Mock<ICache<SubscriptionModel>>();
            _prCache = new Mock<ICache<PurchasedReportModel>>();
            _logger = new Mock<ILogger<OrderValidationService>>();
            _orderService.Setup(x => x.GetMemberOrders(It.IsAny<string>()))
                .Returns(Task.FromResult(new List<Order>() { new Order() { OrderId = "1" } } as IEnumerable<Order>));
            _subService.Setup(x => x.GetSubscriptions(It.IsAny<string>())).Returns(new List<SubscriptionModel>());
            _prService.Setup(x => x.GetPurchasedReports(It.IsAny<string>())).Returns(new List<PurchasedReportModel>());
        }

        [Test]
        public async Task WhenNoSubscriptionAndNoOrders_FreeReportAvailable()
        {
            var orders = new Mock<IOrderService>();
            orders.Setup(x => x.GetMemberOrders(It.IsAny<string>())).Returns(Task.FromResult(Enumerable.Empty<Order>()));
            var valSvc = new OrderValidationService(orders.Object, _subService.Object, _prService.Object, 
                _orderCache.Object, _subCache.Object, _prCache.Object, _logger.Object);

            var result = await valSvc.ValidateOrderRequest(new MobileClient.Authentication.AccountModel() { UserId = "1234" });
            Assert.AreEqual(ValidationState.FreeReportValid, result.State);
            orders.VerifyAll();
            _subService.VerifyAll();
        }

        [Test]
        public async Task WhenNoSubscriptionAndOneOrder_FreeTrialAvailable()
        {
            var valSvc = new OrderValidationService(_orderService.Object, _subService.Object, _prService.Object, 
                _orderCache.Object, _subCache.Object, _prCache.Object, _logger.Object);

            var result = await valSvc.ValidateOrderRequest(new MobileClient.Authentication.AccountModel() { UserId = "1234" });
            Assert.AreEqual(ValidationState.NoSubscriptionAndTrialValid, result.State);
            _orderService.VerifyAll();
            _subService.VerifyAll();
        }

        [Test]
        public async Task WhenPrevSubscriptionAndOneOrder_FreeTrialNotAvailable()
        {
            var sub = new Mock<ISubscriptionService>();
            sub.Setup(x => x.GetSubscriptions(It.IsAny<string>()))
                .Returns(new List<SubscriptionModel>() { new SubscriptionModel() { EndDateTime = DateTimeOffset.Now.AddDays(-3) } });

            var valSvc = new OrderValidationService(_orderService.Object, sub.Object, _prService.Object,
                _orderCache.Object, _subCache.Object, _prCache.Object, _logger.Object);
            var result = await valSvc.ValidateOrderRequest(new MobileClient.Authentication.AccountModel() { UserId = "1234" });
            Assert.AreEqual(ValidationState.NoSubscriptionAndTrialAlreadyUsed, result.State);
            sub.VerifyAll();
            _orderService.VerifyAll();
        }

        [Test]
        [TestCase(SubscriptionType.Basic, 2)]
        [TestCase(SubscriptionType.Premium, 6)]
        [TestCase(SubscriptionType.Enterprise, 20)]
        public async Task WhenActiveSubscriptionAndRemainingOrders_ShouldHaveOrdersLeft(SubscriptionType type, int orderCount)
        {
            var sub = new Mock<ISubscriptionService>();
            var order = new Mock<IOrderService>();
            sub.Setup(x => x.GetSubscriptions(It.IsAny<string>()))
                .Returns(new List<SubscriptionModel>()
                {
                    new SubscriptionModel()
                    {
                        EndDateTime = DateTimeOffset.Now.AddDays(2),
                        SubscriptionType = type,
                        StartDateTime = DateTimeOffset.Now.AddDays(-20)
                    }
                });
            order.Setup(x => x.GetMemberOrders(It.IsAny<string>()))
                .Returns(Task.FromResult(
                    Enumerable.Range(0, orderCount + 1).Select(x => new Order() { DateReceived = DateTimeOffset.Now.AddDays(-2) })));

            var valSvc = new OrderValidationService(order.Object, sub.Object, _prService.Object,
                _orderCache.Object, _subCache.Object, _prCache.Object, _logger.Object);
            var result = await valSvc.ValidateOrderRequest(new MobileClient.Authentication.AccountModel() { UserId = "12345" });
            Assert.AreEqual(ValidationState.SubscriptionReportValid, result.State);
            Assert.AreEqual(SubscriptionUtility.GetInfoFromSubType(type).OrderCount - orderCount, result.RemainingOrders);
            order.VerifyAll();
            sub.VerifyAll();
        }

        [Test]
        [TestCase(SubscriptionType.Basic)]
        [TestCase(SubscriptionType.Premium)]
        [TestCase(SubscriptionType.Enterprise)]
        public async Task WhenActiveSubscriptionAndAllOrdersUsed_ShouldNotValidate(SubscriptionType type)
        {
            var sub = new Mock<ISubscriptionService>();
            var order = new Mock<IOrderService>();

            sub.Setup(x => x.GetSubscriptions(It.IsAny<string>()))
                .Returns(new List<SubscriptionModel>()
                {
                    new SubscriptionModel()
                    {
                        EndDateTime = DateTimeOffset.Now.AddDays(2),
                        SubscriptionType = type,
                        StartDateTime = DateTimeOffset.Now.AddDays(-20)
                    }
                });

            order.Setup(x => x.GetMemberOrders(It.IsAny<string>()))
                .Returns(Task.FromResult(
                    Enumerable.Range(0, SubscriptionUtility.GetInfoFromSubType(type).OrderCount + 1)
                    .Select(x => new Order() { DateReceived = DateTimeOffset.Now.AddDays(-2) })));

            var valSvc = new OrderValidationService(order.Object, sub.Object, _prService.Object,
                _orderCache.Object, _subCache.Object, _prCache.Object, _logger.Object);
            var result = await valSvc.ValidateOrderRequest(new MobileClient.Authentication.AccountModel() { UserId = "234" });
            Assert.AreEqual(ValidationState.NoReportsLeftInPeriod, result.State);
            sub.VerifyAll();
            order.VerifyAll();
        }

        [Test]
        [TestCase(2, ValidationState.SubscriptionReportValid)]
        [TestCase(38, ValidationState.SubscriptionReportValid)]
        [TestCase(39, ValidationState.NoReportsLeftInPeriod)]
        [TestCase(40, ValidationState.NoReportsLeftInPeriod)]
        public async Task WhenNotAllOrdersUsedFromOneSub_RollsOverToNextMonth(int totalOrders, ValidationState expected)
        {
            var sub = new Mock<ISubscriptionService>();
            var order = new Mock<IOrderService>();
            var subs = new List<SubscriptionModel>()
                {
                    new SubscriptionModel()
                    {
                        EndDateTime = DateTimeOffset.Now.AddDays(2),
                        SubscriptionType = SubscriptionType.Basic,
                        StartDateTime = DateTimeOffset.Now.AddDays(-20)
                    },
                    new SubscriptionModel()
                    {
                        EndDateTime = DateTimeOffset.Now.AddDays(-20),
                        SubscriptionType = SubscriptionType.Premium,
                        StartDateTime = DateTimeOffset.Now.AddDays(-40)
                    },
                    new SubscriptionModel()
                    {
                        EndDateTime = DateTimeOffset.Now.AddDays(-40),
                        SubscriptionType = SubscriptionType.Enterprise,
                        StartDateTime = DateTimeOffset.Now.AddDays(-60)
                    },
                    new SubscriptionModel()
                    {
                        EndDateTime = DateTimeOffset.Now.AddDays(-60),
                        SubscriptionType = SubscriptionType.Basic,
                        StartDateTime = DateTimeOffset.Now.AddDays(-80)
                    }
                };
            var purchasedOrderCount = subs.Select(x => SubscriptionUtility.GetInfoFromSubType(x.SubscriptionType).OrderCount).Sum();

            sub.Setup(x => x.GetSubscriptions(It.IsAny<string>()))
                .Returns(subs);
            order.Setup(x => x.GetMemberOrders(It.IsAny<string>()))
                .ReturnsAsync(Enumerable.Range(0, totalOrders + 1).Select(x => new Order()
                {
                    Fulfilled = true,
                    DateReceived = DateTimeOffset.Now.AddDays(-2)
                }));
            var valSvc = new OrderValidationService(order.Object, sub.Object, _prService.Object,
                _orderCache.Object, _subCache.Object, _prCache.Object, _logger.Object);
            var result = await valSvc.ValidateOrderRequest(new MobileClient.Authentication.AccountModel() { UserId = "234" });
            var expectedCount = purchasedOrderCount - totalOrders > 0 ? purchasedOrderCount - totalOrders : 0;
            Assert.AreEqual(expectedCount, result.RemainingOrders);
            Assert.AreEqual(expected, result.State);
            sub.VerifyAll();
            order.VerifyAll();
        }
    }
}