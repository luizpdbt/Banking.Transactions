using Banking.History.Api.Application.Common;
using Banking.History.Api.Domain;
using Banking.History.Api.Infraestructure.Interface;
using Banking.History.Api.Infraestructure.Querys;
using Dapper;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        public async Task<PagedResult<AccountStatement?>> GetStatementAsync(string accountNumber,
                                                                            DateTime referenceDate, 
                                                                            int page,
                                                                            int pageSize, 
                                                                            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
                throw new ArgumentException(
                    "Account number is required.",
                    nameof(accountNumber));

            if (page < 1)
                throw new ArgumentOutOfRangeException(nameof(page));

            if (pageSize < 1 || pageSize > 100)
                throw new ArgumentOutOfRangeException(nameof(pageSize));

            // A data de referência é interpretada como dia UTC.
            var startDate = new DateTimeOffset(
                referenceDate.Date,
                TimeSpan.Zero);

            var endDate = startDate.AddDays(1);

            var offset = ((long)page - 1) * pageSize;

            await using var connection =
                await _dataSource.OpenConnectionAsync(cancellationToken);

            // Mantém as consultas no mesmo snapshot do banco.
            await using var transaction =
                await connection.BeginTransactionAsync(
                    System.Data.IsolationLevel.RepeatableRead,
                    cancellationToken);

            using var results = await connection.QueryMultipleAsync(
                new CommandDefinition(
                    QueryPersistence.sql,
                    new
                    {
                        AccountNumber = accountNumber,
                        StartDate = startDate,
                        EndDate = endDate,
                        PageSize = pageSize,
                        Offset = offset
                    },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            // 1. Verifica se a conta existe.
            var accountExists =
                await results.ReadSingleAsync<bool>();

            if (!accountExists)
                return null;

            // 2. Lê somente as transações da página.
            var transactions = (
                await results.ReadAsync<AccountTransaction>()
            ).AsList();

            // 3. Saldo acumulado.
            var balance =
                await results.ReadSingleAsync<decimal>();

            // 4. Total de transações no período.
            var totalItems =
                await results.ReadSingleAsync<long>();

            var totalPages =
                (totalItems + pageSize - 1) / pageSize;

            await transaction.CommitAsync(cancellationToken);

            var statement = new AccountStatement
            {
                AccountNumber = accountNumber,
                ReferenceDate = referenceDate,
                Balance = balance,
                Transactions = new PagedResult<AccountTransaction>(
                Items: transactions,
                Page: page,
                PageSize: pageSize,
                TotalItems: totalItems,
                TotalPages: totalPages
            ) };

            return new PagedResult<AccountStatement>(
                Items: new[] { statement },
                Page: page,
                PageSize: pageSize,
                TotalItems: totalItems,
                TotalPages: totalPages
            );
        }
    }
}
