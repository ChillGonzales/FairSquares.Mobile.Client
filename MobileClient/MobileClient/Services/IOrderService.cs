using MobileClient.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MobileClient.Services
{
    public interface IOrderService
    {
        Task<string> AddOrder(Order order);
        Task<IEnumerable<Order>> GetMemberOrders(string memberId);
    }
}