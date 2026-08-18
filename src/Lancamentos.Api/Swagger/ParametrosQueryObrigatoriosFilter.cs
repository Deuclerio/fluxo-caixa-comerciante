using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lancamentos.Api.Swagger;

/// <summary>
/// Garante que parâmetros de query com [Required]/[BindRequired] apareçam
/// como obrigatórios no Swagger UI (asterisco vermelho).
/// DateOnly é struct e o Swashbuckle, por padrão, trata query string como opcional.
/// </summary>
public sealed class ParametrosQueryObrigatoriosFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters is null || operation.Parameters.Count == 0)
        {
            return;
        }

        var obrigatorios = context.MethodInfo
            .GetParameters()
            .Where(EhObrigatorio)
            .Select(p => p.Name!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var propriedade in context.MethodInfo.GetParameters().SelectMany(p => p.ParameterType.GetProperties()))
        {
            if (EhObrigatorio(propriedade))
            {
                obrigatorios.Add(propriedade.Name);
            }
        }

        foreach (var parametro in operation.Parameters)
        {
            if (parametro.In == ParameterLocation.Query && obrigatorios.Contains(parametro.Name))
            {
                parametro.Required = true;
            }
        }
    }

    private static bool EhObrigatorio(ICustomAttributeProvider membro)
    {
        return membro.GetCustomAttributes(true).Any(a => a is RequiredAttribute or BindRequiredAttribute);
    }
}
