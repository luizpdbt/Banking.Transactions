using Banking.Barramento.Api.Enums;

namespace Banking.Barramento.Api.Domain
{
    public class AccountMovementDomain
    {
        public Guid TransactionId { get; set; }
        public OperationType OperationType { get; set; }
        public decimal Value { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
    }
}
