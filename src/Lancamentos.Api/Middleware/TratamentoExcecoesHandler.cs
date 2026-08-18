using System.Text;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Lancamentos.Domain.Exceptions;

namespace Lancamentos.Api.Middleware;

public sealed class TratamentoExcecoesHandler : IExceptionHandler
{
    private readonly ILogger<TratamentoExcecoesHandler> _logger;

    public TratamentoExcecoesHandler(ILogger<TratamentoExcecoesHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, titulo, codigo) = exception switch
        {
            RegraDeNegocioException regra => (StatusCodes.Status400BadRequest, regra.Message, regra.Codigo),
            ValidationException => (StatusCodes.Status400BadRequest, "Requisição inválida.", "VALIDACAO"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Não autorizado.", "NAO_AUTORIZADO"),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno ao processar a requisição.", "ERRO_INTERNO")
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Erro não tratado.");
        }

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/problem+json";

        var erros = exception is ValidationException validation
            ? validation.Errors.Select(e => e.ErrorMessage).ToArray()
            : Array.Empty<string>();

        var body = new
        {
            type = "https://httpstatuses.com/" + status,
            title = titulo,
            status,
            code = codigo,
            errors = erros.Length == 0 ? null : erros
        };

        await httpContext.Response.WriteAsync(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            cancellationToken);

        return true;
    }
}
