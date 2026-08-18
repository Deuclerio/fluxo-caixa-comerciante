using Consolidacao.Application.Abstracoes;
using Consolidacao.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Consolidacao.Infrastructure.Persistencia;

public sealed class SaldoDiarioRepositorio : ISaldoDiarioRepositorio
{
    private readonly ConsolidacaoDbContext _context;

    public SaldoDiarioRepositorio(ConsolidacaoDbContext context)
    {
        _context = context;
    }

    public Task<SaldoDiario?> ObterPorDataAsync(DateOnly data, CancellationToken cancellationToken)
    {
        return _context.SaldosDiarios.FirstOrDefaultAsync(x => x.Data == data, cancellationToken);
    }

    public async Task<(IReadOnlyList<SaldoDiario> Itens, int Total)> ListarPorPeriodoAsync(
        DateOnly inicio,
        DateOnly fim,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken)
    {
        var consulta = _context.SaldosDiarios
            .AsNoTracking()
            .Where(x => x.Data >= inicio && x.Data <= fim);

        var total = await consulta.CountAsync(cancellationToken);
        var itens = await consulta
            .OrderBy(x => x.Data)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);

        return (itens, total);
    }

    public async Task AdicionarAsync(SaldoDiario saldo, CancellationToken cancellationToken)
    {
        await _context.SaldosDiarios.AddAsync(saldo, cancellationToken);
    }
}

public sealed class LancamentoProcessadoRepositorio : ILancamentoProcessadoRepositorio
{
    private readonly ConsolidacaoDbContext _context;

    public LancamentoProcessadoRepositorio(ConsolidacaoDbContext context)
    {
        _context = context;
    }

    public Task<bool> JaProcessadoAsync(Guid lancamentoId, CancellationToken cancellationToken)
    {
        return _context.LancamentosProcessados.AnyAsync(x => x.LancamentoId == lancamentoId, cancellationToken);
    }

    public async Task AdicionarAsync(LancamentoProcessado processado, CancellationToken cancellationToken)
    {
        await _context.LancamentosProcessados.AddAsync(processado, cancellationToken);
    }
}

public sealed class UnidadeDeTrabalho : IUnidadeDeTrabalho
{
    private readonly ConsolidacaoDbContext _context;

    public UnidadeDeTrabalho(ConsolidacaoDbContext context)
    {
        _context = context;
    }

    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
