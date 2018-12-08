using System;
using System.Collections.Generic;
using System.Text;

namespace MobileClient.Models
{
    public class NotificationRequest
    {
        public string To { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public MessageType MessageType { get; set; }
    }

    public enum MessageType
    {
        Text = 0,
        Email = 1
    }
}
