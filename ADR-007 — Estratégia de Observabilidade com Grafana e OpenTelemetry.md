# ADR-007 — Estratégia de Observabilidade com Grafana e OpenTelemetry

**Status:** Aceito\
**Data:** 08/11/2026\
**Contexto:** Banking.Transactions\
**Categoria:** Observabilidade e Operação

## 1. Contexto

A solução Banking.Transactions é composta por serviços independentes responsáveis por movimentações financeiras, consulta de histórico e autenticação.

Considerando a natureza transacional da aplicação, identificou-se a necessidade de implementar mecanismos de observabilidade que permitam acompanhar o comportamento dos serviços, investigar falhas, analisar tempos de resposta e identificar possíveis gargalos de desempenho.

A observabilidade também é relevante para investigar situações de concorrência transacional, especialmente quando múltiplas movimentações são realizadas simultaneamente sobre uma mesma conta bancária.

A ausência de uma solução centralizada de consulta dificultaria a correlação das informações operacionais e a identificação da origem de problemas.

## 2. Decisão arquitetural

Foi definida a utilização do **Grafana como interface central de observabilidade**, integrado ao OpenTelemetry, Prometheus e Grafana Tempo.

A solução será composta pelos seguintes componentes:

| Componente              | Responsabilidade                                               |
| ----------------------- | -------------------------------------------------------------- |
| OpenTelemetry SDK       | Instrumentação das aplicações .NET                             |
| OpenTelemetry Collector | Recebimento e encaminhamento da telemetria                     |
| Prometheus              | Coleta e armazenamento de métricas                             |
| Grafana Tempo           | Armazenamento e consulta de traces distribuídos                |
| Grafana                 | Dashboards, exploração de telemetria e configuração de alertas |

A arquitetura permite centralizar a consulta operacional em uma única interface, preservando a separação de responsabilidades entre os componentes responsáveis pela coleta, armazenamento e visualização dos dados.

### 2.1. Fluxo de telemetria

As APIs são instrumentadas com bibliotecas OpenTelemetry e exportam telemetria utilizando o protocolo OTLP.

Os dados são encaminhados ao OpenTelemetry Collector, responsável por direcioná-los aos respectivos componentes.

- Os traces são enviados ao Grafana Tempo.
- As métricas são expostas pelo Collector e coletadas pelo Prometheus.
- O Grafana consulta o Prometheus e o Tempo para visualização e análise.

### 2.2. Instrumentação das aplicações

Foi definida a inclusão das bibliotecas necessárias à observabilidade nas aplicações .NET, contemplando inicialmente:

- Instrumentação de requisições ASP.NET Core.
- Instrumentação de chamadas HTTP externas.
- Coleta de métricas do runtime .NET.
- Exportação de métricas e traces via OTLP.
- Identificação individual dos serviços por meio de `service.name`.

A configuração foi organizada por meio de métodos de extensão, reduzindo a responsabilidade do `Program.cs` e mantendo a observabilidade separada das regras de negócio.

A arquitetura também permite a inclusão futura de spans personalizados para operações de persistência e processamento transacional.

## 3. Justificativa

A escolha foi fundamentada nos seguintes critérios:

**Simplicidade operacional:** a solução pode ser executada em containers Docker, sem exigir a adoção inicial de uma plataforma corporativa de observabilidade.

**Separação de responsabilidades:** cada componente possui uma finalidade específica e pode evoluir independentemente.

**Centralização da experiência operacional:** dashboards, métricas e traces podem ser consultados a partir do Grafana.

**Padronização:** a utilização do OpenTelemetry reduz o acoplamento da instrumentação das aplicações a um fornecedor específico.

**Extensibilidade:** a solução permite evoluir para alertas, dashboards personalizados e rastreamento mais detalhado das operações.

**Adequação ao escopo:** a arquitetura oferece os recursos necessários para o desafio técnico sem introduzir complexidade operacional desproporcional.

## 4. Alternativas consideradas

### 4.1. Datadog

Plataforma corporativa de observabilidade com recursos integrados de APM, métricas, logs, alertas e análise de desempenho.

**Vantagens:**

- Plataforma integrada.
- Recursos avançados de monitoramento e diagnóstico.
- Menor necessidade de administrar determinados componentes de infraestrutura.

**Desvantagens:**

- Custos de licenciamento e ingestão de dados.
- Maior dependência de um fornecedor.
- Complexidade e recursos superiores às necessidades imediatas do desafio.

### 4.2. Dynatrace

Plataforma corporativa de observabilidade com recursos avançados de APM, descoberta de dependências e análise automatizada.

**Vantagens:**

- Monitoramento aprofundado das aplicações.
- Recursos avançados de diagnóstico.
- Capacidades de análise e correlação automatizada.

**Desvantagens:**

- Custos associados à plataforma.
- Maior complexidade de adoção e configuração para o escopo proposto.
- Funcionalidades que excedem as necessidades iniciais da solução.

### 4.3. Grafana com OpenTelemetry

**Vantagens:**

- Arquitetura modular.
- Instrumentação baseada em padrões abertos.
- Possibilidade de execução local em Docker.
- Centralização da visualização.
- Flexibilidade para evolução dos componentes.

**Desvantagens:**

- Necessidade de configurar e administrar múltiplos serviços.
- Maior responsabilidade pela operação da infraestrutura.
- Necessidade de implementar políticas de retenção, disponibilidade e segurança em ambientes produtivos.

**Decisão:** adotar Grafana, Prometheus, Tempo e OpenTelemetry por oferecerem um equilíbrio adequado entre capacidade de observabilidade, flexibilidade e simplicidade para o escopo atual.

## 5. Consequências positivas

A decisão proporciona:

- Visibilidade sobre requisições HTTP e seus tempos de execução.
- Capacidade de consultar traces das movimentações bancárias.
- Base para construção de dashboards de throughput, latência e taxas de erro.
- Possibilidade de configurar alertas a partir das métricas coletadas.
- Instrumentação desacoplada das regras de negócio.
- Capacidade de investigar gargalos e comportamentos anormais.
- Evolução incremental da observabilidade sem necessidade de substituir a arquitetura da aplicação.

## 6. Consequências negativas e riscos

### 6.1. Pontos únicos de falha

Embora os componentes possuam responsabilidades distintas, a implantação inicial utiliza instâncias individuais dos serviços de observabilidade.

A indisponibilidade do Grafana compromete a interface central de consulta e operação.

A indisponibilidade do OpenTelemetry Collector pode interromper o recebimento de novos dados de telemetria.

Falhas no Prometheus ou no Tempo podem comprometer a coleta, o armazenamento ou a consulta dos respectivos dados.

Portanto, a arquitetura inicial **não oferece alta disponibilidade da infraestrutura de observabilidade**.

### 6.2. Dependência operacional

A solução exige manutenção dos componentes, atualização de versões, monitoramento do armazenamento e configuração de políticas de retenção.

### 6.3. Consumo de recursos

A instrumentação e a exportação de telemetria adicionam consumo de CPU, memória, rede e armazenamento.

A coleta deverá ser ajustada para evitar overhead desnecessário nas operações transacionais.

### 6.4. Segurança da telemetria

Os traces e as métricas não devem expor informações bancárias confidenciais, tokens de autenticação, credenciais ou dados pessoais desnecessários.

A instrumentação deverá respeitar princípios de minimização de dados e proteção das informações.

## 7. Estratégias de mitigação

Para uma evolução em ambiente produtivo, são consideradas as seguintes medidas:

- Implementação de alta disponibilidade para componentes críticos.
- Configuração de políticas de retenção e limites de armazenamento.
- Definição de alertas para disponibilidade, erros e degradação de desempenho.
- Configuração de limites de recursos dos containers.
- Utilização de processamento e exportação assíncrona de telemetria.
- Aplicação de políticas de amostragem de traces quando necessário.
- Restrição de acesso às interfaces administrativas e endpoints de telemetria.
- Monitoramento da própria infraestrutura de observabilidade.

A indisponibilidade da infraestrutura de observabilidade não deverá impedir o processamento das movimentações bancárias.

## 8. Estado da implementação

A implementação inicial contempla:

- Infraestrutura de observabilidade executada em Docker Compose independente.
- OpenTelemetry Collector configurado para receber telemetria OTLP.
- Prometheus configurado para coletar métricas.
- Grafana Tempo configurado para receber traces.
- Grafana configurado com os respectivos data sources.
- Instrumentação inicial do Banking.Barramento.Api.
- Validação da visualização de traces de requisições HTTP no Grafana Tempo.

A implementação de dashboards operacionais específicos, alertas e instrumentação detalhada de operações internas poderá ser realizada incrementalmente.

## 9. Conclusão

A adoção de Grafana com OpenTelemetry, Prometheus e Tempo atende às necessidades iniciais de observabilidade do Banking.Transactions, oferecendo visibilidade operacional e capacidade de diagnóstico sem exigir a adoção de uma plataforma corporativa mais complexa.

A decisão prioriza simplicidade, modularidade e padronização, reconhecendo os riscos relacionados à disponibilidade e à operação dos componentes.

A arquitetura poderá evoluir para cenários de maior escala e disponibilidade conforme as necessidades do sistema.
