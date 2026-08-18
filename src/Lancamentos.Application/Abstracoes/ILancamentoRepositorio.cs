using Lancamentos.Domain.Entidades;

namespace Lancamentos.Application.Abstracoes;

public interface ILancamentoRepositorio
{
    Task AdicionarAsync(Lancamento lancamento, CancellationToken cancellationToken);
    Task<Lancamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<(IReadOnlyList<Lancamento> Itens, int Total)> ListarPorDataAsync(
        DateOnly data,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken);
}
