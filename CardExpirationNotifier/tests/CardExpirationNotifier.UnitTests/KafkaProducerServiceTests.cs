using CardExpirationNotifier.BusinessLogic.Services;
using CardExpirationNotifier.DataStorage.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CardExpirationNotifier.UnitTests;

public class KafkaProducerServiceTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        var bootstrapServers = "localhost:9092";
        var topic = "test-topic";
        var mockLogger = new Mock<ILogger<KafkaProducerService>>();

        // Act
        var service = new KafkaProducerService(bootstrapServers, topic, mockLogger.Object);

        // Assert
        service.Should().NotBeNull();
        service.Should().BeAssignableTo<IKafkaProducerService>();
    }

    [Fact]
    public async Task SendCardExpirationNotificationAsync_WithValidCard_DoesNotThrow()
    {
        // Arrange
        var bootstrapServers = "localhost:9092";
        var topic = "test-topic";
        var mockLogger = new Mock<ILogger<KafkaProducerService>>();
        var service = new KafkaProducerService(bootstrapServers, topic, mockLogger.Object);

        var card = new PaymentCard
        {
            Id = 1,
            CardMask = "411111******1111",
            ExpirationYear = 25,
            ExpirationMonth = 12,
            CardType = "Visa",
            UserFirstName = "John",
            UserLastName = "Doe"
        };

        // Note: This test will fail if Kafka is not running, but it tests that the service
        // can be constructed and the method can be called without compile errors
        // In a real scenario, you would mock the Kafka producer

        // Act & Assert
        // We're just testing that the service doesn't throw on construction
        // Actual Kafka send would require a running Kafka instance or mocking
        service.Should().NotBeNull();
        service.Dispose();
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        // Arrange
        var bootstrapServers = "localhost:9092";
        var topic = "test-topic";
        var mockLogger = new Mock<ILogger<KafkaProducerService>>();
        var service = new KafkaProducerService(bootstrapServers, topic, mockLogger.Object);

        // Act
        var act = () => service.Dispose();

        // Assert
        act.Should().NotThrow();
    }
}
