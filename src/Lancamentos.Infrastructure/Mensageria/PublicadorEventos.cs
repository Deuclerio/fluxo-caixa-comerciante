using Lancamentos.Application.Abstracoes;
using MassTransit;
using Shared.Contracts.Eventos;

namespace Lancamentos.Infrastructure.Mensageria;

public sealed class PublicadorEventos : IPublicadorEventos
{
    private readonly IPublishEndpoint _publishEndpoint;

    public PublicadorEventos(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public Task PublicarLancamentoRegistradoAsync(
        LancamentoRegistradoEvento evento,
        CancellationToken cancellationToken)
    {
        return _publishEndpoint.Publish(evento, cancellationToken);
    }
}
