using CardExpirationNotifier.DataStorage.Models;

namespace CardExpirationNotifier.BusinessLogic.Services;

public interface ICardService
{
    Task<PaymentCard> AddCardAsync(string cardMask, int expirationYear, int expirationMonth, string cardType, string userFirstName, string userLastName);
    Task<IEnumerable<PaymentCard>> GetCardsByDateRangeAsync(int startYear, int startMonth, int endYear, int endMonth, int offset, int limit);
    Task<IEnumerable<PaymentCard>> GetPendingNotificationsAsync(int offset, int limit);
    Task ProcessCardNotificationsAsync();
}
