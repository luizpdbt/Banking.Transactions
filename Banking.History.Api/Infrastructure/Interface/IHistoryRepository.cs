using Banking.History.Api.Application.Common;
using Banking.History.Api.Domain;

namespace Banking.History.Api.Infrastructure.Interface
{
    public interface IHistoryRepository
    {
        Task<PagedResult<AccountStatement>?> GetStatementAsync(
        string accountNumber,
        DateTime referenceDate,
         int page, 
         int pageSize,
        CancellationToken cancellationToken = default);
    }
}
