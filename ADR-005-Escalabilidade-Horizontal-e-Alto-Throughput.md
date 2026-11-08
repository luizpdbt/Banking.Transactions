# ADR-005 — Escalabilidade horizontal e processamento de alto throughput

- **Status:** Proposto para implantação AWS (não implementado no ambiente Docker local)
- **Contexto:** Banking.Transactions — Barramento API, History API e Identity API

## Contexto

As APIs ASP.NET Core 9 são executadas em containers e, em uma implantação AWS, poderão operar no Amazon EKS. Testes locais de concorrência validam comportamento transacional, mas não constituem garantia de capacidade de produção. O volume de requisições pode variar por serviço; o processamento de movimentações exige consistência do saldo no PostgreSQL.

## Decisão

Adotar **Horizontal Pod Autoscaler (HPA)** independente para cada Deployment, baseado nas métricas de utilização de CPU **ou** memória. O HPA considera a métrica que demanda o maior número de réplicas; não exige que ambas ultrapassem seus limites simultaneamente.

| Serviço | Réplicas mín. | Réplicas máx. | CPU request / limit | Memória request / limit | Alvo CPU | Alvo memória |
|---|---:|---:|---|---|---:|---:|
| Barramento API | 2 | 10 | 250m / 1000m | 256Mi / 512Mi | 65% | 75% |
| History API | 2 | 8 | 200m / 750m | 256Mi / 512Mi | 65% | 75% |
| Identity API | 2 | 6 | 100m / 500m | 128Mi / 256Mi | 60% | 70% |

Esses valores são **hipóteses iniciais de dimensionamento**, a validar por testes de carga e telemetria; não são capacidades comprovadas. As porcentagens são calculadas sobre `resources.requests`, não sobre `limits`: por exemplo, 65% de 250m equivale a aproximadamente 162,5 millicores de uso médio por pod.

O manifesto `k8s/hpa.yaml` define `autoscaling/v2`, políticas de estabilização e limites de crescimento/redução. O arquivo `k8s/deployments-resources.yaml` contém os valores de recursos a serem aplicados aos Deployments; os nomes dos Deployments e containers precisam corresponder aos objetos efetivamente implantados.

## Fluxo de escalabilidade

1. O balanceador de aplicação distribui tráfego HTTP(S) aos serviços publicados no EKS.
2. O Metrics Server fornece métricas de CPU e memória dos pods.
3. O HPA compara utilização média com os alvos definidos e ajusta o número desejado de réplicas do Deployment.
4. O scheduler posiciona novos pods em nós com capacidade; **Cluster Autoscaler ou Karpenter** é necessário caso seja preciso ampliar a quantidade de nós.
5. Readiness probes evitam encaminhar tráfego a pods ainda não preparados; graceful shutdown reduz interrupções durante o scale-down.

## Controle de gargalos e consistência

- **Banco de dados:** a soma dos pools de todos os pods não pode exceder a capacidade segura do RDS PostgreSQL. Exemplo meramente ilustrativo: 10 pods do Barramento com pool máximo de 20 conexões podem reservar até 200 conexões, sem contar History, Identity, administração e outras aplicações. Ajustar `Maximum Pool Size` por serviço e considerar **RDS Proxy** ou **PgBouncer**, após validar compatibilidade com transações e pooling.
- **Concorrência por conta:** atualizações na mesma linha de saldo continuam serializadas pelo lock do PostgreSQL. Mais pods não eliminam contenção em uma *hot account*.
- **Integridade:** manter atualização de saldo e registro da movimentação na mesma transação ACID, com idempotência por `transaction_id` e tratamento de conflitos/retries quando aplicável.
- **Saturação:** aplicar limites de requisições em voo, timeouts, backpressure e observabilidade; evitar abrir conexões ilimitadas para tentar elevar throughput.
- **Métricas:** acompanhar RPS, P50/P95/P99, taxa de erro, CPU, memória, utilização do pool, conexões ativas/ociosas, espera por locks, tempo de consulta e capacidade do RDS.
- **HPA não mede throughput diretamente:** CPU e memória são sinais indiretos; em workloads limitados por I/O ou lock, considerar futuramente métricas customizadas (RPS por pod, fila ou latência), com adaptador apropriado.

## Consequências e limitações

**Benefícios:** escala independente por API, menor desperdício em baixa carga, maior resiliência com múltiplas réplicas e política explícita de recursos.

**Custos e riscos:** consumo adicional de nós e conexões; crescimento de réplicas pode amplificar a pressão no banco. HPA não substitui testes de capacidade, dimensionamento do RDS, política de disponibilidade nem mecanismos de proteção contra sobrecarga.

## Validação

- Confirmar instalação e disponibilidade do Metrics Server.
- Aplicar recursos e HPA aos Deployments reais; conferir `kubectl describe hpa` e `kubectl top pods`.
- Executar testes com contas distintas e com uma mesma conta para medir contenção.
- Medir scale-up/scale-down, disponibilidade e impacto no pool do PostgreSQL.
- Ajustar metas e réplicas com base em resultados, não apenas em estimativas.
