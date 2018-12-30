using MobileClient.Authentication;
using MobileClient.Services;
using MobileClient.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FeedbackPage : ContentPage
    {
        private readonly INotificationService _notifier;
        private readonly ICurrentUserService _userCache;
        private readonly IMessage _toast;

        public FeedbackPage()
        {
            InitializeComponent();
            _notifier = App.Container.GetInstance<INotificationService>();
            _userCache = App.Container.GetInstance<ICurrentUserService>();
            _toast = DependencyService.Get<IMessage>();
            SubmitButton.Clicked += SubmitButton_Clicked;
        }

        private void SubmitButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(FeedbackEditor.Text))
                {
                    // TODO: Display error
                    return;
                }
                var user = _userCache.GetLoggedInAccount();
                _notifier.Notify(new Models.NotificationRequest()
                {
                    From = "feedback@fairsquarestech.com",
                    To = "colin.monroe@fairsquarestech.com",
                    Message = FeedbackEditor.Text,
                    MessageType = Models.MessageType.Email,
                    Subject = "Feedback from " + user.Email
                });
                _toast.ShortAlert($"Thank you for your feedback!");
            }
            catch (Exception ex)
            {
                // log
            }
            finally
            {
                Navigation.PopAsync();
            }
        }
    }
}