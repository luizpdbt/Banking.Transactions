[README.md](https://github.com/user-attachments/files/34541204/README.md)
# Banking.Transactions

Sistema de movimentações financeiras em .NET, com serviços de autenticação JWT, movimentação de contas e consulta de histórico paginado. A persistência utiliza PostgreSQL e a execução local utiliza Docker Compose.


#Desenho Arquitetura

<img width="1754" height="897" alt="Arquitetura AWS de Microserviços Bancários" src="https://github.com/user-attachments/assets/925de1f5-9a51-46e1-80e5-2b7b9bb29a04" />






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

<img width="500" height="300" alt="image" src="https://github.com/user-attachments/assets/032448c5-1065-4647-a5f3-f3f7e4886202" />


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

<img width="500" height="300" alt="image" src="https://github.com/user-attachments/assets/b4724c78-081b-498b-84a3-22e59543ae7a" />


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

Essas credenciais são exclusivamente para testes locais.

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
- ADR-005 — Escalabilidade horizontal e processamento de alto throughput
- ADR-006-Seguranca-de-Dados-Sensiveis-e-Criptografia

As operações financeiras utilizam transações ACID e atualizações atômicas de saldo no PostgreSQL. Os testes de concorrência demonstram o comportamento nos cenários executados; não representam garantia de capacidade ilimitada.

## 9.Testes automatizados

Para executar os testes automatizados da solução:

```powershell
dotnet test Banking.Transactions.sln
```

**Importante:** os testes de performance dependem dos serviços da aplicação e do PostgreSQL. Portanto, é necessário que os containers Docker estejam em execução antes de iniciar os testes.

Para subir os containers:

```powershell
docker compose up -d --build
```

Após a inicialização dos serviços, execute os testes normalmente.

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

## 11. ## Observabilidade — OpenTelemetry, Prometheus, Tempo e Grafana

O projeto possui uma infraestrutura de observabilidade baseada em OpenTelemetry, Prometheus, Grafana Tempo e Grafana.

A infraestrutura é executada por meio de um Docker Compose independente, localizado no diretório `ObservabilidadeNova`, permitindo inicialização e manutenção separadas dos serviços bancários.

### Pré-requisitos

- Docker Desktop instalado e em execução.
- Docker Compose V2.
- Portas 3000, 9090, 3200, 4317, 4318 e 8889 disponíveis.

### 1. Inicializar a infraestrutura de observabilidade

Abra um terminal na raiz do repositório `Banking.Transactions` e execute:

```powershell
docker compose -f ObservabilidadeNova/docker-compose.yml up -d
```

O comando inicializará os seguintes serviços:

| Serviço | Porta | Responsabilidade |
|---|---|---|
| Grafana | 3000 | Visualização de métricas e traces |
| Prometheus | 9090 | Coleta e armazenamento de métricas |
| Grafana Tempo | 3200 | Armazenamento e consulta de traces |
| OpenTelemetry Collector | 4317 / 4318 | Recebimento de telemetria via OTLP |

Para verificar o estado dos containers:

```powershell
docker compose -f ObservabilidadeNova/docker-compose.yml ps
```

### 2. Inicializar as APIs bancárias

Após iniciar a observabilidade, execute os serviços bancários conforme as instruções de inicialização da aplicação apresentadas anteriormente neste README.

As APIs instrumentadas exportam telemetria para o OpenTelemetry Collector.

Para execução das APIs em containers Docker Desktop, o endpoint OTLP configurado é:

```text
http://host.docker.internal:4317
```

Para execução diretamente na máquina, utilizando Visual Studio, o endpoint é:

```text
http://localhost:4317
```

A configuração efetiva deverá corresponder ao ambiente de execução escolhido.

### 3. Acessar o Grafana

Com os containers iniciados, acesse:

**URL:** http://localhost:3000

**Usuário:** `admin`  
**Senha inicial:** `local-demo-change-me`

Essas credenciais são destinadas exclusivamente ao ambiente local de demonstração.

Os data sources Prometheus e Tempo são provisionados automaticamente durante a inicialização do Grafana.

### 4. Consultar traces das APIs

Para visualizar os traces distribuídos:

1. Acesse o Grafana.
2. No menu lateral, selecione **Explore**.
3. Selecione o data source **Tempo**.
4. Escolha o tipo de consulta **Search**.
5. No filtro **Service Name**, selecione a API desejada.
6. Execute uma requisição HTTP na API.
7. Clique em **Run query**.
8. Selecione um Trace ID para visualizar os detalhes da requisição.

Os serviços instrumentados podem ser identificados pelos respectivos nomes:

- `Banking.Barramento.Api`
- `Banking.History.Api`
- `Banking.Identity.Api`, quando sua instrumentação estiver habilitada.

A visualização permite analisar a duração das requisições, os códigos de resposta HTTP e os spans efetivamente instrumentados.

### 5. Consultar métricas

Para acessar o Prometheus:

**URL:** http://localhost:9090

No Grafana, selecione **Explore → Prometheus** para consultar as métricas coletadas pelo OpenTelemetry Collector.

As métricas disponíveis dependem da instrumentação habilitada em cada aplicação.

### 6. Verificar logs e solucionar problemas

Para consultar os logs do Collector:

```powershell
docker compose -f ObservabilidadeNova/docker-compose.yml logs --tail 100 otel-collector
```

Para consultar os logs do Tempo:

```powershell
docker compose -f ObservabilidadeNova/docker-compose.yml logs --tail 100 tempo
```

Para consultar os logs do Grafana:

```powershell
docker compose -f ObservabilidadeNova/docker-compose.yml logs --tail 100 grafana
```

Caso os traces não apareçam, verifique se a API está executando, se o endpoint OTLP está correto e se o Collector está recebendo conexões.

### 7. Encerrar a infraestrutura

Para interromper os serviços de observabilidade:

```powershell
docker compose -f ObservabilidadeNova/docker-compose.yml down
```

Esse comando remove os containers e a rede criada pelo Compose, preservando os volumes de dados.

### Considerações arquiteturais

A observabilidade foi implementada de maneira desacoplada das regras de negócio, utilizando OpenTelemetry para instrumentação e exportação de telemetria.

A escolha e seus trade-offs estão documentados no **ADR-007 — Estratégia de Observabilidade com Grafana e OpenTelemetry**.

A implementação inicial prioriza a coleta de métricas e traces HTTP. Dashboards operacionais específicos, alertas e instrumentação detalhada das operações internas são possibilidades de evolução da solução.
