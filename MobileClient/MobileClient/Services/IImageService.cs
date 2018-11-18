using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MobileClient.Services
{
    public interface IImageService
    {
        Task<Dictionary<string, byte[]>> GetImages(List<string> orderIds);
    }
}
