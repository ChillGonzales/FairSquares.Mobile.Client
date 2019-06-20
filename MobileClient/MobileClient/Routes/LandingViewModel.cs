using MobileClient.Authentication;
using MobileClient.Services;
using MobileClient.Utilities;
using MobileClient.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Auth;
using Xamarin.Forms;

namespace MobileClient.Routes
{
    public class LandingViewModel : INotifyPropertyChanged
    {
        private readonly OAuth2Authenticator _auth;
        private readonly IToastService _toastService;
        private readonly MainThreadNavigator _nav;
        private bool _loadingAnimRunning;
        private bool _loadingAnimVisible;
        private bool _loginLayoutVisible;

        public LandingViewModel(OAuth2Authenticator auth, 
                                IToastService toastService,
                                MainThreadNavigator nav)
        {
            _auth = auth;
            _toastService = toastService;
            _nav = nav;
            _auth.Completed += Auth_Completed;
            _auth.Error += Auth_Error;
            LoginLayoutVisible = true;
            GoogleLoginCommand = new Command(() => LoginWithGoogle());
        }

        private void Auth_Completed(object sender, AuthenticatorCompletedEventArgs e)
        {
            _nav.Pop();
        }
        private void Auth_Error(object sender, AuthenticatorErrorEventArgs e)
        {
            _toastService.LongToast($"Oops! Looks like there was an issue. Please try again. Error: '{e.Message}'");
            LoadingAnimVisible = false;
            LoadingAnimRunning = false;
            LoginLayoutVisible = true;
        }
        private void LoginWithGoogle()
        {
            LoadingAnimVisible = true;
            LoadingAnimRunning = true;
            LoginLayoutVisible = false;
            var presenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();
            presenter.Login(_auth);
        }

        // Bound properties
        public event PropertyChangedEventHandler PropertyChanged;
        public bool LoadingAnimRunning
        {
            get
            {
                return _loadingAnimRunning;
            }
            set
            {
                _loadingAnimRunning = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LoadingAnimRunning)));
            }
        }
        public bool LoadingAnimVisible
        {
            get
            {
                return _loadingAnimVisible;
            }
            set
            {
                _loadingAnimVisible = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LoadingAnimVisible)));
            }
        }
        public bool LoginLayoutVisible
        {
            get
            {
                return _loginLayoutVisible;
            }
            set
            {
                _loginLayoutVisible = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LoginLayoutVisible)));
            }
        }
        public ICommand GoogleLoginCommand { get; set; }
    }
}
