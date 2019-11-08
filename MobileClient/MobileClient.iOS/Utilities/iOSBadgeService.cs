using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using MobileClient.iOS.Utilities;
using MobileClient.Services;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(iOSBadgeService))]
namespace MobileClient.iOS.Utilities
{
    public class iOSBadgeService : IBadgeService
    {
    }
}