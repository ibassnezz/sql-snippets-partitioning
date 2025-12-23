using CardExpirationNotifier.BusinessLogic.Utils;
using CardExpirationNotifier.BusinessLogic.Validators;
using CardExpirationNotifier.DataStorage.Models;
using CardExpirationNotifier.DataStorage.Repositories;
using Microsoft.Extensions.Logging;

namespace CardExpirationNotifier.BusinessLogic.Services;

public class CardService : ICardService
{
    private readonly ICardRepository _cardRepository;
    private readonly IKafkaProducerService _kafkaProducerService;
    private readonly ILogger<CardService> _logger;
    private readonly int _batchSize;

    public CardService(
        ICardRepository cardRepository,
        IKafkaProducerService kafkaProducerService,
        ILogger<CardService> logger,
        int batchSize = 10)
    {
        _cardRepository = cardRepository;
        _kafkaProducerService = kafkaProducerService;
        _logger = logger;
        _batchSize = batchSize;
    }

    public async Task<PaymentCard> AddCardAsync(string cardMask, int expirationYear, int expirationMonth, string cardType, string userFirstName, string userLastName)
    {
        // Validate card mask
        if (!CardValidator.IsValidCardMask(cardMask, out var cardMaskError))
        {
            throw new ArgumentException(cardMaskError, nameof(cardMask));
        }

        // Validate expiration date
        if (!CardValidator.IsValidExpirationDate(expirationYear, expirationMonth, out var dateError))
        {
            throw new ArgumentException(dateError);
        }

        // Generate card token
        var cardToken = CardTokenGenerator.Generate(cardMask, cardType);

        var card = new PaymentCard
        {
            CardToken = cardToken,
            CardMask = cardMask,
            ExpirationYear = expirationYear,
            ExpirationMonth = expirationMonth,
            CardType = cardType,
            UserFirstName = userFirstName,
            UserLastName = userLastName,
            NotificationSent = false,
            CreatedAt = DateTime.UtcNow
        };

        var cardId = await _cardRepository.AddCardAsync(card);
        card.Id = cardId;

        _logger.LogInformation("Added card {CardMask} with ID {CardId}", cardMask, cardId);

        return card;
    }

    public async Task<IEnumerable<PaymentCard>> GetCardsByDateRangeAsync(int startYear, int startMonth, int endYear, int endMonth, int offset, int limit)
    {
        return await _cardRepository.GetCardsByDateRangeAsync(startYear, startMonth, endYear, endMonth, offset, limit);
    }

    public async Task<IEnumerable<PaymentCard>> GetPendingNotificationsAsync(int offset, int limit)
    {
        return await _cardRepository.GetPendingNotificationsAsync(offset, limit);
    }

    public async Task ProcessCardNotificationsAsync()
    {
        var now = DateTime.UtcNow;
        var currentYear = now.Year % 100; // Get last 2 digits
        var currentMonth = now.Month;

        _logger.LogInformation("Processing card notifications for {Year}/{Month}", currentYear, currentMonth);

        var cards = await _cardRepository.GetCardsToNotifyAsync(currentYear, currentMonth, _batchSize);
        var cardsList = cards.ToList();

        if (!cardsList.Any())
        {
            _logger.LogInformation("No cards to notify");
            return;
        }

        _logger.LogInformation("Found {Count} cards to notify", cardsList.Count);

        foreach (var card in cardsList)
        {
            try
            {
                await _kafkaProducerService.SendCardExpirationNotificationAsync(card);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification for card {CardId}", card.Id);
                throw;
            }
        }

        var cardIds = cardsList.Select(c => c.Id);
        await _cardRepository.MarkAsNotifiedAsync(cardIds);

        _logger.LogInformation("Successfully notified {Count} cards", cardsList.Count);
    }
}
