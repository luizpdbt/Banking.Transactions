using Banking.Barramento.Api.Domain;
using Banking.Barramento.Api.Enums;
using Banking.Barramento.Api.Infrastructure.Interface;
using Banking.Barramento.Api.Infrastructure.Querys;
using Dapper;
using Npgsql;

namespace Banking.Barramento.Api.Infrastructure.Persistence
{
    public class AccountRepository : IAccountRepository
    {
        private readonly NpgsqlDataSource _dataSource;

        public AccountRepository(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public async Task ExecuteMovementAccountTransactionAsync(AccountMovementDomain movement,CancellationToken cancellationToken)
        {
            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                var signedAmount = movement.OperationType switch
                {
                    OperationType.Credit => movement.Value,
                    OperationType.Debit => -movement.Value,
                    _ => throw new ArgumentException(
                        "Invalid operation type.")
                };

                var affectedRows = await connection.ExecuteAsync(
                    new CommandDefinition(
                        QueryPersistence.updateBalanceSql,
                        new
                        {
                            SignedAmount = signedAmount,
                            movement.AccountNumber
                        },
                        transaction: transaction,
                        cancellationToken: cancellationToken));

                if (affectedRows != 1)
                {
                    throw new InvalidOperationException(
                        "Account not found.");
                }

                await connection.ExecuteAsync(
                    new CommandDefinition(
                        QueryPersistence.insertTransactionSql,
                        new
                        {
                            movement.TransactionId,
                            movement.AccountNumber,
                            OperationType = (int)movement.OperationType,
                            movement.Value
                        },
                        transaction: transaction,
                        cancellationToken: cancellationToken));

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
        }
    }
}
