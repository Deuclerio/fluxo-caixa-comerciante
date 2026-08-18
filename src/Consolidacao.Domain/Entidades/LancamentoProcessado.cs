namespace Consolidacao.Domain.Entidades;

/// <summary>
/// Registro de idempotência: garante que um lançamento não seja consolidado duas vezes
/// em caso de reentrega de mensagem (at-least-once).
/// </summary>
public sealed class LancamentoProcessado
{
    public Guid LancamentoId { get; private set; }
    public DateOnly Data { get; private set; }
    public DateTimeOffset ProcessadoEm { get; private set; }

    private LancamentoProcessado()
    {
    }

    public static LancamentoProcessado Registrar(Guid lancamentoId, DateOnly data, TimeProvider? relogio = null)
    {
        return new LancamentoProcessado
        {
            LancamentoId = lancamentoId,
            Data = data,
            ProcessadoEm = (relogio ?? TimeProvider.System).GetUtcNow()
        };
    }
}
