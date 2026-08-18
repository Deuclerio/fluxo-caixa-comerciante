using Consolidacao.Application.Abstracoes;
using Consolidacao.Infrastructure.Cache;
using Consolidacao.Infrastructure.Mensageria;
using Consolidacao.Infrastructure.Persistencia;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Consolidacao.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddConsolidacaoInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var useInMemory = configuration.GetValue("Database:UseInMemory", false);

        if (useInMemory)
        {
            services.AddDbContext<ConsolidacaoDbContext>(options =>
                options.UseInMemoryDatabase("consolidacao"));
        }
        else
        {
            services.AddDbContext<ConsolidacaoDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("Consolidacao")));
        }

        services.AddScoped<ISaldoDiarioRepositorio, SaldoDiarioRepositorio>();
        services.AddScoped<ILancamentoProcessadoRepositorio, LancamentoProcessadoRepositorio>();
        services.AddScoped<IUnidadeDeTrabalho, UnidadeDeTrabalho>();
        services.AddSingleton(TimeProvider.System);

        var redis = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redis) && !configuration.GetValue("Cache:UseMemory", false))
        {
            services.AddStackExchangeRedisCache(options => options.Configuration = redis);
            services.AddSingleton<ICacheSaldo, CacheSaldoDistribuido>();
        }
        else
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheSaldo, CacheSaldoMemoria>();
        }

        var useInMemoryBus = configuration.GetValue("Messaging:UseInMemory", false);

        services.AddMassTransit(x =>
        {
            x.AddConsumer<LancamentoRegistradoConsumer>();
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

                    cfg.ReceiveEndpoint("consolidacao-lancamento-registrado", e =>
                    {
                        e.PrefetchCount = 16;
                        e.UseMessageRetry(r => r.Exponential(
                            5,
                            TimeSpan.FromSeconds(1),
                            TimeSpan.FromSeconds(30),
                            TimeSpan.FromSeconds(2)));
                        e.ConfigureConsumer<LancamentoRegistradoConsumer>(context);
                    });
                });
            }
        });

        return services;
    }
}
