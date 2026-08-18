using FluentValidation;
using Lancamentos.Domain.Enums;

namespace Lancamentos.Application.Lancamentos.Comandos.RegistrarLancamento;

public sealed class RegistrarLancamentoCommandValidator : AbstractValidator<RegistrarLancamentoCommand>
{
    public RegistrarLancamentoCommandValidator()
    {
        RuleFor(x => x.Tipo)
            .IsInEnum()
            .WithMessage("O tipo deve ser Credito (1) ou Debito (2).");

        RuleFor(x => x.Valor)
            .GreaterThan(0)
            .WithMessage("O valor deve ser maior que zero.")
            .Must(v => decimal.Round(v, 2) == v)
            .WithMessage("O valor deve ter no máximo duas casas decimais.");

        RuleFor(x => x.Descricao)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(200);
    }
}
