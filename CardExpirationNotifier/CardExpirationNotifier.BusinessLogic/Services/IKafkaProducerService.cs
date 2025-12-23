using CardExpirationNotifier.DataStorage.Models;

namespace CardExpirationNotifier.BusinessLogic.Services;

public interface IKafkaProducerService
{
    Task SendCardExpirationNotificationAsync(PaymentCard card);
}
