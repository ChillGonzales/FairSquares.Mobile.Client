
using Android.App;
using Android.Widget;
using MobileClient.Droid.Utilities;
using MobileClient.Services;

[assembly: Xamarin.Forms.Dependency(typeof(AndroidToast))]
namespace MobileClient.Droid.Utilities
{
    public class AndroidToast : IToastService
    {
        public void LongToast(string message)
        {
            Toast.MakeText(Application.Context, message, ToastLength.Long).Show();
        }

        public void ShortToast(string message)
        {
            Toast.MakeText(Application.Context, message, ToastLength.Short).Show();
        }
    }
}