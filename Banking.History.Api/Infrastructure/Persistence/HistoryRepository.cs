using Banking.History.Api.Domain;
using Banking.History.Api.Infraestructure.Interface;
using Banking.History.Api.Infraestructure.Querys;
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
            //if (referenceDate.Kind != DateTimeKind.Utc)
            //    throw new ArgumentException(
            //        "Reference date must be in UTC.",
            //        nameof(referenceDate));

            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

            // Uma transação de leitura REPEATABLE READ mantém
            // as consultas sobre o mesmo snapshot do banco.
            await using var transaction = await connection.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead,cancellationToken);

            using var results = await connection.QueryMultipleAsync(
                new CommandDefinition(
                    QueryPersistence.sql,
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
