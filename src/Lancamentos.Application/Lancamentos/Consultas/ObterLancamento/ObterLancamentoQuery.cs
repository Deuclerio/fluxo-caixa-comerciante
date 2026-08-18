using MediatR;

namespace Lancamentos.Application.Lancamentos.Consultas.ObterLancamento;

public sealed record ObterLancamentoQuery(Guid Id) : IRequest<LancamentoDto?>;

public sealed record LancamentoDto(
    Guid Id,
    string Tipo,
    decimal Valor,
    DateOnly Data,
    string Descricao,
    DateTimeOffset CriadoEm);
