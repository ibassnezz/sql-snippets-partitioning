using CardExpirationNotifier.BusinessLogic.Utils;
using FluentAssertions;
using Xunit;

namespace CardExpirationNotifier.UnitTests;

public class CardTokenGeneratorTests
{
    [Fact]
    public void Generate_WithValidCardMaskAndType_ReturnsValidSha256Hash()
    {
        // Arrange
        var cardMask = "411111******1111";
        var cardType = "Visa";

        // Act
        var token = CardTokenGenerator.Generate(cardMask, cardType);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Should().HaveLength(64); // SHA256 produces 64 hex characters
        token.Should().MatchRegex("^[a-f0-9]{64}$"); // Should be lowercase hex
    }

    [Fact]
    public void Generate_WithSameInputs_ReturnsSameToken()
    {
        // Arrange
        var cardMask = "411111******1111";
        var cardType = "Visa";

        // Act
        var token1 = CardTokenGenerator.Generate(cardMask, cardType);
        var token2 = CardTokenGenerator.Generate(cardMask, cardType);

        // Assert
        token1.Should().Be(token2);
    }

    [Fact]
    public void Generate_WithDifferentCardMasks_ReturnsDifferentTokens()
    {
        // Arrange
        var cardMask1 = "411111******1111";
        var cardMask2 = "522222******2222";
        var cardType = "Visa";

        // Act
        var token1 = CardTokenGenerator.Generate(cardMask1, cardType);
        var token2 = CardTokenGenerator.Generate(cardMask2, cardType);

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void Generate_WithDifferentCardTypes_ReturnsDifferentTokens()
    {
        // Arrange
        var cardMask = "411111******1111";
        var cardType1 = "Visa";
        var cardType2 = "MasterCard";

        // Act
        var token1 = CardTokenGenerator.Generate(cardMask, cardType1);
        var token2 = CardTokenGenerator.Generate(cardMask, cardType2);

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void Generate_WithDifferentMaskFormats_ReturnsSameToken()
    {
        // Arrange
        var cardMask1 = "411111******1111";
        var cardMask2 = "411111XXXXXX1111";
        var cardType = "Visa";

        // Act
        var token1 = CardTokenGenerator.Generate(cardMask1, cardType);
        var token2 = CardTokenGenerator.Generate(cardMask2, cardType);

        // Assert
        token1.Should().Be(token2);
    }

    [Fact]
    public void Generate_WithInvalidCardMask_ThrowsArgumentException()
    {
        // Arrange
        var cardMask = "4111"; // Too short
        var cardType = "Visa";

        // Act
        var act = () => CardTokenGenerator.Generate(cardMask, cardType);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Card mask must contain at least first 6 and last 4 digits*");
    }
}
