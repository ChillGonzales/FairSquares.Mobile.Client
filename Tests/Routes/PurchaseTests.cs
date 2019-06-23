using MobileClient.Authentication;
using MobileClient.Models;
using MobileClient.Routes;
using MobileClient.Services;
using MobileClient.Utilities;
using MobileClient.Utility;
using MobileClient.ViewModels;
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
    public class PurchaseTests
    {
        private Mock<IToastService> _alertService;
        private Mock<IPurchasingService> _purchaseService;
        private Mock<ICache<SubscriptionModel>> _subCache;
        private Mock<ISubscriptionService> _subService;
        private Mock<ICurrentUserService> _userCache;
        private Mock<INavigation> _nav;
        private MainThreadNavigator _mnNav;
        private ValidationModel _validationModel;
        private string _runtimePlatform;
        private Action<BaseNavPageType> _navigateFromMenu;
        private Func<string, string, string, string, Task<bool>> _displayAlert;
        private Action<Uri> _openUri;
        private BaseNavPageType? CurrentTab = null;
        private Uri OpenedUri = null;

        [SetUp]
        public void SetUp()
        {
            _alertService = new Mock<IToastService>();
            _purchaseService = new Mock<IPurchasingService>();
            _subCache = new Mock<ICache<SubscriptionModel>>();
            _subService = new Mock<ISubscriptionService>();
            _userCache = new Mock<ICurrentUserService>();
            _nav = new Mock<INavigation>();
            _mnNav = new MainThreadNavigator(_nav.Object);
            _validationModel = new ValidationModel()
            {
                RemainingOrders = 0,
                State = ValidationState.FreeReportValid
            };
            _runtimePlatform = Device.Android;
            _navigateFromMenu = new Action<BaseNavPageType>(x => CurrentTab = x);
            _displayAlert = new Func<string, string, string, string, Task<bool>>((s1, s2, s3, s4) => Task.FromResult(false));
            _openUri = new Action<Uri>(x => OpenedUri = x);
        }

        [Test]
        public void WhenLoadingVm_LoadInitialState()
        {
            var vm = new PurchaseViewModel(_alertService.Object,
                                           _purchaseService.Object,
                                           _subCache.Object,
                                           _subService.Object,
                                           _userCache.Object,
                                           _mnNav,
                                           _validationModel,
                                           _runtimePlatform,
                                           _navigateFromMenu,
                                           _displayAlert,
                                           _openUri);
        }
    }
}
