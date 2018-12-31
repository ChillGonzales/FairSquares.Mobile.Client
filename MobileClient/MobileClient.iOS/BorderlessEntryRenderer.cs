using MobileClient.iOS;
using MobileClient;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using System.ComponentModel;
using CoreAnimation;

[assembly: ExportRenderer(typeof(BorderlessEntry), typeof(BorderlessEntryRenderer))]
namespace MobileClient.iOS
{
    public class BorderlessEntryRenderer : EntryRenderer
    {
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            var borderLayer = new CALayer();
            borderLayer.Frame = new CoreGraphics.CGRect(0f, this.Frame.Height - 1, this.Frame.Width, 1f);
            borderLayer.BorderColor = Color.White.ToCGColor();
            this.Control.Layer.AddSublayer(borderLayer);
        }
    }
}