# 2. Requisitos funcionais e não funcionais (refinados)

O enunciado original é deliberadamente curto. Abaixo está o refinamento usado para desenhar e implementar a solução.

## Requisitos funcionais

### RF-01 — Registrar lançamento

O comerciante autentica registra um crédito ou um débito contendo tipo, valor, data de caixa e descrição.

Regras:

- Valor estritamente maior que zero, com no máximo duas casas decimais.
- Tipo apenas `Credito` ou `Debito`.
- Descrição entre 3 e 200 caracteres.
- Data de caixa não pode ser superior a um dia no futuro (evita lançamentos acidentais em datas distantes).
- O lançamento é **imutável** após a criação (não há PUT/DELETE). Estorno futuro seria um novo lançamento de sentido contrário.

### RF-02 — Consultar lançamentos

O comerciante lista os movimentos de um dia e obtém um lançamento pelo identificador.

### RF-03 — Consolidar saldo diário

Cada lançamento registrado alimenta o saldo do respectivo dia:

`saldo = totalCréditos − totalDébitos`

O consolidado também expõe totais parciais e a quantidade de movimentos, para auditoria rápida.

### RF-04 — Consultar saldo diário e período

- Saldo de uma data específica (relatório principal do enunciado).
- Lista de saldos em intervalo de até 90 dias (evolução natural do relatório).
- Dia sem movimentos retorna consolidado zerado (não é erro).

### RF-05 — Integração entre serviços

A consolidação **não consulta** o banco de lançamentos. Consome o evento `LancamentoRegistrado`. Reentregas não duplicam o saldo (idempotência por `LancamentoId`).

## Requisitos não funcionais

| ID | Categoria | Meta (alvo) | Como a solução endereça |
| --- | --- | --- | --- |
| RNF-01 | Disponibilidade | 99,9% mensal por serviço | Serviços stateless, health checks, isolamento de falha |
| RNF-02 | Latência de escrita | p95 < 300 ms para registrar lançamento | Persistência local + publicação assíncrona |
| RNF-03 | Latência de leitura do saldo | p95 < 150 ms com cache quente | Redis (ou memória em teste) com TTL de 5 min |
| RNF-04 | Atraso de consolidação | p95 < 5 s após o lançamento | Fila + retry exponencial |
| RNF-05 | Integridade | Nenhum lançamento perdido; nenhum saldo duplicado | Persistência antes do evento + idempotência |
| RNF-06 | Segurança | Apenas comerciante autenticado | JWT, HTTPS no alvo, rate limit, validação de entrada |
| RNF-07 | Escalabilidade | Crescer writes e reads de forma independente | Escala horizontal de cada API; cache; fila |
| RNF-08 | Observabilidade | Detectar falha em < 1 min | Logs estruturados (Serilog), `/health` |
| RNF-09 | Manutenibilidade | Evoluir um contexto sem quebrar o outro | Contratos versionáveis em `Shared.Contracts` |

## Fora de escopo (explícito)

Itens conscientes, para não inflar o desafio além do enunciado:

- Multi-tenant / vários comerciantes (o modelo atual é de um comerciante).
- Conciliação bancária, categorias fiscais, NF-e.
- Estorno com workflow de aprovação.
- App mobile / UI rica (a entrega é API + arquitetura).
- SSO corporativo (o JWT local demonstra o padrão; IdP entra no alvo).

Esses itens cabem na arquitetura alvo sem redesenhar os bounded contexts.
