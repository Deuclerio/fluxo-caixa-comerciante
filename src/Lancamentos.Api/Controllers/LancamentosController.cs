using System.ComponentModel.DataAnnotations;
using Lancamentos.Application.Lancamentos.Comandos.RegistrarLancamento;
using Lancamentos.Application.Lancamentos.Consultas.ListarLancamentos;
using Lancamentos.Application.Lancamentos.Consultas.ObterLancamento;
using Lancamentos.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lancamentos.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/lancamentos")]
public sealed class LancamentosController : ControllerBase
{
    private readonly IMediator _mediator;

    public LancamentosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Registra um crédito ou débito no fluxo de caixa diário.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(RegistrarLancamentoResultado), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistrarLancamentoRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(
            new RegistrarLancamentoCommand(request.Tipo, request.Valor, request.Data, request.Descricao),
            cancellationToken);

        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, resultado);
    }

    /// <summary>Obtém um lançamento pelo identificador.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LancamentoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var lancamento = await _mediator.Send(new ObterLancamentoQuery(id), cancellationToken);
        return lancamento is null ? NotFound() : Ok(lancamento);
    }

    /// <summary>Lista os lançamentos de um dia de caixa, com paginação.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(Lancamentos.Application.Comum.ResultadoPaginado<LancamentoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Listar(
        [FromQuery, Required(ErrorMessage = "Informe a data do caixa no formato YYYY-MM-DD.")] DateOnly? data,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        var lista = await _mediator.Send(
            new ListarLancamentosQuery(data!.Value, pagina, tamanhoPagina),
            cancellationToken);
        return Ok(lista);
    }
}

public sealed record RegistrarLancamentoRequest(
    TipoLancamento Tipo,
    decimal Valor,
    DateOnly Data,
    string Descricao);
