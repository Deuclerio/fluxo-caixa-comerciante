# 5. Segurança, resiliência e escalabilidade

## Segurança

Implementado:

- Autenticação JWT (Bearer) em todos os recursos de negócio.
- Autorização por papel `comerciante`.
- Validação de entrada (FluentValidation + invariantes de domínio).
- Rate limiting (100 requisições / minuto / janela fixa).
- Respostas `application/problem+json` sem vazar stack trace.
- Segredos de demo isolados em configuração (substituíveis por variável de ambiente).

Alvo de produção:

- TLS 1.2+ no Gateway; HSTS.
- Identity Provider, MFA, rotação de tokens de curta duração.
- Segredos no cofre (Key Vault).
- Criptografia em trânsito (TLS) e em repouso (storage encryption do Postgres).
- WAF / proteção contra flood no perímetro.
- Princípio de menor privilégio nas identities das APIs (cada uma acessa só o próprio banco).
- Auditoria imutável dos lançamentos (já são append-only).

A senha e a chave JWT deste repositório são **de demonstração**. Não devem ir para produção.

## Resiliência

| Estratégia | Onde |
| --- | --- |
| Isolamento de falha | Dois processos, dois bancos; relatório fora não impede o caixa |
| Retry exponencial | MassTransit no publisher e no consumidor (5 tentativas) |
| Idempotência | Tabela `lancamentos_processados` |
| Health check | `GET /health` para orquestrador retirar instância doente |
| Degradação do cache | Sem Redis, a leitura vai ao banco |
| Garantia de entrega (alvo) | Transactional Outbox — [ADR 002](adr/002-comunicacao-assincrona.md) |
| Failover (alvo) | Réplicas Postgres, cluster RabbitMQ, multi-AZ |
| DLQ (alvo) | Mensagens inválidas após retries para análise |

O consumidor trata reentrega: se o `LancamentoId` já foi aplicado, a mensagem é ignorada com log informativo. Isso é a recuperação de falha mais importante deste domínio (duplicar saldo seria pior do que atrasar o relatório).

## Escalabilidade

- APIs **sem estado de sessão**: novas réplicas no balanceador.
- Writes de lançamentos e reads de saldo **escalam em eixos diferentes**.
- Fila absorve pico de lançamentos (abertura/fechamento de caixa).
- Cache do saldo do dia reduz pressão de leitura.
- Prefetch limitado no consumidor evita saturar o banco de consolidação.
- Particionamento futuro por `data` ou por comerciante (quando houver multi-tenant), sem mudar o contrato do evento.

## Métricas sugeridas (alvo operacional)

| Métrica | Alerta |
| --- | --- |
| `http_request_duration_p95` registrar lançamento | > 300 ms por 5 min |
| `lag_consolidacao_segundos` | > 30 s |
| `rabbit_queue_depth` | > 1.000 |
| `taxa_erro_5xx` | > 1% |
| `health` down | 1 minuto |

## Monitoramento pró-ativo

No código: Serilog + health. No alvo: OpenTelemetry (traces ligando `POST /lancamentos` ao consumo), dashboards de profundidade de fila e de atraso de consolidação — o SLO que o comerciante percebe.
