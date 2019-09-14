using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace MobileClient.Utility
{
    /// <summary>
    /// Credit to https://github.com/halkar/XamarinExtendedMap
    /// </summary>
    public class ExtendedMap : Map
    {
        public event EventHandler<TapEventArgs> Tap;

        public ExtendedMap() : base() { }
        public ExtendedMap(MapSpan region) : base(region) { }

        public void OnTap(Position coordinate)
        {
            OnTap(new TapEventArgs { Position = coordinate });
        }

        protected virtual void OnTap(TapEventArgs e)
        {
            Tap?.Invoke(this, e);
        }
    }

    public class TapEventArgs : EventArgs
    {
        public Position Position { get; set; }
    }
}
