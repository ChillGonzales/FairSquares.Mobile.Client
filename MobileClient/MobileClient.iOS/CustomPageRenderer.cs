using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using MobileClient.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ContentPage), typeof(CustomPageRenderer))]
namespace MobileClient.iOS
{
    class CustomPageRenderer : PageRenderer
    {
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            try
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                {
                    OverrideUserInterfaceStyle = UIUserInterfaceStyle.Light;
                }
            }
            catch { }
        }
    }
}