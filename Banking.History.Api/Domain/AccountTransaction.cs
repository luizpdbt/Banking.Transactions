namespace Banking.History.Api.Domain
{
    public class AccountTransaction
    {
        public Guid TransactionId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public int OperationType { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
