using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MobileClient.Utility
{
    public class MainThreadNavigator
    {
        private readonly INavigation _nav;

        public MainThreadNavigator(INavigation nav)
        {
            _nav = nav;
        }

        public void Push(Page page)
        {
            Device.BeginInvokeOnMainThread(async () => await _nav.PushAsync(page));
        }
        public void Pop()
        {
            Device.BeginInvokeOnMainThread(async () => await _nav.PopAsync());
        }
        public void PushModal(Page page)
        {
            Device.BeginInvokeOnMainThread(async () => await _nav.PushModalAsync(page));
        }
        public void PopModal()
        {
            Device.BeginInvokeOnMainThread(async () => await _nav.PopModalAsync());
        }
        public void PopToRoot()
        {
            Device.BeginInvokeOnMainThread(async () => await _nav.PopToRootAsync());
        }
    }
}
