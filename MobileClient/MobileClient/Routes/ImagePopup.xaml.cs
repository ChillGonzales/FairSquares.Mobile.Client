using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileClient.Routes
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ImagePopup : ContentPage
    {
        public ImagePopup(StreamImageSource image)
        {
            InitializeComponent();
            BindingContext = new ImagePopupViewModel(image);
        }
    }
}