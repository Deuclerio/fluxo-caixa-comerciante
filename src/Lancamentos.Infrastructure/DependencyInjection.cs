using Lancamentos.Application.Abstracoes;
using Lancamentos.Infrastructure.Mensageria;
using Lancamentos.Infrastructure.Persistencia;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lancamentos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLancamentosInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var useInMemory = configuration.GetValue("Database:UseInMemory", false);

        if (useInMemory)
        {
            services.AddDbContext<LancamentosDbContext>(options =>
                options.UseInMemoryDatabase("lancamentos"));
        }
        else
        {
            services.AddDbContext<LancamentosDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("Lancamentos")));
        }

        services.AddScoped<ILancamentoRepositorio, LancamentoRepositorio>();
        services.AddScoped<IUnidadeDeTrabalho, UnidadeDeTrabalho>();
        services.AddScoped<IPublicadorEventos, PublicadorEventos>();
        services.AddSingleton(TimeProvider.System);

        var useInMemoryBus = configuration.GetValue("Messaging:UseInMemory", false);

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            if (useInMemoryBus)
            {
                x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
            }
            else
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    var host = configuration["RabbitMq:Host"] ?? "localhost";
                    var user = configuration["RabbitMq:Username"] ?? "guest";
                    var pass = configuration["RabbitMq:Password"] ?? "guest";

                    cfg.Host(host, "/", h =>
                    {
                        h.Username(user);
                        h.Password(pass);
                    });

                    cfg.UseMessageRetry(r => r.Exponential(
                        5,
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(30),
                        TimeSpan.FromSeconds(2)));

                    cfg.ConfigureEndpoints(context);
                });
            }
        });

        return services;
    }
}
