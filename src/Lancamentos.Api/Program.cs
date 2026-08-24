using System.Text;
using Lancamentos.Api.Auth;
using Lancamentos.Api.Middleware;
using Lancamentos.Application;
using Lancamentos.Infrastructure;
using Lancamentos.Infrastructure.Persistencia;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Lancamentos.Api.Swagger;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Threading.RateLimiting;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration).WriteTo.Console());

    if (builder.Environment.IsEnvironment("Testing"))
    {
        builder.Configuration["Database:UseInMemory"] = "true";
        builder.Configuration["Messaging:UseInMemory"] = "true";
    }

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Fluxo de Caixa - Lançamentos",
            Version = "v1",
            Description = "Contexto de negócio responsável pelo registro de créditos e débitos."
        });
        c.OperationFilter<ParametrosQueryObrigatoriosFilter>();
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    builder.Services.AddLancamentosApplication();
    builder.Services.AddLancamentosInfrastructure(builder.Configuration);
    builder.Services.AddSingleton<JwtTokenService>();
    builder.Services.AddExceptionHandler<TratamentoExcecoesHandler>();
    builder.Services.AddProblemDetails();

    var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key não configurada.");
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });
    builder.Services.AddAuthorization();

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddFixedWindowLimiter("padrao", limiter =>
        {
            limiter.PermitLimit = 60;
            limiter.Window = TimeSpan.FromSeconds(1);
            limiter.QueueLimit = 0;
        });
    });

    builder.Services.AddHealthChecks();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<LancamentosDbContext>();
        db.Database.EnsureCreated();
    }

    app.UseExceptionHandler();
    app.UseSerilogRequestLogging();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers().RequireRateLimiting("padrao");
    app.MapHealthChecks("/health").AllowAnonymous();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Falha ao iniciar o serviço de Lançamentos.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
