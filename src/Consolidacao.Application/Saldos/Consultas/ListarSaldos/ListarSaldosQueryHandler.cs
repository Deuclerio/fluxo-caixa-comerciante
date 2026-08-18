using Consolidacao.Application.Abstracoes;
using Consolidacao.Application.Comum;
using Consolidacao.Application.Saldos.Consultas.ObterSaldoDiario;
using FluentValidation;
using MediatR;

namespace Consolidacao.Application.Saldos.Consultas.ListarSaldos;

public sealed class ListarSaldosQueryValidator : AbstractValidator<ListarSaldosQuery>
{
    public ListarSaldosQueryValidator()
    {
        RuleFor(x => x.Fim)
            .GreaterThanOrEqualTo(x => x.Inicio)
            .WithMessage("A data final deve ser maior ou igual à data inicial.");

        RuleFor(x => x)
            .Must(x => x.Fim.DayNumber - x.Inicio.DayNumber <= 90)
            .WithMessage("O intervalo máximo da consulta é de 90 dias.");

        RuleFor(x => x.Pagina)
            .GreaterThanOrEqualTo(1)
            .WithMessage("A página deve ser maior ou igual a 1.");

        RuleFor(x => x.TamanhoPagina)
            .InclusiveBetween(1, 100)
            .WithMessage("O tamanho da página deve estar entre 1 e 100.");
    }
}

public sealed class ListarSaldosQueryHandler : IRequestHandler<ListarSaldosQuery, ResultadoPaginado<SaldoDiarioDto>>
{
    private readonly ISaldoDiarioRepositorio _repositorio;

    public ListarSaldosQueryHandler(ISaldoDiarioRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<ResultadoPaginado<SaldoDiarioDto>> Handle(
        ListarSaldosQuery request,
        CancellationToken cancellationToken)
    {
        var (itens, total) = await _repositorio.ListarPorPeriodoAsync(
            request.Inicio,
            request.Fim,
            request.Pagina,
            request.TamanhoPagina,
            cancellationToken);

        var dtos = itens
            .Select(s => new SaldoDiarioDto(
                s.Data,
                s.TotalCreditos,
                s.TotalDebitos,
                s.Saldo,
                s.QuantidadeLancamentos,
                s.AtualizadoEm))
            .ToList();

        return ResultadoPaginado<SaldoDiarioDto>.Criar(dtos, request.Pagina, request.TamanhoPagina, total);
    }
}
