using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace MobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ImagePopup : ContentPage
    {
        private StreamImageSource _imgSource;

        public ImagePopup()
        {
            InitializeComponent();
        }
        public ImagePopup(StreamImageSource image)
        {
            InitializeComponent();
            _imgSource = image;
            FullscreenImage.Source = image;
        }
    }
}