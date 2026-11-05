# ADR-001 — Separação dos serviços de movimentação financeira e consulta de saldos

**Status:** Proposto  
**Categoria:** Arquitetura da aplicação

## 1. Contexto

O banco digital necessita de uma solução capaz de registrar movimentações financeiras de clientes e disponibilizar consultas de saldo consolidado em determinado momento.

Considerando os requisitos de desempenho, disponibilidade e confiabilidade, identificamos duas responsabilidades principais:

- Processamento de movimentações financeiras, incluindo créditos e débitos.
- Consulta da posição consolidada de saldo de um cliente em determinado momento.

Essas responsabilidades apresentam características distintas de processamento, escalabilidade e disponibilidade.

## 2. Decisão arquitetural

Optamos pela separação das responsabilidades em dois microsserviços independentes.

### Serviço de Movimentação Financeira

Responsável por receber e processar operações de crédito e débito nas contas dos clientes.

Sua principal responsabilidade será garantir que as atualizações de saldo sejam realizadas de maneira atômica, consistente e segura, preservando a integridade financeira.

### Serviço de Consulta de Saldo

Responsável por disponibilizar a posição consolidada do saldo de um cliente, considerando o momento solicitado.

Esse serviço será dedicado às operações de leitura, permitindo que sua capacidade seja dimensionada conforme a demanda de consultas.

### Persistência de dados

Inicialmente, ambos os serviços utilizarão a mesma instância do PostgreSQL.

O serviço de movimentação será responsável pelas operações de escrita, enquanto o serviço de consulta realizará operações de leitura.

Essa escolha reduz a complexidade de infraestrutura da implementação inicial.

## 3. Justificativas

**Escalabilidade independente**

A separação permite dimensionar cada serviço de acordo com suas características de utilização.

O serviço de movimentação poderá ser escalado conforme o volume de operações financeiras, enquanto o serviço de consulta poderá receber recursos adicionais conforme o crescimento das requisições de leitura.

**Isolamento de falhas**

A separação reduz o acoplamento operacional entre as responsabilidades.

Uma indisponibilidade no serviço de movimentação não precisa interromper o funcionamento do serviço de consulta, e vice-versa, desde que suas dependências compartilhadas permaneçam disponíveis.

**Separação de responsabilidades**

Cada serviço terá uma responsabilidade de negócio bem definida, favorecendo a manutenção, evolução e compreensão da solução.

**Simplicidade inicial**

A utilização de uma única instância de banco de dados evita a introdução antecipada de infraestrutura de replicação, mantendo o foco na implementação das funcionalidades essenciais.

## 4. Alternativas consideradas

**Aplicação única contendo movimentação e consulta**

Uma aplicação única reduziria a complexidade de implantação e operação.

Entretanto, optamos pela separação para permitir escalabilidade independente e reduzir o impacto de falhas específicas de cada serviço.

**Separação imediata das instâncias de leitura e escrita**

A utilização de uma réplica de leitura desde a primeira versão permitiria distribuir parte da carga de consultas.

Entretanto, essa alternativa adicionaria complexidade operacional e exigiria tratamento de atrasos de replicação. Por isso, sua implementação foi adiada.

## 5. Consequências e trade-offs

**Benefícios**
- Escalabilidade independente das operações de leitura e escrita.
- Maior isolamento entre falhas das aplicações.
- Responsabilidades de negócio claramente definidas.
- Infraestrutura inicial simplificada.
- Possibilidade de evolução independente dos serviços.

**Custos e limitações**
- Maior complexidade de implantação e monitoramento em comparação com uma aplicação única.
- Necessidade de administrar múltiplas aplicações.
- A instância compartilhada do PostgreSQL permanece como dependência comum.
- Consultas de alto volume podem competir por recursos com operações de escrita.
- A separação dos serviços não garante, isoladamente, alta disponibilidade.

## 6. Evoluções futuras

### Réplica de leitura do PostgreSQL

Como evolução arquitetural, consideramos disponibilizar uma instância dedicada às consultas, utilizando replicação do PostgreSQL.

O serviço de movimentação continuará utilizando a instância primária, enquanto o serviço de consulta poderá utilizar a réplica de leitura.

Essa evolução permitirá distribuir a carga de leitura e reduzir a competição por recursos com as operações financeiras.

**Trade-off:** a replicação assíncrona poderá introduzir atraso na visibilidade das movimentações. Portanto, consultas que exijam dados imediatamente atualizados deverão considerar esse comportamento e, quando necessário, utilizar a instância primária.

A adoção dessa estratégia dependerá de métricas de desempenho e da necessidade real de escalabilidade.