using Banking.Barramento.Api.Application.Interface;
using Banking.Barramento.Api.Dto;
using Banking.Barramento.Api.Infrastructure.Interface;

namespace Banking.Barramento.Api.Application
{
    public class AccountMovement: IAccountMovement
    {
        private readonly IAccountRepository _accountRepository;
        public AccountMovement(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task MoveAccount(OperationDto operation, CancellationToken cancellation)
        {
            var domainAccount = new Domain.AccountMovementDomain
            {
                TransactionId = operation.TransactionId,
                OperationType = operation.OperationType,
                Value = operation.Value,
                AccountNumber = operation.AccountNumber
            };
            await _accountRepository.ExecuteMovementAccountTransactionAsync(domainAccount, cancellation);
        }
    }
}
