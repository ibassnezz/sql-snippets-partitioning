namespace CardExpirationNotifier.BusinessLogic.Validators;

public static class CardValidator
{
    private const int MinYear = 20;  // 2020
    private const int MaxYear = 35;  // 2035
    private const int MinMonth = 1;
    private const int MaxMonth = 12;

    public static bool IsValidExpirationDate(int year, int month, out string? errorMessage)
    {
        errorMessage = null;

        if (month < MinMonth || month > MaxMonth)
        {
            errorMessage = "Expiration month must be between 1 and 12";
            return false;
        }

        if (year < MinYear || year > MaxYear)
        {
            errorMessage = "Hey, man, we don't support such cards";
            return false;
        }

        return true;
    }

    public static bool IsValidCardMask(string cardMask, out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(cardMask))
        {
            errorMessage = "Card mask cannot be empty";
            return false;
        }

        // Card mask should contain at least first 6 and last 4 digits
        var cleanMask = cardMask.Replace("*", "").Replace("X", "").Replace("x", "").Replace(" ", "");

        if (cleanMask.Length < 10)
        {
            errorMessage = "Card mask must contain at least first 6 and last 4 digits";
            return false;
        }

        return true;
    }
}
