using FluentValidation;
using Lancamentos.Application.Abstracoes;
using Lancamentos.Application.Comum;
using Lancamentos.Application.Lancamentos.Consultas.ObterLancamento;
using MediatR;

namespace Lancamentos.Application.Lancamentos.Consultas.ListarLancamentos;

public sealed class ListarLancamentosQueryValidator : AbstractValidator<ListarLancamentosQuery>
{
    public ListarLancamentosQueryValidator()
    {
        RuleFor(x => x.Pagina)
            .GreaterThanOrEqualTo(1)
            .WithMessage("A página deve ser maior ou igual a 1.");

        RuleFor(x => x.TamanhoPagina)
            .InclusiveBetween(1, 100)
            .WithMessage("O tamanho da página deve estar entre 1 e 100.");
    }
}

public sealed class ListarLancamentosQueryHandler
    : IRequestHandler<ListarLancamentosQuery, ResultadoPaginado<LancamentoDto>>
{
    private readonly ILancamentoRepositorio _repositorio;

    public ListarLancamentosQueryHandler(ILancamentoRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<ResultadoPaginado<LancamentoDto>> Handle(
        ListarLancamentosQuery request,
        CancellationToken cancellationToken)
    {
        var (itens, total) = await _repositorio.ListarPorDataAsync(
            request.Data,
            request.Pagina,
            request.TamanhoPagina,
            cancellationToken);

        var dtos = itens
            .Select(l => new LancamentoDto(
                l.Id,
                l.Tipo.ToString(),
                l.Valor,
                l.Data,
                l.Descricao,
                l.CriadoEm))
            .ToList();

        return ResultadoPaginado<LancamentoDto>.Criar(dtos, request.Pagina, request.TamanhoPagina, total);
    }
}
