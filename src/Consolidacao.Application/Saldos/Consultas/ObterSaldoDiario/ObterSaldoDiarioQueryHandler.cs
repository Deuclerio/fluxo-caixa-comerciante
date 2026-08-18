using Consolidacao.Application.Abstracoes;
using Consolidacao.Application.Saldos.Comandos.AplicarLancamento;
using MediatR;

namespace Consolidacao.Application.Saldos.Consultas.ObterSaldoDiario;

public sealed class ObterSaldoDiarioQueryHandler : IRequestHandler<ObterSaldoDiarioQuery, SaldoDiarioDto>
{
    private readonly ISaldoDiarioRepositorio _repositorio;
    private readonly ICacheSaldo _cache;

    public ObterSaldoDiarioQueryHandler(ISaldoDiarioRepositorio repositorio, ICacheSaldo cache)
    {
        _repositorio = repositorio;
        _cache = cache;
    }

    public async Task<SaldoDiarioDto> Handle(ObterSaldoDiarioQuery request, CancellationToken cancellationToken)
    {
        var chave = AplicarLancamentoCommandHandler.ChaveCache(request.Data);
        var cached = await _cache.ObterAsync<SaldoDiarioDto>(chave, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var saldo = await _repositorio.ObterPorDataAsync(request.Data, cancellationToken);
        var dto = saldo is null
            ? new SaldoDiarioDto(request.Data, 0, 0, 0, 0, null)
            : new SaldoDiarioDto(
                saldo.Data,
                saldo.TotalCreditos,
                saldo.TotalDebitos,
                saldo.Saldo,
                saldo.QuantidadeLancamentos,
                saldo.AtualizadoEm);

        await _cache.DefinirAsync(chave, dto, TimeSpan.FromMinutes(5), cancellationToken);
        return dto;
    }
}
