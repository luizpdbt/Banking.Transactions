[README.md](https://github.com/user-attachments/files/34541204/README.md)
# Banking.Transactions

Sistema de movimentações financeiras em .NET, com serviços de autenticação JWT, movimentação de contas e consulta de histórico paginado. A persistência utiliza PostgreSQL e a execução local utiliza Docker Compose.

## 1. Pré-requisitos

- Docker Desktop instalado e em execução, com Docker Compose.
- Git para obter o repositório.
- Portas definidas no `docker-compose.yml` disponíveis.
- Opcional: .NET 9 SDK para executar os testes e Visual Studio 2022 para desenvolvimento.

## 2. Iniciar os serviços

Na pasta que contém `docker-compose.yml`, execute:

```bash
docker compose up -d --build
```

Verifique o estado dos serviços:

```bash
docker compose ps
```

Para examinar os logs:

```bash
docker compose logs --tail=100
```

**Importante:** a criação das tabelas é **manual**, conforme o passo 4. O ambiente precisa estar configurado antes de testar as APIs.

## 3. Conectar o pgAdmin ao PostgreSQL

Abra **http://localhost:5050** (se essa for a porta publicada no Compose). Entre com o e-mail e a senha definidos nas variáveis de ambiente do serviço `pgadmin` no `docker-compose.yml`.

No painel esquerdo, selecione **Servers → Register → Server**.

### 3.1. Aba General

No campo **Name**, digite `Banking Transactions`. Esse nome serve apenas para identificar a conexão no pgAdmin.


<img width="500" height="300" alt="image" src="https://github.com/user-attachments/assets/103f8bf5-e4f5-4d0a-8bf1-9b4d4be73eb6" />


### 3.2. Aba Connection

Preencha os campos com os valores da configuração local do PostgreSQL no Compose:

| Campo | Valor |
|---|---|
| Host name/address | `postgres` |
| Port | `5432` |
| Maintenance database | `banking` |
| Username | `banking` |
| Password | `BankingLocal2026` |

<img width="500" height="300" alt="image" src="https://github.com/user-attachments/assets/ff9583d3-c5f0-466a-b9d8-a8b498626710" />


> **Atenção:** o print ilustra os campos, mas apresenta valores de exemplo como `admin` e `postgres`. **Substitua-os pelos valores da tabela acima**, ou pelos valores efetivamente definidos no seu `docker-compose.yml`. O hostname `postgres` funciona porque o pgAdmin se comunica com o banco pela rede interna do Docker; não é o endereço a ser usado necessariamente por clientes executados fora do Docker.

Clique em **Save** para registrar a conexão.

## 4. Criar tabelas, índice e conta de demonstração

No pgAdmin, abra **Servers → Banking Transactions → Databases → banking**, clique com o botão direito no banco e selecione **Query Tool**.

Abra o arquivo `infra/init.sql` do repositório, copie **todo o conteúdo** para o editor SQL e clique no botão **Execute Script (▶)** ou pressione `F5`.

![Query Tool do pgAdmin com o script de inicialização](docs/images/pgadmin-query-tool.png)

O script deve criar:

- `accounts`: contas e saldos;
- `transactions`: movimentações financeiras;
- índice para consultas de histórico;
- conta padrão `12346`, com saldo inicial **R$ 0,00**.

> A conta começa com saldo zero para que o saldo calculado pelas movimentações não divirja de um valor inicial inserido diretamente no banco.

### 4.1. Validar a criação

No Query Tool, execute:

```sql
SELECT * FROM accounts;
SELECT * FROM transactions;
```

A primeira consulta deve incluir a conta `12346`, com saldo `0.00`. A segunda deve estar vazia antes da primeira movimentação. Se as tabelas não aparecerem na árvore do pgAdmin, use **Refresh** em `Schemas → public → Tables`.

## 5. Componentes

| Componente | Responsabilidade |
|---|---|
| `Banking.Identity.Api` | Autenticação e emissão de JWT |
| `Banking.Barramento.Api` | Operações de crédito e débito |
| `Banking.History.Api` | Consulta de extrato e histórico paginado |
| PostgreSQL | Persistência de contas e transações |
| pgAdmin | Administração do banco |

Consulte a seção `ports` do `docker-compose.yml` para as portas HTTP publicadas pelas APIs. As URLs usadas em desenvolvimento fora do Docker podem ser diferentes das portas publicadas no Compose.

## 6. Autenticação

A Identity API expõe `POST /api/Auth`. No ambiente de demonstração, as credenciais de teste utilizadas no projeto são:

```json
{
  "username": "tester",
  "password": "senha-local-de-teste"
}
```

Use o token retornado nas APIs protegidas:

```http
Authorization: Bearer <token>
```

Essas credenciais são exclusivamente para testes locais. Confira o contrato de autenticação no código ou Swagger da API.

## 7. Histórico paginado

Exemplo de consulta, usando a porta local de desenvolvimento `5129`:

```http
GET http://localhost:5129/api/HistoryAccount?Account=12346&referenceDate=2026-11-08&page=1&pageSize=10
```

| Parâmetro | Significado |
|---|---|
| `Account` | Número da conta |
| `referenceDate` | Dia consultado |
| `page` | Página, iniciando em 1 |
| `pageSize` | Registros por página (máximo 100) |

A consulta usa `LIMIT` e `OFFSET` no PostgreSQL, com ordenação por data e identificador da transação. O extrato utiliza leitura transacional `REPEATABLE READ` para manter os resultados da consulta no mesmo snapshot.

## 8. Decisões arquiteturais

Os registros de decisão estão em `docs/`:

- ADR-001 — Separação dos serviços de movimentação financeira e consulta de saldos.
- ADR-002 — Persistência de dados e consistência transacional.
- ADR-003 — Controle de concorrência e atomicidade das movimentações financeiras.
- ADR-004-Organizacao-Interna-dos-Servicos

As operações financeiras utilizam transações ACID e atualizações atômicas de saldo no PostgreSQL. Os testes de concorrência demonstram o comportamento nos cenários executados; não representam garantia de capacidade ilimitada.

## 9. Testes automatizados

Com o SDK .NET instalado:

```bash
dotnet test
```

Testes de integração e desempenho podem exigir containers e APIs em execução.

## 10. Encerrar o ambiente

Para parar os containers sem remover os volumes persistidos:

```bash
docker compose down
```

Para reiniciar:

```bash
docker compose up -d
```

**Cuidado:** `docker compose down -v` também remove volumes gerenciados pelo Compose, podendo apagar os dados do banco.

## Solução de problemas

**`Is a directory` ao executar `01-init.sql`:** remova ou corrija o bind mount de inicialização automática que aponta para um diretório em vez de um arquivo. Este README descreve o procedimento **manual** pelo Query Tool; não depende desse mount.

**pgAdmin não conecta:** confira se os containers estão em execução, se o serviço PostgreSQL se chama `postgres` e se banco, usuário e senha correspondem ao Compose.

**Tabelas não aparecem:** confirme que o script foi executado no banco `banking` e atualize a árvore do pgAdmin.
