using MobileClient.Services;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    [TestFixture]
    public class AzureOrderServiceTests
    {
        private Mock<HttpMessageHandler> _handler;

        private HttpClient GetClient(string expectedResult, System.Net.HttpStatusCode expectedCode)
        {
            _handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    Content = new StringContent(expectedResult),
                    StatusCode = expectedCode
                }).Verifiable();
            return new HttpClient(_handler.Object);
        }

        private async Task WhenAddingOrder_GetsNewOrderId()
        {
            var expectedId = "1234";
            var order = new AzureOrderService(GetClient(expectedId, System.Net.HttpStatusCode.OK), "1234", "111");
            var result = await order.AddOrder(new MobileClient.Models.Order());
            Assert.AreEqual(expectedId, result);
            _handler.VerifyAll();
        }
    }
}
