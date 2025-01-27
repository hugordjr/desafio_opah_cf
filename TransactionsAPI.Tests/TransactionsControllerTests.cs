using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Threading.Tasks;
using TransactionsAPI.Application;
using TransactionsAPI.Controllers;
using TransactionsAPI.Domain;
using Xunit;

namespace TransactionsAPI.Tests
{
    public class TransactionsControllerTests
    {
        private readonly Mock<ITransactionService> _serviceMock;
        private readonly TransactionsController _controller;

        public TransactionsControllerTests()
        {
            _serviceMock = new Mock<ITransactionService>();
            _controller = new TransactionsController(_serviceMock.Object);
        }

        [Fact]
        public async Task Create_ValidRequest_ShouldReturnAccepted()
        {
            // Arrange
            var request = new TransactionRequest
            {
                Description = "Test Transaction",
                Amount = 100,
                Date = DateTime.UtcNow,
                Type = "Credit"
            };

            var traceId = Guid.NewGuid().ToString();
            _serviceMock.Setup(service => service.CreateTransaction(request)).ReturnsAsync(traceId);

            // Act
            var result = await _controller.Create(request);

            // Assert
            var acceptedResult = Assert.IsType<AcceptedResult>(result);
            Assert.Equal(traceId, ((dynamic)acceptedResult.Value).TraceId);
        }

        [Fact]
        public async Task Create_InvalidRequest_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new TransactionRequest
            {
                Description = "",
                Amount = 0,
                Date = DateTime.UtcNow,
                Type = null
            };

            _serviceMock
                .Setup(service => service.CreateTransaction(request))
                .ThrowsAsync(new ArgumentException("Invalid transaction data."));

            // Act
            var result = await _controller.Create(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid transaction data.", badRequestResult.Value);
        }
    }
}
