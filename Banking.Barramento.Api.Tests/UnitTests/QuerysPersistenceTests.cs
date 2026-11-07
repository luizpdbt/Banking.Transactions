using Banking.Barramento.Api.Infrastructure.Querys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking.Barramento.Api.Tests.UnitTests
{
    public class QuerysPersistenceTests
    {
        [Fact]
        public void UpdateBalanceSql_ShouldContainExpectedStructure()
        {
            // Arrange
            var query = QueryPersistence.updateBalanceSql;

            // Assert
            Assert.Contains("UPDATE accounts", query);
            Assert.Contains("SET balance = balance + @SignedAmount", query);
            Assert.Contains(
                "WHERE account_number = @AccountNumber",
                query);
        }

        [Fact]
        public void InsertTransactionSql_ShouldContainExpectedStructure()
        {
            // Arrange
            var query = QueryPersistence.insertTransactionSql;

            // Assert
            Assert.Contains("INSERT INTO transactions", query);

            Assert.Contains("transaction_id", query);
            Assert.Contains("account_number", query);
            Assert.Contains("operation_type", query);
            Assert.Contains("amount", query);

            Assert.Contains("@TransactionId", query);
            Assert.Contains("@AccountNumber", query);
            Assert.Contains("@OperationType", query);
            Assert.Contains("@Value", query);
        }

        [Fact]
        public void Queries_ShouldNotBeEmpty()
        {
            Assert.False(
                string.IsNullOrWhiteSpace(
                    QueryPersistence.updateBalanceSql));

            Assert.False(
                string.IsNullOrWhiteSpace(
                    QueryPersistence.insertTransactionSql));
        }

        [Fact]
        public void UpdateBalanceSql_ShouldUseAtomicIncrement()
        {
            // Arrange
            var query = QueryPersistence.updateBalanceSql;

            // Assert
            Assert.Contains(
                "balance = balance + @SignedAmount",
                query);
        }
    }
}
