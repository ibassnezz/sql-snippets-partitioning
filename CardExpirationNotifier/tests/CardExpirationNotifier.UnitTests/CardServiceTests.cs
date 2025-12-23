using CardExpirationNotifier.BusinessLogic.Services;
using CardExpirationNotifier.DataStorage.Models;
using CardExpirationNotifier.DataStorage.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CardExpirationNotifier.UnitTests;

public class CardServiceTests
{
    private readonly Mock<ICardRepository> _mockRepository;
    private readonly Mock<IKafkaProducerService> _mockKafkaProducer;
    private readonly Mock<ILogger<CardService>> _mockLogger;
    private readonly CardService _cardService;

    public CardServiceTests()
    {
        _mockRepository = new Mock<ICardRepository>();
        _mockKafkaProducer = new Mock<IKafkaProducerService>();
        _mockLogger = new Mock<ILogger<CardService>>();
        _cardService = new CardService(_mockRepository.Object, _mockKafkaProducer.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task AddCardAsync_WithValidData_ReturnsCard()
    {
        // Arrange
        var cardMask = "411111******1111";
        var expirationYear = 25;
        var expirationMonth = 12;
        var cardType = "Visa";
        var userFirstName = "John";
        var userLastName = "Doe";
        var expectedCardId = 123L;

        _mockRepository.Setup(r => r.AddCardAsync(It.IsAny<PaymentCard>()))
            .ReturnsAsync(expectedCardId);

        // Act
        var result = await _cardService.AddCardAsync(cardMask, expirationYear, expirationMonth, cardType, userFirstName, userLastName);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(expectedCardId);
        result.CardMask.Should().Be(cardMask);
        result.ExpirationYear.Should().Be(expirationYear);
        result.ExpirationMonth.Should().Be(expirationMonth);
        result.CardType.Should().Be(cardType);
        result.UserFirstName.Should().Be(userFirstName);
        result.UserLastName.Should().Be(userLastName);
        result.NotificationSent.Should().BeFalse();
        result.CardToken.Should().NotBeNullOrEmpty();

        _mockRepository.Verify(r => r.AddCardAsync(It.IsAny<PaymentCard>()), Times.Once);
    }

    [Fact]
    public async Task AddCardAsync_WithInvalidExpirationYear_ThrowsArgumentException()
    {
        // Arrange
        var cardMask = "411111******1111";
        var expirationYear = 19; // Too old
        var expirationMonth = 12;
        var cardType = "Visa";

        // Act
        var act = async () => await _cardService.AddCardAsync(cardMask, expirationYear, expirationMonth, cardType, "John", "Doe");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Hey, man, we don't support such cards*");

        _mockRepository.Verify(r => r.AddCardAsync(It.IsAny<PaymentCard>()), Times.Never);
    }

    [Fact]
    public async Task AddCardAsync_WithInvalidCardMask_ThrowsArgumentException()
    {
        // Arrange
        var cardMask = "4111"; // Too short
        var expirationYear = 25;
        var expirationMonth = 12;
        var cardType = "Visa";

        // Act
        var act = async () => await _cardService.AddCardAsync(cardMask, expirationYear, expirationMonth, cardType, "John", "Doe");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Card mask must contain at least first 6 and last 4 digits*");

        _mockRepository.Verify(r => r.AddCardAsync(It.IsAny<PaymentCard>()), Times.Never);
    }

    [Fact]
    public async Task GetCardsByDateRangeAsync_CallsRepository()
    {
        // Arrange
        var startYear = 25;
        var startMonth = 1;
        var endYear = 25;
        var endMonth = 12;
        var offset = 0;
        var limit = 50;
        var expectedCards = new List<PaymentCard>
        {
            new() { Id = 1, CardMask = "411111******1111", ExpirationYear = 25, ExpirationMonth = 6 }
        };

        _mockRepository.Setup(r => r.GetCardsByDateRangeAsync(startYear, startMonth, endYear, endMonth, offset, limit))
            .ReturnsAsync(expectedCards);

        // Act
        var result = await _cardService.GetCardsByDateRangeAsync(startYear, startMonth, endYear, endMonth, offset, limit);

        // Assert
        result.Should().BeEquivalentTo(expectedCards);
        _mockRepository.Verify(r => r.GetCardsByDateRangeAsync(startYear, startMonth, endYear, endMonth, offset, limit), Times.Once);
    }

    [Fact]
    public async Task ProcessCardNotificationsAsync_WithCardsToNotify_SendsToKafkaAndMarksAsNotified()
    {
        // Arrange
        var cards = new List<PaymentCard>
        {
            new() { Id = 1, CardMask = "411111******1111", ExpirationYear = 25, ExpirationMonth = 12, NotificationSent = false },
            new() { Id = 2, CardMask = "522222******2222", ExpirationYear = 25, ExpirationMonth = 12, NotificationSent = false }
        };

        _mockRepository.Setup(r => r.GetCardsToNotifyAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(cards);

        // Act
        await _cardService.ProcessCardNotificationsAsync();

        // Assert
        _mockKafkaProducer.Verify(k => k.SendCardExpirationNotificationAsync(It.IsAny<PaymentCard>()), Times.Exactly(2));
        _mockRepository.Verify(r => r.MarkAsNotifiedAsync(It.Is<IEnumerable<long>>(ids => ids.Count() == 2)), Times.Once);
    }

    [Fact]
    public async Task ProcessCardNotificationsAsync_WithNoCardsToNotify_DoesNotSendOrMark()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetCardsToNotifyAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<PaymentCard>());

        // Act
        await _cardService.ProcessCardNotificationsAsync();

        // Assert
        _mockKafkaProducer.Verify(k => k.SendCardExpirationNotificationAsync(It.IsAny<PaymentCard>()), Times.Never);
        _mockRepository.Verify(r => r.MarkAsNotifiedAsync(It.IsAny<IEnumerable<long>>()), Times.Never);
    }
}
