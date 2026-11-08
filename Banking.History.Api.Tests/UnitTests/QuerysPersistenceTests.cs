using Banking.History.Api.Infraestructure.Querys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Banking.History.Api.Tests.IntegrationTests
{
    public class QuerysPersistenceTests
    {
        [Fact]
        public void Sql_ShouldNotBeEmpty()
        {
            // Arrange
            var query = QueryPersistence.sql;

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(query));
        }

        [Fact]
        public void Sql_ShouldValidateAccountExistence()
        {
            // Arrange
            var query = QueryPersistence.sql;

            // Assert
            Assert.Contains("SELECT EXISTS", query);
            Assert.Contains("FROM accounts", query);
            Assert.Contains(
                "WHERE account_number = @AccountNumber",
                query);
        }

        [Fact]
        public void Sql_ShouldSelectTransactionHistory()
        {
            // Arrange
            var query = QueryPersistence.sql;

            // Assert
            Assert.Contains("FROM transactions", query);

            Assert.Contains(
                "transaction_id AS TransactionId",
                query);

            Assert.Contains(
                "account_number AS AccountNumber",
                query);

            Assert.Contains(
                "operation_type AS OperationType",
                query);

            Assert.Contains("amount AS Amount", query);
            Assert.Contains("created_at AS CreatedAt", query);
        }

    

        [Fact]
        public void Sql_ShouldReturnZeroWhenThereAreNoTransactions()
        {
            // Arrange
            var query = QueryPersistence.sql;

            // Assert
            Assert.Contains("COALESCE", query);
            Assert.Contains("SUM(", query);
        }

    

        [Fact]
        public void Sql_ShouldContainFourResultSets()
        {
            // Arrange
            var query = QueryPersistence.sql;

            // Act
            var selectCount = Regex.Matches(
                query,
                @"\bSELECT\b",
                RegexOptions.IgnoreCase
            ).Count;

            // Assert
            // 4 consultas, com um SELECT adicional
            // dentro do EXISTS.
            Assert.Equal(5, selectCount);
        }

        [Fact]
        public void Sql_ShouldUseLimitAndOffsetForPagination()
        {
            // Arrange
            var query = QueryPersistence.sql;

            // Assert
            Assert.Matches(
                @"(?i)LIMIT\s+@PageSize",
                query);

            Assert.Matches(
                @"(?i)OFFSET\s+@Offset",
                query);
        }

        [Fact]
        public void Sql_ShouldOrderTransactionsBeforePagination()
        {
            // Arrange
            var query = QueryPersistence.sql;

            // Assert
            Assert.Matches(
                @"(?is)ORDER\s+BY\s+created_at\s+DESC\s*,\s*" +
                @"transaction_id\s+DESC\s+LIMIT\s+@PageSize\s+" +
                @"OFFSET\s+@Offset",
                query);
        }

        [Fact]
        public void Sql_ShouldCountTotalTransactions()
        {
            // Arrange
            var query = QueryPersistence.sql;

            // Assert
            Assert.Matches(
                @"(?is)SELECT\s+COUNT\s*\(\s*\*\s*\)" +
                @"\s+FROM\s+transactions",
                query);
        }

        [Fact]
        public void Sql_ShouldFilterTransactionsByDateRange()
        {
            // Arrange
            var query = QueryPersistence.sql;

            // Assert
            Assert.Contains(
                "created_at >= @StartDate",
                query);

            Assert.Contains(
                "created_at < @EndDate",
                query);
        }

        [Fact]
        public void Sql_ShouldUseAccountNumberFilter()
        {
            // Arrange
            var query = QueryPersistence.sql;

            // Assert
            Assert.Contains(
                "account_number = @AccountNumber",
                query);
        }

        [Fact]
        public void Sql_ShouldUseDeterministicOrdering()
        {
            // Arrange
            var query = QueryPersistence.sql;

            // Assert
            Assert.Matches(
                @"(?i)ORDER\s+BY\s+created_at\s+DESC\s*,\s*" +
                @"transaction_id\s+DESC",
                query);
        }

        [Theory]
        [InlineData(1, 10, 0L)]
        [InlineData(2, 10, 10L)]
        [InlineData(3, 10, 20L)]
        [InlineData(5, 20, 80L)]
        [InlineData(10, 100, 900L)]
        public void Pagination_ShouldCalculateCorrectOffset(
            int page,
            int pageSize,
            long expectedOffset)
        {
            // Act
            var offset = ((long)page - 1) * pageSize;

            // Assert
            Assert.Equal(expectedOffset, offset);
        }

        [Theory]
        [InlineData(0L, 10, 0L)]
        [InlineData(1L, 10, 1L)]
        [InlineData(10L, 10, 1L)]
        [InlineData(11L, 10, 2L)]
        [InlineData(100L, 20, 5L)]
        [InlineData(101L, 20, 6L)]
        public void Pagination_ShouldCalculateTotalPages(
            long totalItems,
            int pageSize,
            long expectedPages)
        {
            // Act
            var totalPages =
                (totalItems + pageSize - 1) / pageSize;

            // Assert
            Assert.Equal(expectedPages, totalPages);
        }
    }
}
