using MobileClient.Routes;
using MobileClient.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests.Routes
{
    [TestFixture]
    public class ManageSubscriptionTests
    {
        ValidationModel _model;
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
        }
        [Test]
        public void WhenLoadingSubscription_DisplaysInformation()
        {
            bool openUri = false;
            Action<Uri> act = x => openUri = true;
            var manage = new ManageSubscriptionViewModel(_model, "Android", act);
            Assert.AreEqual("   3", manage.RemainingOrdersLabel);
            Assert.AreEqual("   Basic", manage.SubscriptionTypeLabel);
            Assert.AreEqual("   " + _endDateTime.ToString("dddd, dd MMMM yyyy"), manage.EndDateLabel);
            Assert.IsFalse(openUri);
            manage.CancelSubCommand.Execute(null);
            Assert.IsTrue(openUri);
        }
    }
}
