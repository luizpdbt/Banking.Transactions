using Banking.Barramento.Api.Validator;
using Banking.Barramento.Api.Dto;
using Banking.Barramento.Api.Enums;
using Xunit;

namespace Banking.Barramento.Api.Tests.UnitTests;

public class OperationValidatorTests
{
    private static OperationDto CreateValidOperation()
    {
        return new OperationDto
        {
            TransactionId = Guid.NewGuid(),
            AccountNumber = "123456",
            Value = 100.00m,
            OperationType = OperationType.Credit
        };
    }

    [Fact]
    public void Validate_ValidOperation_ShouldReturnNoErrors()
    {
        // Arrange
        var operation = CreateValidOperation();

        // Act
        var errors = OperationValidator.Validate(operation);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_EmptyTransactionId_ShouldReturnError()
    {
        // Arrange
        var operation = CreateValidOperation();
        operation.TransactionId = Guid.Empty;

        // Act
        var errors = OperationValidator.Validate(operation);

        // Assert
        Assert.Contains("TransactionId is required.", errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_InvalidAccountNumber_ShouldReturnError(
        string? accountNumber)
    {
        // Arrange
        var operation = CreateValidOperation();
        operation.AccountNumber = accountNumber!;

        // Act
        var errors = OperationValidator.Validate(operation);

        // Assert
        Assert.Contains("AccountNumber is required.", errors);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_NonPositiveValue_ShouldReturnError(
        int value)
    {
        // Arrange
        var operation = CreateValidOperation();
        operation.Value = value;

        // Act
        var errors = OperationValidator.Validate(operation);

        // Assert
        Assert.Contains(
            "Value must be greater than zero.",
            errors);
    }

    [Theory]
    [InlineData("10.001")]
    [InlineData("100.123")]
    [InlineData("0.999")]
    public void Validate_ValueWithMoreThanTwoDecimalPlaces_ShouldReturnError(
        string value)
    {
        // Arrange
        var operation = CreateValidOperation();

        operation.Value = decimal.Parse(
            value,
            System.Globalization.CultureInfo.InvariantCulture);

        // Act
        var errors = OperationValidator.Validate(operation);

        // Assert
        Assert.Contains(
            "Value must have at most two decimal places.",
            errors);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(99)]
    [InlineData(-1)]
    public void Validate_InvalidOperationType_ShouldReturnError(
        int operationType)
    {
        // Arrange
        var operation = CreateValidOperation();
        operation.OperationType = (OperationType)operationType;

        // Act
        var errors = OperationValidator.Validate(operation);

        // Assert
        Assert.Contains("Invalid operation type.", errors);
    }

    [Theory]
    [InlineData("0.01")]
    [InlineData("10.10")]
    [InlineData("100.99")]
    public void Validate_ValidDecimalPrecision_ShouldReturnNoErrors(
        string value)
    {
        // Arrange
        var operation = CreateValidOperation();

        operation.Value = decimal.Parse(
            value,
            System.Globalization.CultureInfo.InvariantCulture);

        // Act
        var errors = OperationValidator.Validate(operation);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_MultipleInvalidFields_ShouldReturnAllErrors()
    {
        // Arrange
        var operation = new OperationDto
        {
            TransactionId = Guid.Empty,
            AccountNumber = "",
            Value = -10.123m,
            OperationType = OperationType.Unknown
        };

        // Act
        var errors = OperationValidator.Validate(operation);

        // Assert
        Assert.Equal(5, errors.Count);

        Assert.Contains("TransactionId is required.", errors);
        Assert.Contains("AccountNumber is required.", errors);
        Assert.Contains("Value must be greater than zero.", errors);
        Assert.Contains(
            "Value must have at most two decimal places.",
            errors);
        Assert.Contains("Invalid operation type.", errors);
    }

    [Theory]
    [InlineData(OperationType.Credit)]
    [InlineData(OperationType.Debit)]
    public void Validate_ValidOperationTypes_ShouldReturnNoErrors(
        OperationType operationType)
    {
        // Arrange
        var operation = CreateValidOperation();
        operation.OperationType = operationType;

        // Act
        var errors = OperationValidator.Validate(operation);

        // Assert
        Assert.Empty(errors);
    }
}