# ADR 002 — Comunicação assíncrona entre contextos

- Status: Aceito
- Data: 2026-08-17

## Contexto

Após registrar um lançamento, o saldo diário precisa ser atualizado. Há duas famílias de integração: orquestração síncrona (HTTP) ou coreografia por eventos.

## Decisão

Publicar o fato `LancamentoRegistrado` em **RabbitMQ** (MassTransit). A consolidação é uma projeção. Entrega **at-least-once** + **idempotência** no consumidor.

## Alternativas

| Opção | Prós | Contras |
| --- | --- | --- |
| HTTP síncrono Lançamentos → Consolidação | Saldo imediato | Acoplamento de disponibilidade; timeout no PDV |
| Banco compartilhado / trigger | Simples | Viola database per service; acopla esquemas |
| Evento na fila | Desacopla, absorve pico, reprocessa | Atraso; risco dual-write |

## Dual-write e outbox (alvo)

A implementação atual persiste e depois publica. Se o processo cair entre os dois passos, o lançamento existe sem evento.

Arquitetura alvo: **Transactional Outbox** (MassTransit Entity Framework Outbox): o evento é gravado na mesma transação do lançamento e um dispatcher publica depois. Não foi ligado neste repositório para manter os testes InMemory simples e a subida local previsível — a lacuna está explícita, não omitida.

## Consequências

- Contrato em `Shared.Contracts` deve ser estável (adicionar campos opcionais; não remover).
- Consumidor obrigatoriamente idempotente.
- Retry com backoff no bus.
- DLQ e outbox entram no runbook de produção.
