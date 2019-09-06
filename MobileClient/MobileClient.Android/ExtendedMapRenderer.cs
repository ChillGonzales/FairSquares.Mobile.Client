using Android.Content;
using Android.Gms.Maps;
using MobileClient.Droid;
using MobileClient.Utility;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.Android;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(MobileClient.Utility.ExtendedMap), typeof(ExtendedMapRenderer))]
namespace MobileClient.Droid
{
    public class ExtendedMapRenderer : MapRenderer, IOnMapReadyCallback
    {
        public ExtendedMapRenderer(Context context) : base(context)
        { }

        protected override void OnMapReady(GoogleMap map)
        {
            base.OnMapReady(map);
            if (map != null)
                NativeMap.MapClick += GoogleMap_MapClick;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Map> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement != null)
                NativeMap.MapClick -= GoogleMap_MapClick;
            if (Control != null)
                ((MapView)Control).GetMapAsync(this);
        }

        private void GoogleMap_MapClick(object sender, GoogleMap.MapClickEventArgs e)
        {
            ((ExtendedMap)Element).OnTap(new Position(e.Point.Latitude, e.Point.Longitude));
        }
    }
}