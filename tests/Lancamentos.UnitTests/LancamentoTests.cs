using FluentAssertions;
using Lancamentos.Domain.Entidades;
using Lancamentos.Domain.Enums;
using Lancamentos.Domain.Exceptions;

namespace Lancamentos.UnitTests.Dominio;

public class LancamentoTests
{
    private static readonly TimeProvider RelogioFixo =
        new FakeTimeProvider(new DateTimeOffset(2026, 8, 17, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public void Registrar_credito_valido_deve_criar_lancamento_imutavel()
    {
        var lancamento = Lancamento.Registrar(
            TipoLancamento.Credito,
            150.50m,
            new DateOnly(2026, 8, 17),
            "Venda no balcão",
            RelogioFixo);

        lancamento.Id.Should().NotBeEmpty();
        lancamento.Tipo.Should().Be(TipoLancamento.Credito);
        lancamento.Valor.Should().Be(150.50m);
        lancamento.Descricao.Should().Be("Venda no balcão");
        lancamento.CriadoEm.Should().Be(RelogioFixo.GetUtcNow());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Registrar_valor_nao_positivo_deve_falhar(decimal valor)
    {
        var act = () => Lancamento.Registrar(
            TipoLancamento.Debito,
            valor,
            new DateOnly(2026, 8, 17),
            "Compra de estoque",
            RelogioFixo);

        act.Should().Throw<RegraDeNegocioException>().Where(e => e.Codigo == "VALOR_INVALIDO");
    }

    [Fact]
    public void Registrar_valor_com_mais_de_duas_casas_deve_falhar()
    {
        var act = () => Lancamento.Registrar(
            TipoLancamento.Credito,
            10.123m,
            new DateOnly(2026, 8, 17),
            "Venda",
            RelogioFixo);

        act.Should().Throw<RegraDeNegocioException>().Where(e => e.Codigo == "VALOR_INVALIDO");
    }

    [Fact]
    public void Registrar_data_muito_futura_deve_falhar()
    {
        var act = () => Lancamento.Registrar(
            TipoLancamento.Credito,
            10m,
            new DateOnly(2026, 8, 25),
            "Venda futura",
            RelogioFixo);

        act.Should().Throw<RegraDeNegocioException>().Where(e => e.Codigo == "DATA_INVALIDA");
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("")]
    public void Registrar_descricao_invalida_deve_falhar(string descricao)
    {
        var act = () => Lancamento.Registrar(
            TipoLancamento.Credito,
            10m,
            new DateOnly(2026, 8, 17),
            descricao,
            RelogioFixo);

        act.Should().Throw<RegraDeNegocioException>().Where(e => e.Codigo == "DESCRICAO_INVALIDA");
    }
}

file sealed class FakeTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _utcNow;

    public FakeTimeProvider(DateTimeOffset utcNow) => _utcNow = utcNow;

    public override DateTimeOffset GetUtcNow() => _utcNow;
}
