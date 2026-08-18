using MediatR;

namespace Consolidacao.Application.Saldos.Consultas.ObterSaldoDiario;

public sealed record ObterSaldoDiarioQuery(DateOnly Data) : IRequest<SaldoDiarioDto>;

public sealed record SaldoDiarioDto(
    DateOnly Data,
    decimal TotalCreditos,
    decimal TotalDebitos,
    decimal Saldo,
    int QuantidadeLancamentos,
    DateTimeOffset? AtualizadoEm);
