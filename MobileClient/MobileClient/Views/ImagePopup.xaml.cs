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
        private double _startScale, _xOffset, _yOffset;

        public ImagePopup()
        {
            InitializeComponent();
        }
        public ImagePopup(StreamImageSource image)
        {
            InitializeComponent();
            _imgSource = image;
            FullscreenImage.Source = image;
            ImageTGR.Tapped += OnImageTapped;
            ImagePGR.PinchUpdated += OnImagePinched;
        }

        private async void OnImageTapped(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
        private void OnImagePinched(object sender, PinchGestureUpdatedEventArgs e)
        {
            if (e.Status == GestureStatus.Started)
            {
                // Store the current scale factor applied to the wrapped user interface element,
                // and zero the components for the center point of the translate transform.
                _startScale = Content.Scale;
                Content.AnchorX = 0;
                Content.AnchorY = 0;
            }
            if (e.Status == GestureStatus.Running)
            {
                // Calculate the scale factor to be applied.
                var currentScale = _startScale;
                currentScale += (e.Scale - 1) * _startScale;
                currentScale = Math.Max(1, currentScale);

                // The ScaleOrigin is in relative coordinates to the wrapped user interface element,
                // so get the X pixel coordinate.
                double renderedX = Content.X + _xOffset;
                double deltaX = renderedX / Width;
                double deltaWidth = Width / (Content.Width * _startScale);
                double originX = (e.ScaleOrigin.X - deltaX) * deltaWidth;

                // The ScaleOrigin is in relative coordinates to the wrapped user interface element,
                // so get the Y pixel coordinate.
                double renderedY = Content.Y + _yOffset;
                double deltaY = renderedY / Height;
                double deltaHeight = Height / (Content.Height * _startScale);
                double originY = (e.ScaleOrigin.Y - deltaY) * deltaHeight;

                // Calculate the transformed element pixel coordinates.
                double targetX = _xOffset - (originX * Content.Width) * (currentScale - _startScale);
                double targetY = _yOffset - (originY * Content.Height) * (currentScale - _startScale);

                // Apply translation based on the change in origin.
                Content.TranslationX = targetX.Clamp(-Content.Width * (currentScale - 1), 0);
                Content.TranslationY = targetY.Clamp(-Content.Height * (currentScale - 1), 0);

                // Apply scale factor.
                Content.Scale = currentScale;
            }
            if (e.Status == GestureStatus.Completed)
            {
                // Store the translation delta's of the wrapped user interface element.
                _xOffset = Content.TranslationX;
                _yOffset = Content.TranslationY;
            }
        }
    }
}