namespace Banking.History.Api.Infraestructure.Querys
{
    public static class QueryPersistence
    {
        public const string sql = """
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
    }
}
