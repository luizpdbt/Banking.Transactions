# ADR-003 — Controle de Concorrência e Atomicidade das Movimentações Financeiras

**Status:** Aceito  
**Categoria:** Consistência e concorrência  
**Tecnologia:** PostgreSQL  
**Dependência:** ADR-002 — Persistência de Dados e Consistência Transacional

## 1. Contexto

A aplicação será responsável pelo processamento de movimentações financeiras de crédito e débito, podendo receber múltiplas requisições simultâneas sobre uma mesma conta.

Esse cenário exige mecanismos capazes de garantir a integridade dos saldos e impedir condições de corrida, atualizações perdidas e inconsistências entre o saldo consolidado e o histórico de movimentações.

Considerando a natureza financeira da aplicação, a consistência dos dados é prioritária em relação à maximização do desempenho.

## 2. Decisão arquitetural

Optamos por utilizar transações ACID do PostgreSQL, associadas a atualizações aritméticas atômicas e mecanismos de bloqueio em nível de linha.

Cada movimentação financeira será executada em uma única transação de banco de dados, contemplando:

1. Validação da operação e identificação da conta.
2. Atualização atômica do saldo.
3. Registro da movimentação financeira.
4. Confirmação da transação por meio de `COMMIT`.

Caso alguma etapa falhe, a transação será revertida, impedindo atualizações parciais.

Para evitar o processamento duplicado de movimentações, será utilizado um identificador único por transação financeira, protegido por uma restrição de unicidade no banco de dados.

## 3. Justificativas

### 3.1. Atualização atômica

A atualização do saldo será realizada diretamente no banco de dados, utilizando operações aritméticas sobre o valor persistido.

Essa abordagem evita o padrão de leitura, cálculo e sobrescrita do saldo na aplicação, que poderia provocar atualizações perdidas em cenários concorrentes.

### 3.2. Bloqueio em nível de linha

O PostgreSQL oferece mecanismos de bloqueio de registros durante operações de atualização.

Optamos por utilizar a granularidade de linha, permitindo que operações sobre a mesma conta sejam coordenadas sem bloquear desnecessariamente movimentações de outras contas.

### 3.3. Consistência entre saldo e histórico

A atualização do saldo e o registro da movimentação serão realizados na mesma transação.

Dessa maneira, uma movimentação não poderá ser confirmada sem que seu efeito sobre o saldo também seja persistido.

### 3.4. Idempotência

Cada movimentação possuirá um identificador único, utilizado para impedir que requisições repetidas provoquem múltiplas alterações no saldo.

A verificação será protegida por uma restrição de unicidade no banco de dados, garantindo segurança mesmo diante de requisições concorrentes.

## 4. Estratégia de implementação

A implementação utilizará uma transação explícita no PostgreSQL.

Exemplo conceitual:

```sql
BEGIN;

UPDATE accounts
SET balance = balance + @amount
WHERE account_id = @accountId;

INSERT INTO transactions (
    transaction_id,
    account_id,
    amount,
    requested_at
)
VALUES (
    @transactionId,
    @accountId,
    @amount,
    @requestedAt
);

COMMIT;
```

O valor de `@amount` será positivo para créditos e negativo para débitos.

A implementação deverá validar se a conta existe, tratar violações de unicidade e garantir rollback em caso de falha.

A restrição única em `transaction_id` impedirá que uma movimentação duplicada seja confirmada, revertendo também a atualização do saldo executada na mesma transação.

O exemplo pressupõe que a conta já exista e que as validações necessárias tenham sido realizadas.

## 5. Alternativas consideradas

### Bloqueio exclusivo de tabelas

Inicialmente, consideramos o bloqueio das tabelas envolvidas durante o processamento das movimentações.

Entretanto, essa abordagem poderia provocar contenção desnecessária entre operações de contas independentes, prejudicando a escalabilidade.

Por esse motivo, optamos por utilizar os mecanismos de bloqueio em nível de linha disponibilizados pelo PostgreSQL.

### Bloqueio pessimista explícito com SELECT FOR UPDATE

Também consideramos a utilização de `SELECT FOR UPDATE` para bloquear previamente o registro da conta.

Entretanto, como a regra atual permite saldo negativo e não exige leitura prévia para autorizar o débito, entendemos que uma atualização aritmética atômica atende à necessidade inicial com menor complexidade.

O bloqueio explícito poderá ser reavaliado caso novas regras de negócio exijam validações dependentes do saldo atual.

### Controle otimista de concorrência

O controle por versionamento também foi considerado como alternativa.

Entretanto, para o escopo inicial, optamos pelos mecanismos transacionais e de bloqueio do PostgreSQL, evitando a necessidade de implementar uma estratégia adicional de detecção de conflitos e novas tentativas.

## 6. Consequências e trade-offs

### Benefícios

- Prevenção de atualizações perdidas.
- Consistência entre saldo e movimentações financeiras.
- Processamento concorrente de contas independentes.
- Proteção contra movimentações duplicadas.
- Utilização de mecanismos nativos do PostgreSQL.
- Menor complexidade de implementação.

### Custos e limitações

- Operações simultâneas sobre a mesma conta podem enfrentar contenção.
- Bloqueios podem aumentar a latência em períodos de alta demanda.
- Transações longas podem reduzir o throughput.
- Deadlocks e falhas transitórias exigem tratamento adequado.
- A idempotência depende de identificadores estáveis para cada operação lógica.

Esses custos são considerados aceitáveis diante da necessidade de preservar a integridade financeira.

## 7. Observabilidade e evolução futura

Serão considerados indicadores de desempenho e concorrência, incluindo:

- Latência das transações financeiras.
- Tempo de espera por bloqueios.
- Ocorrência de deadlocks.
- Taxa de transações concluídas e revertidas.
- Volume de operações concorrentes.

Caso sejam identificados gargalos, poderão ser avaliadas estratégias de otimização das transações, revisão de índices e ajustes de infraestrutura.

A adoção de mecanismos adicionais de concorrência será orientada por evidências de desempenho e necessidades de negócio, evitando complexidade prematura.