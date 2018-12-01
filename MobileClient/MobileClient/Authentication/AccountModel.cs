using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Auth;

namespace MobileClient.Authentication
{
    public class AccountModel
    {
        public string Email { get; set; }
        public string UserId { get; set; }
        public Account Token { get; set; }
    }
}
