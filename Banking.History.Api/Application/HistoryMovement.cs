using Banking.History.Api.Application.Common;
using Banking.History.Api.Application.Interface;
using Banking.History.Api.Domain;
using Banking.History.Api.Dto;
using Banking.History.Api.Infrastructure.Interface;

namespace Banking.History.Api.Application
{
    public class HistoryMovement : IHistoryMovement
    {
        private readonly IHistoryRepository _historyRepository;

        public HistoryMovement(IHistoryRepository historyRepository)
        {
            _historyRepository = historyRepository;
        }

        public async Task<PagedResult<AccountStatement>?> GetHistoryAsync(DtoAccount account,
                                                            int page,
                                                            int pagesize ,
                                                            CancellationToken cancellationToken = default)
        {

            return await _historyRepository.GetStatementAsync(account.Account, account.ReferenceDate, page,pagesize, cancellationToken);
        }
    }
}
