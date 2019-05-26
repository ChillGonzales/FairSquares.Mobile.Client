using MobileClient.Authentication;
using MobileClient.Models;
using MobileClient.Routes;
using MobileClient.Services;
using MobileClient.Utilities;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Tests.Routes
{
    [TestFixture]
    public class FeedbackTests
    {
        private Mock<INotificationService> _notifier;
        private Mock<ICurrentUserService> _userCache;
        private Mock<IAlertService> _toast;
        private Mock<INavigation> _nav;

        [SetUp]
        public void SetUp()
        {
            _notifier = new Mock<INotificationService>();
            _userCache = new Mock<ICurrentUserService>();
            _toast = new Mock<IAlertService>();
            _nav = new Mock<INavigation>();
            _notifier.Setup(x => x.Notify(It.Is<NotificationRequest>(y => y.From == "feedback@fairsquarestech.com" &&
                                                                          y.To == "colin.monroe@fairsquarestech.com" &&
                                                                          y.MessageType == MessageType.Email)));
            _userCache.Setup(x => x.GetLoggedInAccount()).Returns(new AccountModel()
            {
                Email = "test@test.com",
                UserId = "1234"
            });
            _toast.Setup(x => x.ShortAlert(It.IsAny<string>()));
            _nav.Setup(x => x.PopAsync()).ReturnsAsync(null as Page);
        }

        [Test]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("hi")]
        public void WhenFeedbackEntered_RespondAppropriately(string feedback)
        {
            var vm = new FeedbackViewModel(_notifier.Object, _userCache.Object, _toast.Object, _nav.Object);
            vm.FeedbackEntry = feedback;
            vm.SubmitCommand.Execute(null);
            if (string.IsNullOrWhiteSpace(feedback))
            {
                _notifier.Verify(x => x.Notify(It.IsAny<NotificationRequest>()), Times.Never);
                _userCache.Verify(x => x.GetLoggedInAccount(), Times.Never);
                _nav.Verify(x => x.PopAsync(), Times.Never);
                _toast.Verify(x => x.ShortAlert(It.IsAny<string>()), Times.Never);
            }
            else
            {
                _notifier.VerifyAll();
                _userCache.VerifyAll();
                _nav.VerifyAll();
                _toast.VerifyAll();
            }
        }

        [Test]
        public void WhenNoUserLoggedIn_StillSendsFeedback()
        {
            _userCache = new Mock<ICurrentUserService>();
            _userCache.Setup(x => x.GetLoggedInAccount()).Returns(null as AccountModel);
            var vm = new FeedbackViewModel(_notifier.Object, _userCache.Object, _toast.Object, _nav.Object);
            vm.FeedbackEntry = "hi";
            vm.SubmitCommand.Execute(null);
            _notifier.VerifyAll();
            _userCache.VerifyAll();
            _nav.VerifyAll();
            _toast.VerifyAll();
        }
    }
}
