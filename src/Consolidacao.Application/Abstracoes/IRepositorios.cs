using Consolidacao.Domain.Entidades;

namespace Consolidacao.Application.Abstracoes;

public interface ISaldoDiarioRepositorio
{
    Task<SaldoDiario?> ObterPorDataAsync(DateOnly data, CancellationToken cancellationToken);
    Task<(IReadOnlyList<SaldoDiario> Itens, int Total)> ListarPorPeriodoAsync(
        DateOnly inicio,
        DateOnly fim,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken);
    Task AdicionarAsync(SaldoDiario saldo, CancellationToken cancellationToken);
}

public interface ILancamentoProcessadoRepositorio
{
    Task<bool> JaProcessadoAsync(Guid lancamentoId, CancellationToken cancellationToken);
    Task AdicionarAsync(LancamentoProcessado processado, CancellationToken cancellationToken);
}

public interface IUnidadeDeTrabalho
{
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
}

public interface ICacheSaldo
{
    Task<T?> ObterAsync<T>(string chave, CancellationToken cancellationToken);
    Task DefinirAsync<T>(string chave, T valor, TimeSpan expiracao, CancellationToken cancellationToken);
    Task RemoverAsync(string chave, CancellationToken cancellationToken);
}
