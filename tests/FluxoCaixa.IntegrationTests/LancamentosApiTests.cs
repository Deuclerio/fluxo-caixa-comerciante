using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Lancamentos.Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace FluxoCaixa.IntegrationTests;

public class LancamentosApiFactory : WebApplicationFactory<Lancamentos.Api.ApiMarker>
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
                ["Jwt:Key"] = "chave-demonstracao-fluxo-caixa-exame-2026-nao-usar-em-producao!",
                ["Jwt:Issuer"] = "fluxo-caixa",
                ["Jwt:Audience"] = "fluxo-caixa-clientes",
                ["Auth:Usuario"] = "comerciante",
                ["Auth:Senha"] = "Fluxo@2026"
            });
        });
    }
}

public class LancamentosApiTests : IClassFixture<LancamentosApiFactory>
{
    private readonly HttpClient _client;

    public LancamentosApiTests(LancamentosApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_deve_retornar_ok()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Registrar_sem_token_deve_retornar_unauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/lancamentos", new
        {
            tipo = TipoLancamento.Credito,
            valor = 10,
            data = "2026-08-17",
            descricao = "Venda"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Fluxo_autenticado_deve_registrar_e_consultar_lancamento()
    {
        var tokenResponse = await _client.PostAsJsonAsync("/api/v1/auth/token", new
        {
            usuario = "comerciante",
            senha = "Fluxo@2026"
        });
        tokenResponse.EnsureSuccessStatusCode();
        var token = await tokenResponse.Content.ReadFromJsonAsync<TokenDto>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);

        var criar = await _client.PostAsJsonAsync("/api/v1/lancamentos", new
        {
            tipo = TipoLancamento.Credito,
            valor = 99.90m,
            data = DateOnly.FromDateTime(DateTime.UtcNow),
            descricao = "Venda de produto"
        });

        criar.StatusCode.Should().Be(HttpStatusCode.Created);
        var criado = await criar.Content.ReadFromJsonAsync<LancamentoCriadoDto>();
        criado.Should().NotBeNull();
        criado!.Valor.Should().Be(99.90m);

        var obter = await _client.GetAsync($"/api/v1/lancamentos/{criado.Id}");
        obter.StatusCode.Should().Be(HttpStatusCode.OK);

        var listar = await _client.GetAsync($"/api/v1/lancamentos?data={criado.Data:yyyy-MM-dd}&pagina=1&tamanhoPagina=1");
        listar.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagina = await listar.Content.ReadFromJsonAsync<PaginaDto>();
        pagina.Should().NotBeNull();
        pagina!.Itens.Should().NotBeEmpty();
        pagina.Pagina.Should().Be(1);
        pagina.TamanhoPagina.Should().Be(1);
        pagina.TotalItens.Should().BeGreaterThanOrEqualTo(1);
        pagina.TotalPaginas.Should().BeGreaterThanOrEqualTo(1);
    }

    private sealed record TokenDto(string AccessToken, string TokenType, int ExpiresIn);
    private sealed record LancamentoCriadoDto(Guid Id, string Tipo, decimal Valor, DateOnly Data, string Descricao, DateTimeOffset CriadoEm);
    private sealed record PaginaDto(LancamentoCriadoDto[] Itens, int Pagina, int TamanhoPagina, int TotalItens, int TotalPaginas);
}
