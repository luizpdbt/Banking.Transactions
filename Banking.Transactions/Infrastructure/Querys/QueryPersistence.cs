namespace Banking.Barramento.Api.Infrastructure.Querys
{
    public static class QueryPersistence
    {
        public const string  updateBalanceSql = """
                UPDATE accounts
                SET balance = balance + @SignedAmount
                WHERE account_number = @AccountNumber;
                """;


        public const string insertTransactionSql = """
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
    }
}
