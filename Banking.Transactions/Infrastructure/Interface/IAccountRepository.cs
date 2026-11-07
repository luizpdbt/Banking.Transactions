using Banking.Barramento.Api.Domain;

namespace Banking.Barramento.Api.Infrastructure.Interface
{
    public interface IAccountRepository
    {
        Task ExecuteMovementAccountTransactionAsync(
        AccountMovementDomain movement,
        CancellationToken cancellationToken);
    }
}
