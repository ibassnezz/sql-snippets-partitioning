using CardExpirationNotifier.DataStorage.Models;

namespace CardExpirationNotifier.DataStorage.Repositories;

public interface ICardRepository
{
    Task<long> AddCardAsync(PaymentCard card);
    Task<IEnumerable<PaymentCard>> GetCardsByDateRangeAsync(int startYear, int startMonth, int endYear, int endMonth, int offset, int limit);
    Task<IEnumerable<PaymentCard>> GetPendingNotificationsAsync(int offset, int limit);
    Task<IEnumerable<PaymentCard>> GetCardsToNotifyAsync(int year, int month, int limit);
    Task MarkAsNotifiedAsync(IEnumerable<long> cardIds);
}
