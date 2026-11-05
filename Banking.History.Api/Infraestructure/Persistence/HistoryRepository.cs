using Banking.History.Api.Domain;
using Banking.History.Api.Infraestructure.Interface;
using Dapper;
using Npgsql;

namespace Banking.History.Api.Infraestructure.Persistence
{
    public class HistoryRepository : IHistoryRepository
    {
        private readonly NpgsqlDataSource _dataSource;

        public HistoryRepository(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public async Task<AccountStatement?> GetStatementAsync(string accountNumber,DateTime referenceDate, CancellationToken cancellationToken = default)
        {

            if (string.IsNullOrWhiteSpace(accountNumber))
                throw new ArgumentException(
                    "Account number is required.",
                    nameof(accountNumber));

            if (referenceDate.Kind != DateTimeKind.Utc)
                throw new ArgumentException(
                    "Reference date must be in UTC.",
                    nameof(referenceDate));

            const string sql = """
                        SELECT EXISTS (
                            SELECT 1
                            FROM accounts
                            WHERE account_number = @AccountNumber
                        );

                        SELECT
                            transaction_id AS TransactionId,
                            account_number AS AccountNumber,
                            operation_type AS OperationType,
                            amount AS Amount,
                            created_at AS CreatedAt
                        FROM transactions
                        WHERE account_number = @AccountNumber
                          AND created_at <= @ReferenceDate
                        ORDER BY created_at, transaction_id;

                        SELECT COALESCE(
                            SUM(
                                CASE
                                    WHEN operation_type = 1 THEN amount
                                    WHEN operation_type = 2 THEN -amount
                                    ELSE 0
                                END
                            ),
                            0
                        )
                        FROM transactions
                        WHERE account_number = @AccountNumber
                          AND created_at <= @ReferenceDate;
                        """;

            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

            // Uma transação de leitura REPEATABLE READ mantém
            // as consultas sobre o mesmo snapshot do banco.
            await using var transaction =
                await connection.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead,cancellationToken);

            using var results = await connection.QueryMultipleAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        AccountNumber = accountNumber,
                        ReferenceDate = referenceDate
                    },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            var accountExists = await results.ReadSingleAsync<bool>();

            if (!accountExists)
                return null;

            var transactions = (
                await results.ReadAsync<AccountTransaction>()
            ).AsList();

            var balance = await results.ReadSingleAsync<decimal>();

            await transaction.CommitAsync(cancellationToken);

            return new AccountStatement
            {
                AccountNumber = accountNumber,
                ReferenceDate = referenceDate,
                Balance = balance,
                Transactions = transactions
            };
        }
    }
}
