using MobileClient.Routes;
using MobileClient.Services;
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
    public class ManageSubscriptionTests
    {
        ValidationModel _model;
        Mock<INavigation> _nav;
        Mock<IPageFactory> _pageFactory;
        DateTimeOffset _endDateTime = DateTimeOffset.Now.AddDays(30);

        [SetUp]
        public void SetUp()
        {
            _model = new ValidationModel()
            {
                RemainingOrders = 3,
                State = ValidationState.SubscriptionReportValid,
                Subscription = new MobileClient.Models.SubscriptionModel()
                {
                    EndDateTime = _endDateTime,
                    SubscriptionType = MobileClient.Models.SubscriptionType.Basic
                }
            };
            _nav = new Mock<INavigation>();
            _pageFactory = new Mock<IPageFactory>();
        }
        [Test]
        public void WhenLoadingSubscription_DisplaysInformation()
        {
            bool openUri = false;
            Action<Uri> act = x => openUri = true;
            var manage = new ManageSubscriptionViewModel(_model, "Android", act, _nav.Object, _pageFactory.Object);
            Assert.AreEqual("   3", manage.RemainingOrdersLabel);
            Assert.AreEqual("   Basic", manage.SubscriptionTypeLabel);
            Assert.AreEqual("   " + _endDateTime.ToString("dddd, dd MMMM yyyy"), manage.EndDateLabel);
            Assert.IsFalse(openUri);
            manage.CancelSubCommand.Execute(null);
            Assert.IsTrue(openUri);
        }
    }
}
