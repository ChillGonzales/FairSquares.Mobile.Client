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
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Tests.Routes
{
    [TestFixture]
    public class FeedbackTests
    {
        private Mock<INotificationService> _notifier;
        private Mock<ICurrentUserService> _userCache;
        private Mock<INavigation> _nav;
        private Mock<IPageFactory> _pageFactory;
        private Mock<ILogger<FeedbackViewModel>> _logger;
        private AlertUtility _alert;
        private MainThreadNavigator _mnNav;

        [SetUp]
        public void SetUp()
        {
            _notifier = new Mock<INotificationService>();
            _userCache = new Mock<ICurrentUserService>();
            _nav = new Mock<INavigation>();
            _alert = new AlertUtility((s1, s2, s3, s4) => Task.FromResult(false), (s1, s2, s3) => Task.Delay(0));
            _logger = new Mock<MobileClient.Utilities.ILogger<FeedbackViewModel>>();
            _pageFactory = new Mock<IPageFactory>();
            _notifier.Setup(x => x.Notify(It.Is<NotificationRequest>(y => y.From == "feedback@fairsquarestech.com" &&
                                                                          y.To == "colin.monroe@fairsquarestech.com" &&
                                                                          y.MessageType == MessageType.Email)));
            _userCache.Setup(x => x.GetLoggedInAccount()).Returns(new AccountModel()
            {
                Email = "test@test.com",
                UserId = "1234"
            });
            _nav.Setup(x => x.PopAsync()).ReturnsAsync(null as Page);
            _mnNav = new MainThreadNavigator(x => x(), _nav.Object);
        }

        [Test]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("hi")]
        public void WhenFeedbackEntered_RespondAppropriately(string feedback)
        {
            var vm = new FeedbackViewModel(_notifier.Object, _userCache.Object, _alert, _pageFactory.Object, _logger.Object, _mnNav);
            vm.FeedbackEntry = feedback;
            vm.SubmitCommand.Execute(null);
            if (string.IsNullOrWhiteSpace(feedback))
            {
                _notifier.Verify(x => x.Notify(It.IsAny<NotificationRequest>()), Times.Never);
                _userCache.Verify(x => x.GetLoggedInAccount(), Times.Never);
                _nav.Verify(x => x.PopAsync(), Times.Never);
            }
            else
            {
                _notifier.VerifyAll();
                _userCache.VerifyAll();
                _nav.VerifyAll();
            }
        }
    }
}
