namespace Banking.History.Api.Domain
{
    public class AccountStatement
    {
        public string AccountNumber { get; set; } = string.Empty;
        public DateTime ReferenceDate { get; set; }
        public decimal Balance { get; set; }
        public IReadOnlyList<AccountTransaction> Transactions { get; set; }
            = [];
    }
}
