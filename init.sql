-- =============================================
-- Banking Transactions - Database Initialization
-- =============================================

-- Contas bancárias
CREATE TABLE IF NOT EXISTS accounts (
    account_number VARCHAR(30) PRIMARY KEY,
    balance NUMERIC(18,2) NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Movimentações bancárias
CREATE TABLE IF NOT EXISTS transactions (
    transaction_id UUID PRIMARY KEY,
    account_number VARCHAR(30) NOT NULL
        REFERENCES accounts(account_number),
    operation_type SMALLINT NOT NULL
        CHECK (operation_type IN (1, 2)),
    amount NUMERIC(18,2) NOT NULL
        CHECK (amount > 0),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Índice para consultas de histórico
CREATE INDEX IF NOT EXISTS idx_transactions_history
ON transactions (
    account_number,
    created_at DESC,
    transaction_id DESC
);

-- Conta padrão de demonstração
INSERT INTO accounts (
    account_number,
    balance
)
VALUES (
    '12346',
    0.00
)
ON CONFLICT (account_number) DO NOTHING;