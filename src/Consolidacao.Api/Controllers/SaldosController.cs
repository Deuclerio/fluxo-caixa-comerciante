using System.ComponentModel.DataAnnotations;
using Consolidacao.Application.Saldos.Consultas.ListarSaldos;
using Consolidacao.Application.Saldos.Consultas.ObterSaldoDiario;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Consolidacao.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/saldos")]
public sealed class SaldosController : ControllerBase
{
    private readonly IMediator _mediator;

    public SaldosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Obtém o saldo consolidado de um dia de caixa.</summary>
    [HttpGet("{data}")]
    [ProducesResponseType(typeof(SaldoDiarioDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterPorData(DateOnly data, CancellationToken cancellationToken)
    {
        var saldo = await _mediator.Send(new ObterSaldoDiarioQuery(data), cancellationToken);
        return Ok(saldo);
    }

    /// <summary>Lista os saldos consolidados em um intervalo de datas (máximo 90 dias), com paginação.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(Consolidacao.Application.Comum.ResultadoPaginado<SaldoDiarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Listar(
        [FromQuery, Required(ErrorMessage = "Informe a data inicial no formato YYYY-MM-DD.")] DateOnly? inicio,
        [FromQuery, Required(ErrorMessage = "Informe a data final no formato YYYY-MM-DD.")] DateOnly? fim,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        var lista = await _mediator.Send(
            new ListarSaldosQuery(inicio!.Value, fim!.Value, pagina, tamanhoPagina),
            cancellationToken);
        return Ok(lista);
    }
}
