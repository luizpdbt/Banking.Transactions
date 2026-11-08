using Banking.History.Api.Application.Common;

namespace Banking.History.Api.Domain
{
    public class AccountStatement
    {
        public string AccountNumber { get; set; } = string.Empty;
        public DateTime ReferenceDate { get; set; }
        public decimal Balance { get; set; }
        public PagedResult<AccountTransaction> Transactions
        { get; init; } = default!;
    }
}
