using Xamarin.Forms;

namespace MobileClient.Utility
{
    public interface IPageFactory
    {
        Page GetPage(PageType pageType, params object[] stateArgs);
    }
}