# ADR 001 — Estilo arquitetural: microsserviços por bounded context

- Status: Aceito
- Data: 2026-08-17

## Contexto

O comerciante precisa registrar créditos/débitos e obter saldo diário consolidado. O processo avalia a capacidade de decompor domínio, escolher estilo (microsserviços, monolito, SOA, serverless) e justificar trade-offs.

## Decisão

Adotar **dois microsserviços** alinhados a bounded contexts (Lançamentos e Consolidação), internamente em Clean Architecture, integrados por eventos.

## Alternativas

| Opção | Prós | Contras | Por que não |
| --- | --- | --- | --- |
| Monolito modular | Operação simples, transação única | Esconde a integração; escala conjunta | O enunciado pede dois serviços e avalia decomposição |
| SOA + ESB | Contratos, orquestração | Complexidade e custo de ESB | Desproporcional ao domínio |
| Serverless (funções) | Escala a zero | Estado, idempotência e transação ficam mais difíceis de demonstrar localmente | Menos didático para o avaliador executar |
| Microsserviços por contexto | Isolamento, escala independente, linguagem ubíqua | Consistência eventual, mais infra | **Escolhido** — casa com o papel e com o enunciado |

## Consequências

- Relatório pode atrasar segundos (aceitável para saldo gerencial).
- É obrigatório tratar reentrega e contrato de evento.
- Deploy e falhas são independentes.
- Custo operacional maior que um monolito; mitigado neste desafio com Compose.
