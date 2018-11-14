using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MobileClient.Models;

namespace MobileClient.Services
{
    public class MockOrderService : IOrderService
    {
        public string AddOrder(Order order)
        {
            return "1234";
        }

        public IEnumerable<Order> GetMemberOrders(string memberId)
        {
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
