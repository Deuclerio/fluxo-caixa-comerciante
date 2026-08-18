using Consolidacao.Application.Abstracoes;
using Consolidacao.Domain.Entidades;
using Consolidacao.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Consolidacao.Application.Saldos.Comandos.AplicarLancamento;

public sealed class AplicarLancamentoCommandHandler : IRequestHandler<AplicarLancamentoCommand, bool>
{
    private readonly ISaldoDiarioRepositorio _saldos;
    private readonly ILancamentoProcessadoRepositorio _processados;
    private readonly IUnidadeDeTrabalho _unidadeDeTrabalho;
    private readonly ICacheSaldo _cache;
    private readonly TimeProvider _relogio;
    private readonly ILogger<AplicarLancamentoCommandHandler> _logger;

    public AplicarLancamentoCommandHandler(
        ISaldoDiarioRepositorio saldos,
        ILancamentoProcessadoRepositorio processados,
        IUnidadeDeTrabalho unidadeDeTrabalho,
        ICacheSaldo cache,
        TimeProvider relogio,
        ILogger<AplicarLancamentoCommandHandler> logger)
    {
        _saldos = saldos;
        _processados = processados;
        _unidadeDeTrabalho = unidadeDeTrabalho;
        _cache = cache;
        _relogio = relogio;
        _logger = logger;
    }

    public async Task<bool> Handle(AplicarLancamentoCommand request, CancellationToken cancellationToken)
    {
        if (await _processados.JaProcessadoAsync(request.LancamentoId, cancellationToken))
        {
            _logger.LogInformation(
                "Lançamento {LancamentoId} já consolidado. Mensagem ignorada (idempotência).",
                request.LancamentoId);
            return false;
        }

        if (!Enum.TryParse<TipoLancamento>(request.Tipo, ignoreCase: true, out var tipo))
        {
            _logger.LogWarning("Tipo de lançamento inválido: {Tipo}", request.Tipo);
            return false;
        }

        var saldo = await _saldos.ObterPorDataAsync(request.Data, cancellationToken);
        if (saldo is null)
        {
            saldo = SaldoDiario.CriarVazio(request.Data, _relogio);
            await _saldos.AdicionarAsync(saldo, cancellationToken);
        }

        saldo.Aplicar(tipo, request.Valor, _relogio);
        await _processados.AdicionarAsync(
            LancamentoProcessado.Registrar(request.LancamentoId, request.Data, _relogio),
            cancellationToken);

        await _unidadeDeTrabalho.SalvarAlteracoesAsync(cancellationToken);
        await _cache.RemoverAsync(ChaveCache(request.Data), cancellationToken);

        _logger.LogInformation(
            "Saldo de {Data} atualizado. Saldo atual: {Saldo}.",
            request.Data,
            saldo.Saldo);

        return true;
    }

    public static string ChaveCache(DateOnly data) => $"saldo:{data:yyyy-MM-dd}";
}
