# ADR-004 — Organização interna dos serviços e simplificação arquitetural

- **Status:** Aceito
- **Data:** 2026-11-08
- **Escopo:** Banking.Barramento.Api, Banking.History.Api e Banking.Identity.Api

## 1. Contexto

A solução Banking.Transactions é composta por três serviços independentes, responsáveis, respectivamente, pelo processamento de movimentações financeiras, pela consulta de histórico e pela autenticação. Cada serviço precisa oferecer uma organização interna compreensível, previsível e fácil de manter.

Na implementação deste desafio, não foi identificada necessidade concreta de reutilização de componentes entre os três serviços que justificasse a criação de múltiplas Class Libraries. Tampouco foram identificados requisitos que compensassem a complexidade adicional de uma arquitetura hexagonal completa, com portas, adaptadores e abstrações em todas as fronteiras.

A prioridade é demonstrar separação de responsabilidades, testabilidade e clareza do fluxo de execução, mantendo a solução proporcional ao seu escopo.

## 2. Decisão

Adotar uma **arquitetura em camadas, organizada por pastas dentro de cada projeto de API**, com um fluxo de execução explícito:

```text
Requisição HTTP
      |
      v
Controller  --> validação da entrada / contrato HTTP
      |
      v
Application (Service) --> orquestração do caso de uso
      |
      v
Domain --> regras e conceitos de negócio, quando aplicáveis
      |
      v
Infrastructure (Repository) --> persistência e integrações externas
      |
      v
PostgreSQL / Redis / outros recursos externos
```

O desenho representa o **fluxo lógico**, não a obrigação de que todo caso de uso invoque todas as camadas. Serviços de aplicação podem consultar repositórios por meio de interfaces, sem criar uma classe de domínio artificial apenas para cumprir o diagrama.

Cada um dos três serviços adota o mesmo padrão organizacional, respeitando suas responsabilidades específicas.

## 3. Responsabilidades das áreas

| Área | Responsabilidade |
|---|---|
| `Controllers` | Expor endpoints, receber parâmetros, devolver respostas HTTP e encaminhar solicitações à aplicação. |
| `Application` | Implementar e orquestrar casos de uso, coordenando regras e dependências. |
| `Domain` | Representar conceitos, entidades e regras de negócio independentes do transporte HTTP e da persistência. |
| `Infrastructure` | Implementar repositórios, consultas SQL, acesso a PostgreSQL, Redis e integrações externas. |
| `Dto` | Definir objetos de transferência de dados usados nos contratos aplicáveis. |
| `Validator` | Concentrar validações de entrada e regras de validação específicas, evitando duplicação nos controllers. |
| `Enums` | Agrupar enumerações pertinentes ao serviço; enumerações estritamente de domínio podem permanecer junto ao domínio. |
| `Middleware` | Implementar componentes transversais do pipeline HTTP. |
| `Extension` | Organizar extensões de configuração e registro de dependências. |

A estrutura é uma **convenção de organização**, não uma justificativa para espalhar regras de negócio em DTOs, controllers ou repositórios.

## 4. Convenções obrigatórias para desenvolvimento

1. **Respeitar o fluxo de responsabilidades.** Controllers não devem executar SQL, acessar Redis diretamente ou conter a lógica principal de negócio.
2. **Centralizar orquestração na Application.** O service coordena o caso de uso e utiliza contratos explícitos para dependências externas.
3. **Preservar a independência do Domain.** Regras de negócio não devem depender de controllers, Dapper, Npgsql ou detalhes de infraestrutura.
4. **Manter acesso a recursos externos na Infrastructure.** Repositórios, comandos SQL, PostgreSQL, Redis e clientes de serviços externos pertencem a essa área.
5. **Aplicar nomenclatura consistente.** Classes e interfaces devem expressar suas responsabilidades, como `HistoryAccountController`, `HistoryMovement`, `IHistoryRepository` e `HistoryRepository`.
6. **Não criar abstrações sem necessidade.** Interfaces devem existir quando definirem uma fronteira útil para desacoplamento, substituição ou testes, e não apenas para aumentar a quantidade de arquivos.
7. **Não duplicar lógica entre serviços indiscriminadamente.** Reuso futuro deverá ser avaliado a partir de necessidade real, considerando também o custo de acoplamento entre serviços.
8. **Manter as regras locais ao serviço responsável.** Os três projetos compartilham um padrão arquitetural, mas não devem depender do código interno uns dos outros.

## 5. Alternativas consideradas

### 5.1 Arquitetura hexagonal completa

**Não adotada neste momento.** A arquitetura hexagonal poderia reforçar a separação entre regras de negócio e adaptadores externos, porém exigiria uma estrutura e um conjunto de abstrações adicionais sem benefício proporcional demonstrado para o escopo atual.

Isso **não elimina** princípios importantes da arquitetura hexagonal: o uso de interfaces para isolar dependências externas e a proteção do domínio continuam sendo práticas válidas na solução.

### 5.2 Múltiplas Class Libraries por camada

**Não adotadas neste momento.** A separação física em assemblies diferentes não foi considerada necessária porque não existe demanda comprovada de distribuição ou reutilização dessas bibliotecas. A organização por pastas oferece legibilidade com menor custo de manutenção.

Essa escolha não significa que pastas imponham isolamento técnico: os limites são sustentados por convenções, revisões de código e testes. Caso a evolução do sistema exija fronteiras verificáveis por compilação, a extração de projetos poderá ser reavaliada.

### 5.3 Lógica concentrada nos controllers

**Rejeitada.** Embora reduza o número de classes inicialmente, essa abordagem aumenta o acoplamento ao HTTP, prejudica a testabilidade e dificulta a evolução dos casos de uso.

## 6. Consequências e trade-offs

**Benefícios esperados:** menor complexidade estrutural, curva de aprendizado reduzida, responsabilidades localizáveis, testes mais simples e consistência de organização entre os três serviços.

**Limitações assumidas:** a separação por pastas não impede dependências indevidas em tempo de compilação; a disciplina de desenvolvimento e a revisão técnica são necessárias para manter as fronteiras. A solução poderá precisar de novos módulos ou bibliotecas caso surjam requisitos de compartilhamento, crescimento significativo do domínio ou integrações mais complexas.

## 7. Critérios para revisão da decisão

Esta decisão deverá ser reavaliada quando ocorrer pelo menos uma das situações abaixo:

- Necessidade real e recorrente de compartilhar componentes entre serviços, com contratos estáveis.
- Crescimento do domínio que torne difícil preservar as dependências entre camadas apenas por convenção.
- Necessidade de trocar tecnologias de infraestrutura com frequência ou sustentar múltiplos adaptadores.
- Exigência de isolamento por assembly, publicação independente de componentes ou regras de arquitetura verificáveis automaticamente.

## 8. Relação com outros ADRs

- **ADR-001:** separação das responsabilidades entre os serviços.
- **ADR-002:** persistência e consistência transacional.
- **ADR-003:** concorrência e atomicidade das movimentações.

Este ADR trata especificamente da **organização interna de cada serviço** e das convenções que devem orientar sua evolução.
