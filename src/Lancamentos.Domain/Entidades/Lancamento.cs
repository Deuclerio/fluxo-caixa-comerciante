using Lancamentos.Domain.Enums;
using Lancamentos.Domain.Exceptions;

namespace Lancamentos.Domain.Entidades;

/// <summary>
/// Agregado raiz do contexto de Lançamentos.
/// Um lançamento é imutável após o registro (princípio contábil de não alteração).
/// </summary>
public sealed class Lancamento
{
    public Guid Id { get; private set; }
    public TipoLancamento Tipo { get; private set; }
    public decimal Valor { get; private set; }
    public DateOnly Data { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public DateTimeOffset CriadoEm { get; private set; }

    private Lancamento()
    {
    }

    public static Lancamento Registrar(
        TipoLancamento tipo,
        decimal valor,
        DateOnly data,
        string descricao,
        TimeProvider? relogio = null)
    {
        if (!Enum.IsDefined(tipo))
        {
            throw new RegraDeNegocioException("TIPO_INVALIDO", "O tipo do lançamento deve ser Credito ou Debito.");
        }

        if (valor <= 0)
        {
            throw new RegraDeNegocioException("VALOR_INVALIDO", "O valor do lançamento deve ser maior que zero.");
        }

        if (decimal.Round(valor, 2) != valor)
        {
            throw new RegraDeNegocioException("VALOR_INVALIDO", "O valor deve ter no máximo duas casas decimais.");
        }

        var clock = relogio ?? TimeProvider.System;
        var hojeUtc = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
        if (data > hojeUtc.AddDays(1))
        {
            throw new RegraDeNegocioException("DATA_INVALIDA", "A data do lançamento não pode ser superior a um dia no futuro.");
        }

        var texto = (descricao ?? string.Empty).Trim();
        if (texto.Length is < 3 or > 200)
        {
            throw new RegraDeNegocioException("DESCRICAO_INVALIDA", "A descrição deve ter entre 3 e 200 caracteres.");
        }

        return new Lancamento
        {
            Id = Guid.NewGuid(),
            Tipo = tipo,
            Valor = valor,
            Data = data,
            Descricao = texto,
            CriadoEm = clock.GetUtcNow()
        };
    }
}
