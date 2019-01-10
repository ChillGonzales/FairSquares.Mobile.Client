using System;
using System.Collections.Generic;
using System.Text;

namespace MobileClient.Models
{
    public class ImageModel
    {
        public string OrderId { get; set; }
        public string Uri { get; set; }
        public byte[] Image { get; set; }
    }
}
