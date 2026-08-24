using System.Text;
using Consolidacao.Api.Middleware;
using Consolidacao.Application;
using Consolidacao.Infrastructure;
using Consolidacao.Infrastructure.Persistencia;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Consolidacao.Api.Swagger;
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
        builder.Configuration["Cache:UseMemory"] = "true";
    }

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Fluxo de Caixa - Consolidação",
            Version = "v1",
            Description = "Contexto de negócio responsável pelo saldo diário consolidado."
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

    builder.Services.AddConsolidacaoApplication();
    builder.Services.AddConsolidacaoInfrastructure(builder.Configuration);
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
        var db = scope.ServiceProvider.GetRequiredService<ConsolidacaoDbContext>();
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
    Log.Fatal(ex, "Falha ao iniciar o serviço de Consolidação.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
