# ADR 003 — Persistência, cache e precisão monetária

- Status: Aceito
- Data: 2026-08-17

## Contexto

Lançamentos exigem precisão decimal e consulta por data. O saldo é leitura frequente do mesmo dia. Cada contexto deve possuir seu modelo.

## Decisão

- PostgreSQL, um database por serviço.
- `numeric(18,2)` para valores monetários.
- `DateOnly` como chave do saldo diário.
- Redis (ou memória em teste) para cache do saldo, invalidado quando um lançamento é aplicado.

## Alternativas

| Opção | Motivo de descarte |
| --- | --- |
| Um único banco para os dois serviços | Acopla deploys e modelos |
| SQLite como persistência “oficial” | Não representa o alvo corporativo |
| Event Store como fonte da verdade | Excesso para o tamanho do domínio |
| Sem cache | Aceitável no volume atual; o cache demonstra a estratégia de escala de leitura pedida no desafio |

## Consequências

- Migrations formais substituiriam `EnsureCreated` em produção.
- Cache nunca é fonte da verdade: o banco de consolidação é.
- TTL de 5 minutos é rede de segurança caso a invalidação falhe.
