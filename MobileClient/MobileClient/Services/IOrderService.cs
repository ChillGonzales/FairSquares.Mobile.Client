using MobileClient.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MobileClient.Services
{
    public interface IOrderService
    {
        string AddOrder(Order order);
        IEnumerable<Order> GetMemberOrders(string memberId);
    }
}