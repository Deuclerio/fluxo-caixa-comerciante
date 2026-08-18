using Consolidacao.Domain.Entidades;
using Consolidacao.Domain.Enums;
using Consolidacao.Domain.Exceptions;
using FluentAssertions;

namespace Consolidacao.UnitTests.Dominio;

public class SaldoDiarioTests
{
    [Fact]
    public void Aplicar_credito_e_debito_deve_calcular_saldo()
    {
        var saldo = SaldoDiario.CriarVazio(new DateOnly(2026, 8, 17));

        saldo.Aplicar(TipoLancamento.Credito, 100m);
        saldo.Aplicar(TipoLancamento.Debito, 30m);

        saldo.TotalCreditos.Should().Be(100m);
        saldo.TotalDebitos.Should().Be(30m);
        saldo.Saldo.Should().Be(70m);
        saldo.QuantidadeLancamentos.Should().Be(2);
    }

    [Fact]
    public void Aplicar_somente_debitos_pode_gerar_saldo_negativo()
    {
        var saldo = SaldoDiario.CriarVazio(new DateOnly(2026, 8, 17));
        saldo.Aplicar(TipoLancamento.Debito, 80m);
        saldo.Saldo.Should().Be(-80m);
    }

    [Fact]
    public void Aplicar_valor_invalido_deve_falhar()
    {
        var saldo = SaldoDiario.CriarVazio(new DateOnly(2026, 8, 17));
        var act = () => saldo.Aplicar(TipoLancamento.Credito, 0);
        act.Should().Throw<RegraDeNegocioException>().Where(e => e.Codigo == "VALOR_INVALIDO");
    }
}
