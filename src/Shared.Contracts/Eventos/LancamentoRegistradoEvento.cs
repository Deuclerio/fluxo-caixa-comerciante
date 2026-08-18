namespace Shared.Contracts.Eventos;

/// <summary>
/// Evento de integração publicado pelo contexto de Lançamentos.
/// Contrato estável entre bounded contexts — versionado e independente de persistência.
/// </summary>
public sealed record LancamentoRegistradoEvento
{
    public Guid LancamentoId { get; init; }
    public string Tipo { get; init; } = string.Empty;
    public decimal Valor { get; init; }
    public DateOnly Data { get; init; }
    public string Descricao { get; init; } = string.Empty;
    public DateTimeOffset OcorridoEm { get; init; }
}
