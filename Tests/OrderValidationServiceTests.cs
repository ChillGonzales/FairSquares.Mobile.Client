using MobileClient.Authentication;
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
        private ICache<Order> _orderCache;
        private ICache<PurchasedReportModel> _prCache;
        private ICache<SubscriptionModel> _subCache;
        private Mock<ICacheRefresher> _refresher;
        private Mock<ILogger<OrderValidationService>> _logger;

        [SetUp]
        public void Setup()
        {
            _orderCache = new MemoryCache<Order>(new DebugLogger<MemoryCache<Order>>());
            _subCache = new MemoryCache<SubscriptionModel>(new DebugLogger<MemoryCache<SubscriptionModel>>());
            _prCache = new MemoryCache<PurchasedReportModel>(new DebugLogger<MemoryCache<PurchasedReportModel>>());
            _refresher = new Mock<ICacheRefresher>();
            _refresher.Setup(x => x.RefreshCaches(It.IsAny<AccountModel>())).Returns(Task.Delay(5));
            _logger = new Mock<ILogger<OrderValidationService>>();
        }

        [Test]
        public async Task WhenNoSubscriptionAndNoOrders_FreeReportAvailable()
        {
            var valSvc = new OrderValidationService(_refresher.Object, _orderCache, _subCache, _prCache, _logger.Object);
            var result = await valSvc.ValidateOrderRequest(new AccountModel() { UserId = "1234" });
            Assert.AreEqual(ValidationState.FreeReportValid, result.State);
            Assert.AreEqual(1, result.RemainingOrders);
        }

        [Test]
        public async Task WhenNotUsingCached_ShouldRefreshCachesBeforeValidating()
        {
            var refresh = new Mock<ICacheRefresher>();
            refresh.Setup(x => x.RefreshCaches(It.IsAny<AccountModel>()))
                .Returns(Task.Run(() => _orderCache.Put("1", new Order())));
            var valSvc = new OrderValidationService(_refresher.Object, _orderCache, _subCache, _prCache, _logger.Object);
            var result = await valSvc.ValidateOrderRequest(new AccountModel() { UserId = "123" }, false);
            Assert.AreEqual(ValidationState.NoSubscriptionAndTrialValid, result.State);
            Assert.AreEqual(0, result.RemainingOrders);
        }

        [Test]
        public async Task WhenNoSubscriptionAndOneOrder_FreeTrialAvailable()
        {
            _orderCache.Put("1", new Order());
            var valSvc = new OrderValidationService(_refresher.Object, _orderCache, _subCache, _prCache, _logger.Object);

            var result = await valSvc.ValidateOrderRequest(new AccountModel() { UserId = "1234" });
            Assert.AreEqual(ValidationState.NoSubscriptionAndTrialValid, result.State);
            Assert.AreEqual(0, result.RemainingOrders);
            _refresher.Verify(x => x.RefreshCaches(It.IsAny<AccountModel>()), Times.Never);
        }

        [Test]
        public async Task WhenPrevSubscriptionAndOneOrder_FreeTrialNotAvailable()
        {
            _subCache.Put("1", new SubscriptionModel() { EndDateTime = DateTimeOffset.Now.AddDays(-3) });
            _orderCache.Put("1", new Order());

            var valSvc = new OrderValidationService(_refresher.Object, _orderCache, _subCache, _prCache, _logger.Object);
            var result = await valSvc.ValidateOrderRequest(new AccountModel() { UserId = "1234" });
            Assert.AreEqual(ValidationState.NoSubscriptionAndTrialAlreadyUsed, result.State);
        }

        [Test]
        [TestCase(SubscriptionType.Basic, 2)]
        [TestCase(SubscriptionType.Premium, 6)]
        [TestCase(SubscriptionType.Enterprise, 20)]
        public async Task WhenActiveSubscriptionAndRemainingOrders_ShouldHaveOrdersLeft(SubscriptionType type, int orderCount)
        {
            _subCache.Put("1", new SubscriptionModel()
            {
                EndDateTime = DateTimeOffset.Now.AddDays(2),
                SubscriptionType = type,
                StartDateTime = DateTimeOffset.Now.AddDays(-20)
            });
            _orderCache.Put(Enumerable.Range(0, orderCount + 1)
                .ToDictionary(x => x.ToString(), x => new Order() { DateReceived = DateTimeOffset.Now.AddDays(-2) }));

            var valSvc = new OrderValidationService(_refresher.Object, _orderCache, _subCache, _prCache, _logger.Object);
            var result = await valSvc.ValidateOrderRequest(new AccountModel() { UserId = "12345" });
            Assert.AreEqual(ValidationState.SubscriptionReportValid, result.State);
            Assert.AreEqual(SubscriptionUtility.GetInfoFromSubType(type).OrderCount - orderCount, result.RemainingOrders);
        }

        [Test]
        [TestCase(SubscriptionType.Basic)]
        [TestCase(SubscriptionType.Premium)]
        [TestCase(SubscriptionType.Enterprise)]
        public async Task WhenActiveSubscriptionAndAllOrdersUsed_ShouldNotValidate(SubscriptionType type)
        {
            _subCache.Put("1", new SubscriptionModel()
            {
                EndDateTime = DateTimeOffset.Now.AddDays(2),
                SubscriptionType = type,
                StartDateTime = DateTimeOffset.Now.AddDays(-20)
            });
            _orderCache.Put(Enumerable.Range(0, SubscriptionUtility.GetInfoFromSubType(type).OrderCount + 1)
                    .ToDictionary(x => x.ToString(), x => new Order() { DateReceived = DateTimeOffset.Now.AddDays(-2) }));

            var valSvc = new OrderValidationService(_refresher.Object, _orderCache, _subCache, _prCache, _logger.Object);
            var result = await valSvc.ValidateOrderRequest(new MobileClient.Authentication.AccountModel() { UserId = "234" });
            Assert.AreEqual(ValidationState.NoReportsLeftInPeriod, result.State);
        }

        [Test]
        [TestCase(2, ValidationState.SubscriptionReportValid)]
        [TestCase(40, ValidationState.SubscriptionReportValid)]
        [TestCase(41, ValidationState.NoReportsLeftInPeriod)]
        [TestCase(42, ValidationState.NoReportsLeftInPeriod)]
        public async Task WhenNotAllOrdersUsedFromOneSub_RollsOverToNextMonth(int totalOrders, ValidationState expected)
        {
            var subs = new List<SubscriptionModel>()
                {
                    new SubscriptionModel()
                    {
                        PurchaseId = "1",
                        EndDateTime = DateTimeOffset.Now.AddDays(2),
                        SubscriptionType = SubscriptionType.Basic,
                        StartDateTime = DateTimeOffset.Now.AddDays(-20)
                    },
                    new SubscriptionModel()
                    {
                        PurchaseId = "2",
                        EndDateTime = DateTimeOffset.Now.AddDays(-20),
                        SubscriptionType = SubscriptionType.Premium,
                        StartDateTime = DateTimeOffset.Now.AddDays(-40)
                    },
                    new SubscriptionModel()
                    {
                        PurchaseId = "3",
                        EndDateTime = DateTimeOffset.Now.AddDays(-40),
                        SubscriptionType = SubscriptionType.Enterprise,
                        StartDateTime = DateTimeOffset.Now.AddDays(-60)
                    },
                    new SubscriptionModel()
                    {
                        PurchaseId = "4",
                        EndDateTime = DateTimeOffset.Now.AddDays(-60),
                        SubscriptionType = SubscriptionType.Basic,
                        StartDateTime = DateTimeOffset.Now.AddDays(-80)
                    }
                };
            var prs = new List<PurchasedReportModel>()
            {
                new PurchasedReportModel()
                {
                    PurchaseId = "1"
                },
                new PurchasedReportModel()
                {
                    PurchaseId = "2"
                }
            };
            var purchasedOrderCount = subs.Select(x => SubscriptionUtility.GetInfoFromSubType(x.SubscriptionType).OrderCount).Sum() + prs.Count();

            _subCache.Put(subs.ToDictionary(x => x.PurchaseId, x => x));
            _prCache.Put(prs.ToDictionary(x => x.PurchaseId, x => x));
            _orderCache.Put(Enumerable.Range(0, totalOrders + 1)
                .ToDictionary(x => x.ToString(), x => new Order()
                {
                    Fulfilled = true,
                    DateReceived = DateTimeOffset.Now.AddDays(-2)
                }));
            var valSvc = new OrderValidationService(_refresher.Object, _orderCache, _subCache, _prCache, _logger.Object);
            var result = await valSvc.ValidateOrderRequest(new MobileClient.Authentication.AccountModel() { UserId = "234" });
            var expectedCount = purchasedOrderCount - totalOrders > 0 ? purchasedOrderCount - totalOrders : 0;
            Assert.AreEqual(expectedCount, result.RemainingOrders);
            Assert.AreEqual(expected, result.State);
        }
    }
}