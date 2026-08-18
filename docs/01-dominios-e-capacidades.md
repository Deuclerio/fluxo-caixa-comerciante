# 1. Domínios funcionais e capacidades de negócio

## Contexto de negócio

O comerciante precisa **enxergar o caixa do dia**: o que entrou (créditos), o que saiu (débitos) e quanto restou. São duas perguntas distintas, com ciclos de vida e padrões de acesso diferentes:

1. **Registrar um movimento** — operação de escrita, crítica, com regras contábeis.
2. **Consultar o consolidado do dia** — operação de leitura, analítica, tolerante a atraso de segundos.

Essa diferença justifica **dois bounded contexts**, alinhados ao enunciado (“um serviço de lançamentos” e “um serviço de saldo consolidado diário”).

## Mapa de contextos (DDD)

```mermaid
flowchart TB
  subgraph Lancamentos["Contexto: Lançamentos"]
    R[Registrar crédito/débito]
    C[Consultar lançamentos do dia]
    I[Publicar fato de negócio]
  end

  subgraph Consolidacao["Contexto: Consolidação"]
    A[Aplicar lançamento ao dia]
    S[Obter saldo diário]
    P[Consultar saldos do período]
  end

  Lancamentos -->|"LancamentoRegistrado<br/>(evento de integração)"| Consolidacao
```

### Linguagem ubíqua

| Termo | Significado |
| --- | --- |
| Lançamento | Movimento imutável de caixa (crédito ou débito) |
| Crédito | Entrada de recurso (aumenta o saldo) |
| Débito | Saída de recurso (diminui o saldo) |
| Data do caixa | Dia de negócio ao qual o lançamento pertence (`DateOnly`) |
| Saldo diário consolidado | `créditos − débitos` daquela data |
| Consistência eventual | O saldo reflete o lançamento após o consumo do evento |

## Capacidades de negócio

```mermaid
mindmap
  root((Fluxo de caixa))
    Controlar lançamentos
      Registrar crédito
      Registrar débito
      Consultar movimentos do dia
      Garantir imutabilidade
    Consolidar saldo
      Atualizar totais do dia
      Emitir saldo diário
      Consultar período
      Evitar duplicidade
    Proteger o acesso
      Autenticar comerciante
      Autorizar operações
    Operar com confiança
      Auditar movimentos
      Recuperar falhas de integração
```

| Capacidade | Contexto | Serviço | Endpoint principal |
| --- | --- | --- | --- |
| Registrar lançamento | Lançamentos | `Lancamentos.Api` | `POST /api/v1/lancamentos` |
| Consultar lançamentos | Lançamentos | `Lancamentos.Api` | `GET /api/v1/lancamentos?data=` |
| Consolidar saldo do dia | Consolidação | `Consolidacao.Api` | consumidor do evento |
| Emitir saldo diário | Consolidação | `Consolidacao.Api` | `GET /api/v1/saldos/{data}` |
| Consultar saldos do período | Consolidação | `Consolidacao.Api` | `GET /api/v1/saldos?inicio=&fim=` |

## Por que dois contextos (e não um módulo único)

| Critério | Lançamentos | Consolidação |
| --- | --- | --- |
| Modelo | Documento imutável do movimento | Agregado acumulador do dia |
| Padrão de carga | Writes pontuais | Reads frequentes + writes por evento |
| Consistência | Forte no próprio agregado | Eventual em relação à origem |
| Evolução | Novos tipos, categorias, anexos | Projeções, dashboards, fechamento |
| Falha isolada | Caixa continua registrando mesmo se o relatório estiver atrasado | Relatório pode degradar sem bloquear o PDV |

Um único banco compartilhado acoplaria os modelos e impediria escalar a leitura do relatório sem pressionar a escrita do caixa. A segregação é a decisão de arquitetura corporativa pedida no desafio: **responsabilidades distribuídas e capacidades isoladas**, com integração explícita.

## Cadeia de valor

```mermaid
sequenceDiagram
  actor Comerciante
  participant L as Serviço de Lançamentos
  participant B as Barramento (RabbitMQ)
  participant C as Serviço de Consolidação

  Comerciante->>L: Registra crédito/débito
  L->>L: Valida regras e persiste
  L-->>Comerciante: 201 Created
  L->>B: Publica LancamentoRegistrado
  B->>C: Entrega evento
  C->>C: Idempotência + aplica no saldo do dia
  Comerciante->>C: Consulta saldo diário
  C-->>Comerciante: Totais e saldo consolidado
```

O valor para o comerciante não é “persistir uma linha”: é **tomar decisão no dia** (comprar, sangrar caixa, fechar o expediente) com um consolidado confiável. A arquitetura separa a captura do fato da projeção gerencial.
