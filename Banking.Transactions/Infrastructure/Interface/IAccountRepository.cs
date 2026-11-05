using Banking.Barramento.Api.Domain;

namespace Banking.Barramento.Api.Infrastructure.Interface
{
    public interface IAccountRepository
    {
        Task<bool> ExecuteMovementAccountTransactionAsync(
        AccountMovementDomain movement,
        CancellationToken cancellationToken);
    }
}
