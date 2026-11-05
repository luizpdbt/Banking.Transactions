using Banking.Barramento.Api.Domain;
using Banking.Barramento.Api.Enums;
using Banking.Barramento.Api.Infrastructure.Interface;
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

        public async Task<bool> ExecuteMovementAccountTransactionAsync(AccountMovementDomain movement,
                                                                        CancellationToken cancellationToken)
        {

            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                const string updateBalanceSql = """
                UPDATE accounts
                SET balance = balance + @SignedAmount
                WHERE account_number = @AccountNumber;
                """;

                var signedAmount = movement.OperationType switch
                {
                    OperationType.Credit => movement.Value,
                    OperationType.Debit => -movement.Value,
                    _ => throw new ArgumentException(
                        "Invalid operation type.")
                };

                var affectedRows = await connection.ExecuteAsync(
                    new CommandDefinition(
                        updateBalanceSql,
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

                const string insertTransactionSql = """
                INSERT INTO transactions
                    (
                        transaction_id,
                        account_number,
                        operation_type,
                        amount
                    )
                VALUES
                    (
                        @TransactionId,
                        @AccountNumber,
                        @OperationType,
                        @Value
                    );
                """;

                await connection.ExecuteAsync(
                    new CommandDefinition(
                        insertTransactionSql,
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

                return true;
            }
            catch
            {
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
        }
    }
}
