using Moq;

using TRANSMUTANSTEIN.ChatServer.Services;

namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests;

public class ChatServiceTests
{
    [Test]
    public async Task StopAsync_WhenChatServerIsNull_LogsErrorAndThrows()
    {
        // Arrange
        Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>();
        Mock<ILogger<ChatService>> loggerMock = new Mock<ILogger<ChatService>>();

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ILogger<ChatService>)))
            .Returns(loggerMock.Object);

        // Initialize Static Log for Dispose() call which uses Log.Error
        global::TRANSMUTANSTEIN.ChatServer.Utilities.Log.Initialise(loggerMock.Object);

        // Create ChatService. Note: StartAsync is NOT called, so ChatServer property remains null.
        using ChatService chatService = new ChatService(serviceProviderMock.Object);

        // Act
        Task task = chatService.StopAsync(CancellationToken.None);

        // Assert
        await Assert.That(task.IsFaulted).IsTrue();
        await Assert.That(task.Exception?.InnerException).IsTypeOf<ApplicationException>();
        await Assert.That(task.Exception?.InnerException?.Message).IsEqualTo("Chat Server Is NULL");

        // Verify logging
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.Is<EventId>(e => e.Name == "LogChatServerNullDuringStop"),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
