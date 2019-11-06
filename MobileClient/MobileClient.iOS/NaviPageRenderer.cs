﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using MobileClient.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(NavigationPage), typeof(NaviPageRenderer))]
namespace MobileClient.iOS
{
    public class NaviPageRenderer : NavigationRenderer
    {
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            try
            {
                OverrideUserInterfaceStyle = UIUserInterfaceStyle.Light;
            }
            catch { }
        }
    }
}