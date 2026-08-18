using Consolidacao.Domain.Enums;
using Consolidacao.Domain.Exceptions;

namespace Consolidacao.Domain.Entidades;

/// <summary>
/// Agregado do contexto de Consolidação.
/// Representa o saldo consolidado de um dia de caixa do comerciante.
/// </summary>
public sealed class SaldoDiario
{
    public DateOnly Data { get; private set; }
    public decimal TotalCreditos { get; private set; }
    public decimal TotalDebitos { get; private set; }
    public decimal Saldo { get; private set; }
    public int QuantidadeLancamentos { get; private set; }
    public DateTimeOffset AtualizadoEm { get; private set; }

    private SaldoDiario()
    {
    }

    public static SaldoDiario CriarVazio(DateOnly data, TimeProvider? relogio = null)
    {
        var clock = relogio ?? TimeProvider.System;
        return new SaldoDiario
        {
            Data = data,
            TotalCreditos = 0,
            TotalDebitos = 0,
            Saldo = 0,
            QuantidadeLancamentos = 0,
            AtualizadoEm = clock.GetUtcNow()
        };
    }

    public void Aplicar(TipoLancamento tipo, decimal valor, TimeProvider? relogio = null)
    {
        if (valor <= 0)
        {
            throw new RegraDeNegocioException("VALOR_INVALIDO", "Não é possível consolidar um valor menor ou igual a zero.");
        }

        if (tipo == TipoLancamento.Credito)
        {
            TotalCreditos += valor;
        }
        else if (tipo == TipoLancamento.Debito)
        {
            TotalDebitos += valor;
        }
        else
        {
            throw new RegraDeNegocioException("TIPO_INVALIDO", "Tipo de lançamento não reconhecido na consolidação.");
        }

        Saldo = TotalCreditos - TotalDebitos;
        QuantidadeLancamentos++;
        AtualizadoEm = (relogio ?? TimeProvider.System).GetUtcNow();
    }
}
