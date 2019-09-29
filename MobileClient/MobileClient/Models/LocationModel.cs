using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms.Maps;

namespace MobileClient.Models
{
    public class LocationModel
    {
        public Position Position { get; set; }
        public Placemark Placemark { get; set; }
    }
}
