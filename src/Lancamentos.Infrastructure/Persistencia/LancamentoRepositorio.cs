using Lancamentos.Application.Abstracoes;
using Lancamentos.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Lancamentos.Infrastructure.Persistencia;

public sealed class LancamentoRepositorio : ILancamentoRepositorio
{
    private readonly LancamentosDbContext _context;

    public LancamentoRepositorio(LancamentosDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(Lancamento lancamento, CancellationToken cancellationToken)
    {
        await _context.Lancamentos.AddAsync(lancamento, cancellationToken);
    }

    public Task<Lancamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _context.Lancamentos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Lancamento> Itens, int Total)> ListarPorDataAsync(
        DateOnly data,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken)
    {
        var consulta = _context.Lancamentos.AsNoTracking().Where(x => x.Data == data);
        var total = await consulta.CountAsync(cancellationToken);
        var itens = await consulta
            .OrderBy(x => x.CriadoEm)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);

        return (itens, total);
    }
}

public sealed class UnidadeDeTrabalho : IUnidadeDeTrabalho
{
    private readonly LancamentosDbContext _context;

    public UnidadeDeTrabalho(LancamentosDbContext context)
    {
        _context = context;
    }

    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
