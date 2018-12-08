using MobileClient.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MobileClient.Services
{
    public interface INotificationService
    {
        void Notify(NotificationRequest request);
    }
}
