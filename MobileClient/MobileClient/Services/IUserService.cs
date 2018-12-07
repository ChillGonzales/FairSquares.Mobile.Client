using MobileClient.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MobileClient.Services
{
    public interface IUserService
    {
        UserModel GetUser(string userId);
        void AddUser(UserModel user);
    }
}
