# ADR-002 — Persistência de Dados e Consistência Transacional

**Status:** Proposto  
**Categoria:** Persistência de dados  
**Tecnologia escolhida:** PostgreSQL

## 1. Contexto

A solução será responsável pelo processamento de movimentações financeiras, envolvendo operações de crédito e débito nas contas dos clientes.

Devido à natureza financeira dessas operações, identificamos a necessidade de adotar um mecanismo de persistência que ofereça integridade, consistência transacional e controle de concorrência.

A ocorrência de atualizações inconsistentes, perda de movimentações ou processamento concorrente inadequado pode comprometer diretamente a confiabilidade dos saldos dos clientes.

## 2. Decisão arquitetural

Optamos pela utilização de um banco de dados relacional, PostgreSQL, como mecanismo principal de persistência.

A escolha considera o suporte a transações ACID, permitindo que operações financeiras relacionadas sejam executadas de maneira atômica e consistente.

Também utilizaremos os mecanismos de isolamento transacional e controle de concorrência disponibilizados pelo banco de dados para preservar a integridade das operações realizadas simultaneamente.

## 3. Justificativas

### Integridade e consistência dos dados

A natureza financeira da aplicação exige que as movimentações respeitem regras de integridade e que as alterações relacionadas sejam confirmadas ou revertidas conjuntamente.

O modelo transacional do PostgreSQL permite preservar essas propriedades durante o processamento.

### Controle de concorrência

Considerando a possibilidade de múltiplas requisições simultâneas sobre uma mesma conta, é necessário impedir condições de corrida e atualizações perdidas.

Os mecanismos transacionais do PostgreSQL permitem coordenar operações concorrentes, preservando a integridade do saldo.

A estratégia específica de controle de concorrência será definida conforme as características das operações implementadas.

### Priorização da consistência

Entendemos que, no contexto de movimentações financeiras, a integridade e a consistência dos dados possuem prioridade sobre a maximização da performance.

Essa decisão considera que inconsistências financeiras podem provocar impactos diretos aos clientes e à operação do banco.

## 4. Alternativas consideradas

### Bancos de dados não relacionais

Bancos não relacionais foram considerados como uma categoria alternativa de persistência.

Entretanto, optamos por um banco relacional devido à adequação do modelo transacional às necessidades identificadas.

Essa decisão não pressupõe que bancos não relacionais sejam incapazes de oferecer transações ou consistência forte, mas reconhece que o PostgreSQL atende aos requisitos com mecanismos consolidados e adequados ao escopo da solução.

## 5. Consequências e trade-offs

### Benefícios

- Suporte a transações ACID.
- Preservação da integridade financeira.
- Controle de concorrência entre operações simultâneas.
- Mecanismos de isolamento transacional.
- Possibilidade de utilização de restrições e índices para proteger e otimizar os dados.

### Custos e limitações

- Transações concorrentes podem provocar contenção de recursos.
- Mecanismos de bloqueio e isolamento podem aumentar a latência.
- O crescimento do volume transacional pode exigir otimizações na persistência.
- A priorização da consistência pode limitar o throughput em cenários de elevada concorrência sobre os mesmos registros.

Esses custos são considerados aceitáveis diante da necessidade de preservar a integridade financeira.

## 6. Observabilidade e evolução futura

Reconhecemos que o crescimento do volume de transações poderá provocar sobrecarga no banco de dados.

Por esse motivo, consideramos essencial acompanhar indicadores operacionais que permitam identificar antecipadamente possíveis gargalos.

Entre os indicadores relevantes estão:

- Latência das operações de leitura e escrita.
- Volume de transações por segundo.
- Tempo de espera por bloqueios.
- Ocorrência de deadlocks.
- Utilização de conexões e recursos do banco de dados.

Com base nas métricas observadas, poderão ser avaliadas estratégias de otimização de consultas, índices, ajustes de concorrência e escalabilidade da infraestrutura.

A implementação dessas evoluções será orientada por evidências de desempenho, evitando complexidade prematura.