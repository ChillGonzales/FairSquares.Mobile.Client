﻿using MobileClient.Authentication;
using MobileClient.Services;
using MobileClient.Utilities;
using MobileClient.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MobileClient.Routes
{
    public class FeedbackViewModel : INotifyPropertyChanged
    {
        private readonly INotificationService _notifier;
        private readonly ICurrentUserService _userCache;
        private readonly MainThreadNavigator _nav;
        private readonly ILogger<FeedbackViewModel> _logger;
        private string _feedbackEntry;

        public FeedbackViewModel(INotificationService notifier,
                                 ICurrentUserService userCache,
                                 ILogger<FeedbackViewModel> logger,
                                 MainThreadNavigator nav)
        {
            _notifier = notifier;
            _userCache = userCache;
            _nav = nav;
            _logger = logger;

            SubmitCommand = new Command(() => SubmitFeedback(_feedbackEntry));
        }

        private void SubmitFeedback(string feedback)
        {
            if (string.IsNullOrWhiteSpace(feedback))
            {
                return;
            }
            try
            {
                var user = _userCache.GetLoggedInAccount();
                _notifier.Notify(new Models.NotificationRequest()
                {
                    From = "feedback@fairsquarestech.com",
                    To = "colin.monroe@fairsquarestech.com",
                    Message = feedback,
                    MessageType = Models.MessageType.Email,
                    Subject = "Feedback from " + user?.Email
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to submit feedback.", ex, $"Feedback: {feedback}");
            }
            _nav.Pop();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public string FeedbackEntry
        {
            get
            {
                return _feedbackEntry;
            }
            set
            {
                _feedbackEntry = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FeedbackEntry)));
            }
        }
        public ICommand SubmitCommand { get; private set; }
    }
}
