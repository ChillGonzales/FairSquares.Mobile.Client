using System;
using System.Collections.Generic;
using System.Text;

namespace MobileClient.Utilities
{
    public interface IMessagingSubscriber
    {
        void Subscribe(List<string> topics);
        void Unsubscribe(List<string> topics);
    }
}
