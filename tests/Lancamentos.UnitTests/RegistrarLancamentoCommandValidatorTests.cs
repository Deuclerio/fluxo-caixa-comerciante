using FluentAssertions;
using FluentValidation;
using Lancamentos.Application.Lancamentos.Comandos.RegistrarLancamento;
using Lancamentos.Domain.Enums;

namespace Lancamentos.UnitTests.Aplicacao;

public class RegistrarLancamentoCommandValidatorTests
{
    private readonly RegistrarLancamentoCommandValidator _validator = new();

    [Fact]
    public void Comando_valido_nao_deve_ter_erros()
    {
        var comando = new RegistrarLancamentoCommand(
            TipoLancamento.Debito,
            50.10m,
            new DateOnly(2026, 8, 17),
            "Pagamento de fornecedor");

        _validator.Validate(comando).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Valor_zero_deve_ser_rejeitado()
    {
        var comando = new RegistrarLancamentoCommand(
            TipoLancamento.Credito,
            0,
            new DateOnly(2026, 8, 17),
            "Venda");

        _validator.Validate(comando).IsValid.Should().BeFalse();
    }
}
