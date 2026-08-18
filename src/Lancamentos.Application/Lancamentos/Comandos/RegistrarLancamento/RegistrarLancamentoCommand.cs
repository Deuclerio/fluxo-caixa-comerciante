using Lancamentos.Domain.Enums;
using MediatR;

namespace Lancamentos.Application.Lancamentos.Comandos.RegistrarLancamento;

public sealed record RegistrarLancamentoCommand(
    TipoLancamento Tipo,
    decimal Valor,
    DateOnly Data,
    string Descricao) : IRequest<RegistrarLancamentoResultado>;

public sealed record RegistrarLancamentoResultado(
    Guid Id,
    string Tipo,
    decimal Valor,
    DateOnly Data,
    string Descricao,
    DateTimeOffset CriadoEm);
