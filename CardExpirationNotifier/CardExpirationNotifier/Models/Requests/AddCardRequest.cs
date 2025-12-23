namespace CardExpirationNotifier.Models.Requests;

public class AddCardRequest
{
    public string CardMask { get; set; } = string.Empty;
    public int ExpirationYear { get; set; }
    public int ExpirationMonth { get; set; }
    public string CardType { get; set; } = string.Empty;
    public string UserFirstName { get; set; } = string.Empty;
    public string UserLastName { get; set; } = string.Empty;
}
