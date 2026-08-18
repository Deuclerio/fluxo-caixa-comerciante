using Consolidacao.Application.Saldos.Comandos.AplicarLancamento;
using MassTransit;
using MediatR;
using Shared.Contracts.Eventos;

namespace Consolidacao.Infrastructure.Mensageria;

public sealed class LancamentoRegistradoConsumer : IConsumer<LancamentoRegistradoEvento>
{
    private readonly IMediator _mediator;

    public LancamentoRegistradoConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task Consume(ConsumeContext<LancamentoRegistradoEvento> context)
    {
        var msg = context.Message;
        return _mediator.Send(
            new AplicarLancamentoCommand(msg.LancamentoId, msg.Tipo, msg.Valor, msg.Data),
            context.CancellationToken);
    }
}
