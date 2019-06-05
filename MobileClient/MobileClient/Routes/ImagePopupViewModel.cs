using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

namespace MobileClient.Routes
{
    public class ImagePopupViewModel : INotifyPropertyChanged
    {
        private StreamImageSource _imageSource;

        public ImagePopupViewModel(StreamImageSource image)
        {
            _imageSource = image;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public StreamImageSource ImageSource
        {
            get
            {
                return _imageSource;
            }
            set
            {
                _imageSource = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageSource)));
            }
        }
    }
}
