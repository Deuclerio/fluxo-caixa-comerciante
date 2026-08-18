using Consolidacao.Application.Comportamentos;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Consolidacao.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddConsolidacaoApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidacaoBehavior<,>));
        return services;
    }
}
