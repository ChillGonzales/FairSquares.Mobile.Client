using MobileClient.Authentication;
using MobileClient.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MobileClient.Utilities
{
    public class EmailLogger<T> : ILogger<T>
    {
        private readonly INotificationService _notifyService;
        private readonly ICurrentUserService _userCache;

        public EmailLogger(INotificationService notifyService, ICurrentUserService userCache)
        {
            _notifyService = notifyService;
            _userCache = userCache;
        }

        public void LogError(string message, params object[] args)
        {
            try
            {
                var emailBody = $"<p>User with ID '{_userCache.GetLoggedInAccount().UserId}' and email '{_userCache.GetLoggedInAccount().Email}' had the following issue: ";
                emailBody += @"<br/>" + message;
                foreach (var arg in args)
                {
                    emailBody += @"<br/>" + JsonConvert.SerializeObject(arg, Formatting.Indented);
                }
                emailBody += @"<br/>" + Device.Idiom.ToString() + "</p>";
                Task.Run(() =>
                {
                    _notifyService.Notify(new Models.NotificationRequest()
                    {
                        From = $"{Device.RuntimePlatform.ToString()}errors@fairsquarestech.com",
                        To = $"colin.monroe@fairsquarestech.com",
                        Message = emailBody,
                        MessageType = Models.MessageType.Email,
                        Subject = $"{Device.RuntimePlatform.ToString()} Critical Error"
                    });
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred while logging to email." + ex.ToString());
            }
        }
    }
}
