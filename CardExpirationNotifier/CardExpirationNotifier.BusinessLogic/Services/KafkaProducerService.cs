using System.Text.Json;
using CardExpirationNotifier.DataStorage.Models;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace CardExpirationNotifier.BusinessLogic.Services;

public class KafkaProducerService : IKafkaProducerService, IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly string _topic;
    private readonly ILogger<KafkaProducerService> _logger;

    public KafkaProducerService(string bootstrapServers, string topic, ILogger<KafkaProducerService> logger)
    {
        _topic = topic;
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };

        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task SendCardExpirationNotificationAsync(PaymentCard card)
    {
        var message = new
        {
            cardMask = card.CardMask,
            expirationYear = card.ExpirationYear,
            expirationMonth = card.ExpirationMonth,
            cardType = card.CardType,
            userFirstName = card.UserFirstName,
            userLastName = card.UserLastName
        };

        var messageJson = JsonSerializer.Serialize(message);

        try
        {
            var result = await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = messageJson });
            _logger.LogInformation("Sent notification for card {CardMask} to Kafka topic {Topic}", card.CardMask, _topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification for card {CardMask} to Kafka", card.CardMask);
            throw;
        }
    }

    public void Dispose()
    {
        _producer?.Dispose();
    }
}
