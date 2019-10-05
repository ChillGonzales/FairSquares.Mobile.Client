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
        private readonly Action<Action> _invoke;

        public MainThreadNavigator(Action<Action> invoke, INavigation nav)
        {
            _nav = nav;
            _invoke = invoke;
        }

        public void Push(Page page)
        {
            _invoke(async () => await _nav.PushAsync(page));
        }
        public void Pop()
        {
            _invoke(async () => await _nav.PopAsync());
        }
        public void PushModal(Page page)
        {
            _invoke(async () => await _nav.PushModalAsync(page));
        }
        public void PopModal()
        {
            _invoke(async () => await _nav.PopModalAsync());
        }
        public void PopToRoot()
        {
            _invoke(async () => await _nav.PopToRootAsync());
        }
    }
}
