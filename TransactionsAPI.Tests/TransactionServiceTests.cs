using Moq;
using System;
using System.Threading.Tasks;
using TransactionsAPI.Application;
using TransactionsAPI.Domain;
using TransactionsAPI.Infrastructure;
using Xunit;

namespace TransactionsAPI.Tests
{
    public class TransactionServiceTests
    {
        private readonly Mock<IMessageBroker> _messageBrokerMock;
        private readonly Mock<ITransactionCache> _cacheMock;
        private readonly Mock<ITransactionRepository> _repositoryMock;
        private readonly TransactionService _service;

        public TransactionServiceTests()
        {
            _messageBrokerMock = new Mock<IMessageBroker>();
            _cacheMock = new Mock<ITransactionCache>();
            _repositoryMock = new Mock<ITransactionRepository>();

            _service = new TransactionService(_messageBrokerMock.Object, _cacheMock.Object, _repositoryMock.Object);
        }

        [Fact]
        public async Task CreateTransaction_ValidRequest_ShouldReturnTraceId()
        {
            // Arrange
            var request = new TransactionRequest
            {
                Description = "Test Transaction",
                Amount = 100,
                Date = DateTime.UtcNow,
                Type = "Credit"
            };

            _messageBrokerMock
                .Setup(broker => broker.PublishAsync(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(Task.CompletedTask);

            // Act
            var traceId = await _service.CreateTransaction(request);

            // Assert
            Assert.False(string.IsNullOrEmpty(traceId));
            _cacheMock.Verify(cache => cache.SetStatus(traceId, "creating"), Times.Once);
            _messageBrokerMock.Verify(broker => broker.PublishAsync("CreateNewTransactionCommand", It.IsAny<CreateNewTransactionCommand>()), Times.Once);
        }

        [Fact]
        public async Task CreateTransaction_InvalidRequest_ShouldThrowArgumentException()
        {
            // Arrange
            var request = new TransactionRequest
            {
                Description = "",
                Amount = 0,
                Date = DateTime.UtcNow,
                Type = null
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateTransaction(request));
        }
    }
}
