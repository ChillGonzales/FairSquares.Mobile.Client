using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using MobileClient.iOS.Utilities;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ListView), typeof(CustomList))]
namespace MobileClient.iOS.Utilities
{
    public class CustomList : ListViewRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
        {
            base.OnElementChanged(e);
            if (this.Control == null) return;
            this.Control.TableFooterView = new UIView();
        }
    }
}