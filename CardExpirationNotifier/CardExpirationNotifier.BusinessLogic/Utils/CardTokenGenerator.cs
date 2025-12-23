using System.Security.Cryptography;
using System.Text;

namespace CardExpirationNotifier.BusinessLogic.Utils;

public static class CardTokenGenerator
{
    public static string Generate(string cardMask, string cardType)
    {
        // Extract first 6 and last 4 from card mask
        // Card mask format: "411111******1111" or "411111XXXXXX1111"
        var cleanMask = cardMask.Replace("*", "").Replace("X", "").Replace("x", "");

        string first6;
        string last4;

        if (cleanMask.Length >= 10)
        {
            first6 = cleanMask.Substring(0, 6);
            last4 = cleanMask.Substring(cleanMask.Length - 4, 4);
        }
        else
        {
            throw new ArgumentException("Card mask must contain at least first 6 and last 4 digits", nameof(cardMask));
        }

        var input = $"{first6}{last4}{cardType}";
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
