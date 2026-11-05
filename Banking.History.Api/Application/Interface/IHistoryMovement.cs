using Banking.History.Api.Domain;
using Banking.History.Api.Dto;

namespace Banking.History.Api.Application.Interface
{
    public interface IHistoryMovement
    {
        Task<AccountStatement?> GetHistoryAsync(DtoAccount account,CancellationToken cancellationToken = default);
    }
}
