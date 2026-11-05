using Banking.Barramento.Api.Dto;
using Banking.Barramento.Api.Enums;

namespace Banking.Barramento.Api.Validator
{
    public static class OperationValidator
    {
        public static List<string> Validate(OperationDto operation)
        {
            var errors = new List<string>();

            if (operation.TransactionId == Guid.Empty)
            {
                errors.Add("TransactionId is required.");
            }

            if (string.IsNullOrWhiteSpace(operation.AccountNumber))
            {
                errors.Add("AccountNumber is required.");
            }

            if (operation.Value <= 0)
            {
                errors.Add("Value must be greater than zero.");
            }

            if (decimal.Round(operation.Value, 2) != operation.Value)
            {
                errors.Add("Value must have at most two decimal places.");
            }

            if (!Enum.IsDefined(typeof(OperationType), operation.OperationType)
                || operation.OperationType == OperationType.Unknown)
            {
                errors.Add("Invalid operation type.");
            }

            return errors;
        }
    }
}
