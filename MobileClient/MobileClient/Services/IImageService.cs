using MobileClient.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MobileClient.Services
{
    public interface IImageService
    {
        Dictionary<string, ImageModel> GetImages(List<string> orderIds);
    }
}
