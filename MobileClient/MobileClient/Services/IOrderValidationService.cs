using MobileClient.Authentication;
using MobileClient.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MobileClient.Services
{
    public interface IOrderValidationService
    {
        Task<ValidationModel> ValidateOrderRequest(AccountModel user, bool useCached = true);
    }
}
