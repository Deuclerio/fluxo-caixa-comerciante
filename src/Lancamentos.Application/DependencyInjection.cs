using FluentValidation;
using Lancamentos.Application.Comportamentos;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Lancamentos.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddLancamentosApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidacaoBehavior<,>));
        return services;
    }
}
