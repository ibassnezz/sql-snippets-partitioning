namespace CardExpirationNotifier.DataStorage.Models;

public class PaymentCard
{
    public long Id { get; set; }
    public string CardToken { get; set; } = string.Empty;
    public string CardMask { get; set; } = string.Empty;
    public int ExpirationYear { get; set; }
    public int ExpirationMonth { get; set; }
    public string CardType { get; set; } = string.Empty;
    public string UserFirstName { get; set; } = string.Empty;
    public string UserLastName { get; set; } = string.Empty;
    public bool NotificationSent { get; set; }
    public DateTime CreatedAt { get; set; }
}
