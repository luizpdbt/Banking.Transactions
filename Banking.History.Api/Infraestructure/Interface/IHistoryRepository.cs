using Banking.History.Api.Domain;

namespace Banking.History.Api.Infraestructure.Interface
{
    public interface IHistoryRepository
    {
        Task<AccountStatement?> GetStatementAsync(
        string accountNumber,
        DateTime referenceDate,
        CancellationToken cancellationToken = default);
    }
}
