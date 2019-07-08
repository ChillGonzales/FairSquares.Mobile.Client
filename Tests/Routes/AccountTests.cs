using MobileClient.Authentication;
using MobileClient.Models;
using MobileClient.Routes;
using MobileClient.Services;
using MobileClient.Utilities;
using MobileClient.Utility;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Tests.Routes
{
    [TestFixture]
    public class AccountTests
    {
        private Mock<ICurrentUserService> _userCache;
        private Mock<IOrderValidationService> _validator;
        private Mock<ILogger<AccountViewModel>> _logger;
        private Mock<INavigation> _nav;
        private MainThreadNavigator _mnNav;
        private Mock<IPageFactory> _pageFac;
        private string _loginStyle;
        private string _subStyle;
        private Action<string> _loginStyleAction;
        private Action<string> _subStyleAction;


        [SetUp]
        public void SetUp()
        {
            _userCache = new Mock<ICurrentUserService>();
            _validator = new Mock<IOrderValidationService>();
            _nav = new Mock<INavigation>();
            _logger = new Mock<ILogger<AccountViewModel>>();
            _pageFac = new Mock<IPageFactory>();
            _mnNav = new MainThreadNavigator(_nav.Object);
            _loginStyle = "";
            _subStyle = "";
            _loginStyleAction = x => _loginStyle = x;
            _subStyleAction = x => _subStyle = x;
        }

        [Test]
        public void WhenUserLogsIn_NavToLanding()
        {
            _pageFac.Setup(x => x.GetPage(PageType.Landing)).Returns(null as Landing);
            _pageFac.Setup(x => x.GetPage(PageType.Instruction, false)).Returns(null as Instruction);
            var account = new AccountViewModel(_userCache.Object,
                    _validator.Object,
                    _mnNav,
                    _pageFac.Object,
                    _loginStyleAction,
                    _subStyleAction,
                    _logger.Object);
            Assert.AreEqual("Info", _loginStyle);
            Assert.AreEqual("Success", _subStyle);
            Assert.AreEqual(false, account.SubscriptionButtonEnabled);
            Assert.AreEqual("Please log in to continue", account.Email);
            Assert.AreEqual("Log In", account.LogOutText);
            Assert.AreEqual("View Options", account.SubscriptionButtonText);
            Assert.AreEqual("Please log in first.", account.SubscriptionLabel);
            account.LogOutCommand.Execute(null);
            account.ToolbarInfoCommand.Execute(null);
            _pageFac.Verify(x => x.GetPage(PageType.Landing), Times.Once);
            _pageFac.Verify(x => x.GetPage(PageType.Instruction, false), Times.Once);
            _nav.Verify(x => x.PushAsync(It.IsAny<Page>()), Times.Exactly(2));
        }

        [Test]
        public void WhenUserWithNoSub_CanPerformActions()
        {
            var user = new AccountModel()
            {
                Email = "testemail",
                UserId = "123"
            };
            _userCache.Setup(x => x.GetLoggedInAccount()).Returns(user);
            _validator.Setup(x => x.ValidateOrderRequest(user, It.IsAny<bool>()))
                .ReturnsAsync(new ValidationModel()
                {
                    State = ValidationState.FreeReportValid
                });
            var account = new AccountViewModel(_userCache.Object,
                    _validator.Object,
                    _mnNav,
                    _pageFac.Object,
                    _loginStyleAction,
                    _subStyleAction,
                    _logger.Object);
            Assert.AreEqual("Success", _subStyle);
            Assert.AreEqual("Danger", _loginStyle);
            Assert.AreEqual("Purchase a monthly subscription that fits your needs.", account.SubscriptionLabel);
            Assert.AreEqual(true, account.SubscriptionButtonEnabled);
            Assert.AreEqual("Purchase", account.SubscriptionButtonText);
            Assert.AreEqual("Sign Out", account.LogOutText);
            Assert.AreEqual(user.Email, account.Email);
            _userCache.VerifyAll();
            _validator.VerifyAll();
            account.SubscriptionCommand.Execute(null);
            _pageFac.Verify(x => x.GetPage(PageType.Purchase, It.IsAny<ValidationModel>()), Times.Once);
            _nav.Verify(x => x.PushAsync(It.IsAny<Purchase>()), Times.Once);
        }

        [Test]
        public void WhenUserWithSub_CanPerformActions()
        {
            var user = new AccountModel()
            {
                Email = "test",
                UserId = "123"
            };
            _userCache.Setup(x => x.GetLoggedInAccount()).Returns(user);
            _pageFac.Setup(x => x.GetPage(PageType.ManageSubscription, It.IsAny<ValidationModel>()))
                .Returns(null as ManageSubscription);
            _nav.Setup(x => x.PushAsync(It.IsAny<ManageSubscription>()));
            _validator.Setup(x => x.ValidateOrderRequest(user, It.IsAny<bool>()))
                .ReturnsAsync(new ValidationModel()
                {
                    State = ValidationState.SubscriptionReportValid,
                    RemainingOrders = 2
                });
            var acct = new AccountViewModel(_userCache.Object,
                _validator.Object,
                _mnNav,
                _pageFac.Object,
                _loginStyleAction,
                _subStyleAction,
                _logger.Object);
            Assert.AreEqual("Danger", _loginStyle);
            Assert.AreEqual("Success", _subStyle);
            Assert.AreEqual("Manage", acct.SubscriptionButtonText);
            Assert.AreEqual("Reports remaining: 2", acct.SubscriptionLabel);
            Assert.AreEqual(true, acct.SubscriptionButtonEnabled);
            Assert.AreEqual(user.Email, acct.Email);
            acct.SubscriptionCommand.Execute(null);
            _userCache.VerifyAll();
            _nav.VerifyAll();
            _validator.VerifyAll();
            _pageFac.VerifyAll();
        }
    }
}
