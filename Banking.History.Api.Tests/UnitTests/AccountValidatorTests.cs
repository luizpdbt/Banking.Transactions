using Banking.History.Api.Dto;
using Banking.History.Api.Validator;

namespace Banking.History.Api.Tests.IntegrationTests;

public class AccountValidatorTests
{
    private static DtoAccount CreateValidAccount()
    {
        return new DtoAccount
        {
            Account = "123456",
            ReferenceDate = new DateTime(
                2026, 11, 5, 17, 40, 0,
                DateTimeKind.Utc)
        };
    }

    [Fact]
    public void Validate_ValidAccount_ShouldReturnNoErrors()
    {
        // Arrange
        var account = CreateValidAccount();

        // Act
        var errors = AccountValidator.Validate(account);

        // Assert
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_InvalidAccount_ShouldReturnRequiredError(
        string? accountNumber)
    {
        // Arrange
        var account = CreateValidAccount();
        account.Account = accountNumber!;

        // Act
        var errors = AccountValidator.Validate(account);

        // Assert
        Assert.Single(errors);
        Assert.Contains(
            "Account number is required.",
            errors);
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("000001")]
    [InlineData("ABC123")]
    public void Validate_NonEmptyAccount_ShouldReturnNoErrors(
        string accountNumber)
    {
        // Arrange
        var account = CreateValidAccount();
        account.Account = accountNumber;

        // Act
        var errors = AccountValidator.Validate(account);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_WhitespaceAccount_ShouldReturnNoErrors()
    {
        // Arrange
        var account = CreateValidAccount();
        account.Account = "   ";

        // Act
        var errors = AccountValidator.Validate(account);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_DefaultReferenceDate_ShouldReturnNoErrors()
    {
        // Arrange
        var account = CreateValidAccount();
        account.ReferenceDate = default;

        // Act
        var errors = AccountValidator.Validate(account);

        // Assert
        Assert.Empty(errors);
    }
}