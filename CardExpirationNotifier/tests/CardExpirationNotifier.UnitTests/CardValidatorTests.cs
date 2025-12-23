using CardExpirationNotifier.BusinessLogic.Validators;
using FluentAssertions;
using Xunit;

namespace CardExpirationNotifier.UnitTests;

public class CardValidatorTests
{
    [Theory]
    [InlineData(20, 1, true, null)]
    [InlineData(25, 6, true, null)]
    [InlineData(35, 12, true, null)]
    [InlineData(19, 12, false, "Hey, man, we don't support such cards")]
    [InlineData(36, 1, false, "Hey, man, we don't support such cards")]
    [InlineData(25, 0, false, "Expiration month must be between 1 and 12")]
    [InlineData(25, 13, false, "Expiration month must be between 1 and 12")]
    public void IsValidExpirationDate_WithVariousInputs_ReturnsExpectedResult(
        int year,
        int month,
        bool expectedIsValid,
        string? expectedErrorMessage)
    {
        // Act
        var isValid = CardValidator.IsValidExpirationDate(year, month, out var errorMessage);

        // Assert
        isValid.Should().Be(expectedIsValid);
        errorMessage.Should().Be(expectedErrorMessage);
    }

    [Theory]
    [InlineData("411111******1111", true, null)]
    [InlineData("411111XXXXXX1111", true, null)]
    [InlineData("522222******2222", true, null)]
    [InlineData("4111", false, "Card mask must contain at least first 6 and last 4 digits")]
    [InlineData("", false, "Card mask cannot be empty")]
    [InlineData(null, false, "Card mask cannot be empty")]
    [InlineData("   ", false, "Card mask cannot be empty")]
    public void IsValidCardMask_WithVariousInputs_ReturnsExpectedResult(
        string? cardMask,
        bool expectedIsValid,
        string? expectedErrorMessage)
    {
        // Act
        var isValid = CardValidator.IsValidCardMask(cardMask!, out var errorMessage);

        // Assert
        isValid.Should().Be(expectedIsValid);
        errorMessage.Should().Be(expectedErrorMessage);
    }

    [Fact]
    public void IsValidExpirationDate_WithMinimumBoundary_ReturnsTrue()
    {
        // Arrange
        var year = 20;
        var month = 1;

        // Act
        var isValid = CardValidator.IsValidExpirationDate(year, month, out var errorMessage);

        // Assert
        isValid.Should().BeTrue();
        errorMessage.Should().BeNull();
    }

    [Fact]
    public void IsValidExpirationDate_WithMaximumBoundary_ReturnsTrue()
    {
        // Arrange
        var year = 35;
        var month = 12;

        // Act
        var isValid = CardValidator.IsValidExpirationDate(year, month, out var errorMessage);

        // Assert
        isValid.Should().BeTrue();
        errorMessage.Should().BeNull();
    }

    [Fact]
    public void IsValidExpirationDate_WithYearBelowMinimum_ReturnsCustomErrorMessage()
    {
        // Arrange
        var year = 19;
        var month = 12;

        // Act
        var isValid = CardValidator.IsValidExpirationDate(year, month, out var errorMessage);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Be("Hey, man, we don't support such cards");
    }

    [Fact]
    public void IsValidExpirationDate_WithYearAboveMaximum_ReturnsCustomErrorMessage()
    {
        // Arrange
        var year = 36;
        var month = 1;

        // Act
        var isValid = CardValidator.IsValidExpirationDate(year, month, out var errorMessage);

        // Assert
        isValid.Should().BeFalse();
        errorMessage.Should().Be("Hey, man, we don't support such cards");
    }
}
