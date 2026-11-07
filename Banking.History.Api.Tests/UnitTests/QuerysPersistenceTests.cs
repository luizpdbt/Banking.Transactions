using Banking.History.Api.Infraestructure.Querys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public void Sql_ShouldFilterTransactionsByReferenceDate()
        {
            // Arrange
            var query = QueryPersistence.sql;

            // Assert
            Assert.Contains(
                "created_at <= @ReferenceDate",
                query);

            Assert.Contains(
                "account_number = @AccountNumber",
                query);
        }

        [Fact]
        public void Sql_ShouldOrderTransactionsChronologically()
        {
            // Arrange
            var query = QueryPersistence.sql;

            // Assert
            Assert.Contains(
                "ORDER BY created_at, transaction_id",
                query);
        }

        [Fact]
        public void Sql_ShouldCalculateCreditsAsPositiveAmounts()
        {
            // Arrange
            var query = QueryPersistence.sql;

            // Assert
            Assert.Contains(
                "WHEN operation_type = 1 THEN amount",
                query);
        }

        [Fact]
        public void Sql_ShouldCalculateDebitsAsNegativeAmounts()
        {
            // Arrange
            var query = QueryPersistence.sql;

            // Assert
            Assert.Contains(
                "WHEN operation_type = 2 THEN -amount",
                query);
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
        public void Sql_ShouldUseParameterizedQueries()
        {
            // Arrange
            var query = QueryPersistence.sql;

            // Assert
            Assert.Contains("@AccountNumber", query);
            Assert.Contains("@ReferenceDate", query);

            Assert.DoesNotContain(
                "WHERE account_number = '123456'",
                query);
        }

        [Fact]
        public void Sql_ShouldContainThreeSelectStatements()
        {
            // Arrange
            var query = QueryPersistence.sql;

            // Act
            var selectCount = System.Text.RegularExpressions.Regex
                .Matches(
                    query,
                    @"\bSELECT\b",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                .Count;

            // Assert
            Assert.Equal(3, selectCount);
        }
    }
}
