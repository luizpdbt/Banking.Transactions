using Banking.History.Api.Application.Common;
using Banking.History.Api.Domain;
using Banking.History.Api.Dto;

namespace Banking.History.Api.Application.Interface
{
    public interface IHistoryMovement
    {
        Task<PagedResult<AccountStatement>?> GetHistoryAsync(
        DtoAccount account,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    }
}
