using MediatR;

namespace Consolidacao.Application.Saldos.Comandos.AplicarLancamento;

public sealed record AplicarLancamentoCommand(
    Guid LancamentoId,
    string Tipo,
    decimal Valor,
    DateOnly Data) : IRequest<bool>;
