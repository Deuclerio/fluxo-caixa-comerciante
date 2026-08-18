using Consolidacao.Application.Abstracoes;
using Consolidacao.Application.Saldos.Comandos.AplicarLancamento;
using Consolidacao.Domain.Entidades;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Consolidacao.UnitTests.Aplicacao;

public class AplicarLancamentoCommandHandlerTests
{
    [Fact]
    public async Task Handle_deve_criar_saldo_e_aplicar_lancamento()
    {
        var saldos = Substitute.For<ISaldoDiarioRepositorio>();
        var processados = Substitute.For<ILancamentoProcessadoRepositorio>();
        var uow = Substitute.For<IUnidadeDeTrabalho>();
        var cache = Substitute.For<ICacheSaldo>();

        processados.JaProcessadoAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        saldos.ObterPorDataAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>()).Returns((SaldoDiario?)null);

        var handler = new AplicarLancamentoCommandHandler(
            saldos,
            processados,
            uow,
            cache,
            TimeProvider.System,
            NullLogger<AplicarLancamentoCommandHandler>.Instance);

        var data = new DateOnly(2026, 8, 17);
        var id = Guid.NewGuid();

        var aplicado = await handler.Handle(
            new AplicarLancamentoCommand(id, "Credito", 120m, data),
            CancellationToken.None);

        aplicado.Should().BeTrue();
        await saldos.Received(1).AdicionarAsync(Arg.Any<SaldoDiario>(), Arg.Any<CancellationToken>());
        await processados.Received(1).AdicionarAsync(Arg.Any<LancamentoProcessado>(), Arg.Any<CancellationToken>());
        await uow.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
        await cache.Received(1).RemoverAsync($"saldo:{data:yyyy-MM-dd}", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_deve_ser_idempotente_quando_lancamento_ja_foi_processado()
    {
        var saldos = Substitute.For<ISaldoDiarioRepositorio>();
        var processados = Substitute.For<ILancamentoProcessadoRepositorio>();
        var uow = Substitute.For<IUnidadeDeTrabalho>();
        var cache = Substitute.For<ICacheSaldo>();

        processados.JaProcessadoAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);

        var handler = new AplicarLancamentoCommandHandler(
            saldos,
            processados,
            uow,
            cache,
            TimeProvider.System,
            NullLogger<AplicarLancamentoCommandHandler>.Instance);

        var aplicado = await handler.Handle(
            new AplicarLancamentoCommand(Guid.NewGuid(), "Debito", 10m, new DateOnly(2026, 8, 17)),
            CancellationToken.None);

        aplicado.Should().BeFalse();
        await saldos.DidNotReceive().AdicionarAsync(Arg.Any<SaldoDiario>(), Arg.Any<CancellationToken>());
        await uow.DidNotReceive().SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }
}
