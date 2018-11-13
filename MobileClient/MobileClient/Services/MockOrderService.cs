using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MobileClient.Models;

namespace MobileClient.Services
{
    public class MockOrderService : IOrderService
    {
        public async Task<string> AddOrder(Order order)
        {
            await Task.Delay(50);
            return "1234";
        }

        public async Task<IEnumerable<Order>> GetMemberOrders(string memberId)
        {
            await Task.Delay(50);
            return new List<Order>()
            {
                new Order() {StreetAddress = "123 Street Drive"},
                new Order() {StreetAddress = "234 Example Drive"},
                new Order() {StreetAddress = "234 Example Drive"},
                new Order() {StreetAddress = "234 Example Drive"},
                new Order() {StreetAddress = "234 Example Drive"},
                new Order() {StreetAddress = "234 Example Drive"},
                new Order() {StreetAddress = "234 Example Drive"},
                new Order() {StreetAddress = "234 Example Drive"},
                new Order() {StreetAddress = "234 Example Drive"},
                new Order() {StreetAddress = "234 Example Drive"},
                new Order() {StreetAddress = "234 Example Drive"},
                new Order() {StreetAddress = "234 Example Drive"},
                new Order() {StreetAddress = "234 Example Drive"},
                new Order() {StreetAddress = "234 Example Drive"},
                new Order() {StreetAddress = "234 Example Drive"},
                new Order() {StreetAddress = "234 Example Drive"},
                new Order() {StreetAddress = "234 Example Drive"},
                new Order() {StreetAddress = "234 Example Drive"},
                new Order() {StreetAddress = "234 Example Drive"},
                new Order() {StreetAddress = "234 Example Drive"},
                new Order() {StreetAddress = "234 Example Drive"},
                new Order() {StreetAddress = "234 Example Drive"},
                new Order() {StreetAddress = "234 Example Drive"},
                new Order() {StreetAddress = "234 Example Drive"},
                new Order() {StreetAddress = "234 Example Drive"}
            };
        }
    }
}
