using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MobileClient.Models
{
    public class LaunchedFromPushModel : INotifyPropertyChanged
    {
        private string _orderId;

        public string OrderId
        {
            get
            {
                return _orderId;
            }
            set
            {
                if (value == _orderId)
                    return;
                _orderId = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OrderId)));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
