using Lancamentos.Application.Abstracoes;
using Lancamentos.Domain.Entidades;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Eventos;

namespace Lancamentos.Application.Lancamentos.Comandos.RegistrarLancamento;

public sealed class RegistrarLancamentoCommandHandler
    : IRequestHandler<RegistrarLancamentoCommand, RegistrarLancamentoResultado>
{
    private readonly ILancamentoRepositorio _repositorio;
    private readonly IUnidadeDeTrabalho _unidadeDeTrabalho;
    private readonly IPublicadorEventos _publicador;
    private readonly TimeProvider _relogio;
    private readonly ILogger<RegistrarLancamentoCommandHandler> _logger;

    public RegistrarLancamentoCommandHandler(
        ILancamentoRepositorio repositorio,
        IUnidadeDeTrabalho unidadeDeTrabalho,
        IPublicadorEventos publicador,
        TimeProvider relogio,
        ILogger<RegistrarLancamentoCommandHandler> logger)
    {
        _repositorio = repositorio;
        _unidadeDeTrabalho = unidadeDeTrabalho;
        _publicador = publicador;
        _relogio = relogio;
        _logger = logger;
    }

    public async Task<RegistrarLancamentoResultado> Handle(
        RegistrarLancamentoCommand request,
        CancellationToken cancellationToken)
    {
        var lancamento = Lancamento.Registrar(
            request.Tipo,
            request.Valor,
            request.Data,
            request.Descricao,
            _relogio);

        await _repositorio.AdicionarAsync(lancamento, cancellationToken);
        await _unidadeDeTrabalho.SalvarAlteracoesAsync(cancellationToken);

        var evento = new LancamentoRegistradoEvento
        {
            LancamentoId = lancamento.Id,
            Tipo = lancamento.Tipo.ToString(),
            Valor = lancamento.Valor,
            Data = lancamento.Data,
            Descricao = lancamento.Descricao,
            OcorridoEm = lancamento.CriadoEm
        };

        await _publicador.PublicarLancamentoRegistradoAsync(evento, cancellationToken);

        _logger.LogInformation(
            "Lançamento {LancamentoId} registrado ({Tipo} de {Valor} em {Data}).",
            lancamento.Id,
            lancamento.Tipo,
            lancamento.Valor,
            lancamento.Data);

        return new RegistrarLancamentoResultado(
            lancamento.Id,
            lancamento.Tipo.ToString(),
            lancamento.Valor,
            lancamento.Data,
            lancamento.Descricao,
            lancamento.CriadoEm);
    }
}
