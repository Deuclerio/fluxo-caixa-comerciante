# 4. Justificativa de ferramentas, tecnologias e estilo

## Estilo: microsserviços por contexto

O enunciado pede explicitamente dois serviços e avalia a capacidade de **segregar responsabilidades**. Microsserviços aqui não são moda: cada contexto tem modelo, persistência e ciclo de deploy próprios.

Trade-off aceito: consistência eventual e mais peças para operar. Mitigação: contratos estáveis, idempotência, Docker Compose para o avaliador executar em minutos.

Alternativa descartada neste desafio: **monolito modular**. Seria mais simples para um único comerciante, mas esconderia a decisão de integração que o processo quer ver. A documentação registra que, em um produto real recém-nascido, o monolito modular seria um passo intermediário válido.

Detalhes: [ADR 001](adr/001-estilo-arquitetural.md).

## Linguagem: C# / .NET 10

- Linguagem escolhida para o desafio.
- Ecossistema maduro para APIs, mensageria e persistência.
- `DateOnly` nativo (data de caixa sem fuso embutido).
- `TimeProvider` para relógio testável.
- Rate limiting e health checks no framework.

## Clean Architecture + DDD tático

Regras de caixa (valor positivo, imutabilidade, saldo = créditos − débitos) ficam no **domínio**, cobertas por testes sem banco. Infraestrutura é plugável: PostgreSQL em execução, InMemory nos testes.

MediatR organiza casos de uso sem inflar controllers. FluentValidation barra entrada inválida antes da regra de domínio.

## PostgreSQL (database per service)

Relacional combina com lançamentos financeiros (precisão `numeric`, índices por data, integridade). Dois bancos no mesmo servidor Docker; em produção, instâncias isoladas.

SQLite seria mais simples de “rodar sem Docker”, mas não representa o alvo corporativo. InMemory cobre testes automatizados.

## RabbitMQ + MassTransit

Fila com retry, endpoint por consumidor e contrato tipado. MassTransit evita código AMQP de baixo nível e é o padrão de fato em .NET para este estilo.

Por que não HTTP síncrono Lançamentos → Consolidação? Porque o PDV não pode falhar se o relatório estiver fora. Por que não gRPC? O padrão de integração é **fato de negócio**, não consulta.

[ADR 002](adr/002-comunicacao-assincrona.md).

## Redis

O saldo do dia é leitura repetida (fechamento, conferência). Cache com invalidação no consumo do evento reduz carga no banco de consolidação. Em teste, `IMemoryCache` substitui o Redis.

## JWT

Demonstra autenticação/autorização sem montar um IdP. Mesma chave e audience nas duas APIs. Em produção: Keycloak / Entra ID, HTTPS obrigatório, rotação de chaves.

## Serilog, Swagger, Docker Compose

- Logs estruturados para correlacionar `LancamentoId`.
- Swagger para o avaliador exercitar a API.
- Compose para reproduzir a topologia (dois serviços + dependências) com um comando.

## O que foi evitado de propósito

| Tentação | Por que não |
| --- | --- |
| Kafka | Overhead operacional desproporcional a dois tipos de evento |
| MongoDB | Precisão monetária e relatório diário se encaixam melhor em relacional |
| CQRS completo com Event Store | O consolidado já é uma projeção; Event Store seria overengineering |
| Kubernetes no repositório | O avaliador precisa rodar localmente; K8s está na arquitetura alvo |
| MediatR + 20 projetos por serviço | Quatro camadas por contexto já evidenciam o desenho |
