using Lancamentos.Application.Abstracoes;
using MediatR;

namespace Lancamentos.Application.Lancamentos.Consultas.ObterLancamento;

public sealed class ObterLancamentoQueryHandler : IRequestHandler<ObterLancamentoQuery, LancamentoDto?>
{
    private readonly ILancamentoRepositorio _repositorio;

    public ObterLancamentoQueryHandler(ILancamentoRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<LancamentoDto?> Handle(ObterLancamentoQuery request, CancellationToken cancellationToken)
    {
        var lancamento = await _repositorio.ObterPorIdAsync(request.Id, cancellationToken);
        if (lancamento is null)
        {
            return null;
        }

        return new LancamentoDto(
            lancamento.Id,
            lancamento.Tipo.ToString(),
            lancamento.Valor,
            lancamento.Data,
            lancamento.Descricao,
            lancamento.CriadoEm);
    }
}
