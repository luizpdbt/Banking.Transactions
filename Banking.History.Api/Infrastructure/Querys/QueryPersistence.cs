namespace Banking.History.Api.Infraestructure.Querys
{
    public static class QueryPersistence
    {
        public const string sql = """
                        -- 1. Verifica se a conta existe
                        SELECT EXISTS (
                            SELECT 1
                            FROM accounts
                            WHERE account_number = @AccountNumber
                        );

                        -- 2. Histórico paginado
                        SELECT
                            transaction_id AS TransactionId,
                            account_number AS AccountNumber,
                            operation_type AS OperationType,
                            amount AS Amount,
                            created_at AS CreatedAt
                        FROM transactions
                        WHERE account_number = @AccountNumber
                          AND created_at >= @StartDate
                          AND created_at < @EndDate
                        ORDER BY created_at DESC, transaction_id DESC
                        LIMIT @PageSize
                        OFFSET @Offset;

                        -- 3. Saldo acumulado até a data de referência
                        SELECT COALESCE(
                            SUM(
                                CASE
                                    WHEN operation_type = 1 THEN -amount
                                    WHEN operation_type = 2 THEN amount
                                    ELSE 0
                                END
                            ),
                            0
                        )
                        FROM transactions
                        WHERE account_number = @AccountNumber
                          AND created_at < @EndDate;

                        -- 4. Total de movimentações no período
                        SELECT COUNT(*)
                        FROM transactions
                        WHERE account_number = @AccountNumber
                          AND created_at >= @StartDate
                          AND created_at < @EndDate;
                        """;
    }
}
