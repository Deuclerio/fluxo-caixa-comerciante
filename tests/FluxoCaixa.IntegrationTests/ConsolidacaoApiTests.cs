using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FluxoCaixa.IntegrationTests;

public class ConsolidacaoApiFactory : WebApplicationFactory<Consolidacao.Api.ApiMarker>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:UseInMemory"] = "true",
                ["Messaging:UseInMemory"] = "true",
                ["Cache:UseMemory"] = "true",
                ["Jwt:Key"] = "chave-demonstracao-fluxo-caixa-exame-2026-nao-usar-em-producao!",
                ["Jwt:Issuer"] = "fluxo-caixa",
                ["Jwt:Audience"] = "fluxo-caixa-clientes"
            });
        });
    }
}

public class ConsolidacaoApiTests : IClassFixture<ConsolidacaoApiFactory>
{
    private readonly HttpClient _client;

    public ConsolidacaoApiTests(ConsolidacaoApiFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GerarToken());
    }

    [Fact]
    public async Task Health_deve_retornar_ok()
    {
        var anonimo = new ConsolidacaoApiFactory().CreateClient();
        var response = await anonimo.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Obter_saldo_de_dia_sem_lancamentos_deve_retornar_zero()
    {
        var response = await _client.GetAsync("/api/v1/saldos/2026-08-17");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var saldo = await response.Content.ReadFromJsonAsync<SaldoDto>();
        saldo.Should().NotBeNull();
        saldo!.Saldo.Should().Be(0);
        saldo.QuantidadeLancamentos.Should().Be(0);
    }

    private static string GerarToken()
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("chave-demonstracao-fluxo-caixa-exame-2026-nao-usar-em-producao!"));
        var token = new JwtSecurityToken(
            issuer: "fluxo-caixa",
            audience: "fluxo-caixa-clientes",
            claims: [new Claim(ClaimTypes.Role, "comerciante")],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed record SaldoDto(
        DateOnly Data,
        decimal TotalCreditos,
        decimal TotalDebitos,
        decimal Saldo,
        int QuantidadeLancamentos,
        DateTimeOffset? AtualizadoEm);
}
