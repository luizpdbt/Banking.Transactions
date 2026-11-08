# ADR-006 — Proteção de dados sensíveis com PostgreSQL e AWS Secrets Manager

- **Status:** Proposto — requer implementação e validação; não faz parte do esquema atual
- **Escopo:** CPF de titular de conta, caso o modelo seja estendido para armazená-lo

## Contexto

O esquema atual armazena `account_number`, `balance` e movimentações, mas **não possui CPF ou nome**. CPF é dado pessoal sob a LGPD; dependendo do contexto, pode exigir controles reforçados, embora **não integre automaticamente a categoria legal de dado pessoal sensível** do art. 5º, II, da LGPD. O número da conta também é dado pessoal quando vinculado a pessoa identificada ou identificável e deve receber controles apropriados.

Para demonstrar proteção de dados pessoais sem adicionar um serviço criptográfico próprio, considera-se criptografia de CPF no PostgreSQL via extensão `pgcrypto` com criptografia simétrica autenticada, sob controle da aplicação.

## Decisão

1. Adicionar à tabela `accounts` a coluna `cpf_encrypted BYTEA NULL`, **sem coluna de CPF em texto puro**.
2. Armazenar a senha/chave de criptografia em **AWS Secrets Manager**, com criptografia em repouso via AWS KMS, políticas IAM de menor privilégio, auditoria e rotação planejada.
3. A aplicação autorizada obtém o segredo do Secrets Manager por identidade de workload do EKS (por exemplo, EKS Pod Identity/IRSA). **Não** colocar o segredo no repositório, imagem Docker, URL, arquivo de log ou `appsettings.json` versionado.
4. Para gravar, a API autorizada fornece o CPF e o segredo como **parâmetros** de uma instrução SQL que usa `pgp_sym_encrypt(...)` com `cipher-algo=aes256`.
5. Para consultar, a **History API**, após autenticação e autorização específicas, obtém o segredo (preferencialmente via cache em memória com TTL e política de atualização) e o envia como parâmetro SQL para `pgp_sym_decrypt(...)`.
6. A resposta deve expor CPF completo **somente quando a finalidade e a autorização exigirem**; por padrão, mascarar ou omitir o dado.

> A chave é disponibilizada ao processo da aplicação e enviada ao servidor PostgreSQL durante a operação. Portanto, a solução não representa criptografia de ponta a ponta nem impede acesso ao dado por administradores com privilégios suficientes sobre o servidor e suas consultas. A conexão entre aplicação e PostgreSQL deve utilizar TLS com validação do certificado.

## Exemplo de migração SQL (proposta)

```sql
CREATE EXTENSION IF NOT EXISTS pgcrypto;

ALTER TABLE accounts
    ADD COLUMN IF NOT EXISTS cpf_encrypted BYTEA;
```

### Gravação parametrizada (exemplo conceitual)

```sql
UPDATE accounts
SET cpf_encrypted = pgp_sym_encrypt(
    @Cpf,
    @EncryptionSecret,
    'cipher-algo=aes256'
)
WHERE account_number = @AccountNumber;
```

### Leitura parametrizada (exemplo conceitual)

```sql
SELECT account_number,
       pgp_sym_decrypt(cpf_encrypted, @EncryptionSecret) AS cpf
FROM accounts
WHERE account_number = @AccountNumber;
```

Os parâmetros devem ser enviados por Npgsql/Dapper, nunca concatenados na SQL. A função `pgp_sym_encrypt` produz um pacote OpenPGP com proteção de integridade; a segurança depende da qualidade e proteção da senha de criptografia. O segredo **não deve ser enviado como literal em SQL, interpolado ou registrado em tracing**. Mesmo parametrizado, pode haver exposição em memória ou observabilidade mal configurada; revisar logs de SQL, APM e permissões de banco.

## Autorização, privacidade e busca

- Usar JWT com políticas/claims específicas para acesso a CPF; autenticação isolada não é autorização suficiente.
- Evitar retornar CPF no endpoint de histórico por padrão; preferir um fluxo específico e auditável para dados cadastrais.
- Auditar o acesso sem registrar CPF completo ou segredo.
- **Não** consultar CPF com `WHERE pgp_sym_decrypt(...) = ...` em grandes volumes: a descriptografia por linha dificulta indexação. Caso haja busca por CPF, avaliar índice auxiliar por HMAC determinístico com **chave separada**, controles contra enumeração e requisitos de unicidade.
- Estabelecer base legal, finalidade, retenção, exclusão e acesso mínimo conforme a LGPD.

## Rotação e falhas

- Rotação do segredo requer plano de **recriptografia** ou suporte temporário a versões de chave; trocar o segredo sem migrar os registros impede a leitura dos dados anteriores.
- Falha no Secrets Manager ou no acesso à chave deve produzir erro controlado e **nunca** fallback para CPF em texto puro.
- Cache de segredo deve ser limitado, protegido em memória tanto quanto possível e invalidado na rotação; evitar buscar o segredo remotamente em **toda** requisição de alto throughput.
- Definir política de backup, restore e recuperação de chave. Perda da chave pode tornar o CPF irrecuperável.

## Consequências e alternativas

**Benefícios:** demonstra criptografia de dados em repouso na coluna, segregação do segredo do banco e do código, integração com IAM/Secrets Manager e acesso controlado.

**Riscos:** o PostgreSQL recebe o segredo e processa o CPF em texto puro durante a operação; há custos de criptografia, gestão de rotação e potencial exposição via logs ou privilégios excessivos. Para requisitos mais fortes de isolamento de chave, considerar criptografia na aplicação com AWS KMS/envelope encryption, mantendo o banco sem acesso à chave de descriptografia.

## Critérios de validação

1. Verificar que `cpf_encrypted` é `BYTEA` e não armazena CPF legível.
2. Confirmar que somente workloads autorizados leem o segredo do Secrets Manager.
3. Validar gravação/leitura, mascaramento, negação de acesso, falha de chave e rotação.
4. Confirmar TLS na conexão com RDS, logs sem dados pessoais e auditoria de acesso.
5. Documentar explicitamente que esta extensão é **proposta** até que código, migração, infraestrutura e testes estejam implementados.
