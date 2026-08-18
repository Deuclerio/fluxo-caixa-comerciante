using Shared.Contracts.Eventos;

namespace Lancamentos.Application.Abstracoes;

public interface IPublicadorEventos
{
    Task PublicarLancamentoRegistradoAsync(LancamentoRegistradoEvento evento, CancellationToken cancellationToken);
}
