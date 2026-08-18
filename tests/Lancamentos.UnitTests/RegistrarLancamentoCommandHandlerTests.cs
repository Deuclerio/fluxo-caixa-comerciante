using FluentAssertions;
using Lancamentos.Application.Abstracoes;
using Lancamentos.Application.Lancamentos.Comandos.RegistrarLancamento;
using Lancamentos.Domain.Entidades;
using Lancamentos.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shared.Contracts.Eventos;

namespace Lancamentos.UnitTests.Aplicacao;

public class RegistrarLancamentoCommandHandlerTests
{
    [Fact]
    public async Task Handle_deve_persistir_e_publicar_evento()
    {
        var repositorio = Substitute.For<ILancamentoRepositorio>();
        var uow = Substitute.For<IUnidadeDeTrabalho>();
        var publicador = Substitute.For<IPublicadorEventos>();
        var relogio = TimeProvider.System;

        var handler = new RegistrarLancamentoCommandHandler(
            repositorio,
            uow,
            publicador,
            relogio,
            NullLogger<RegistrarLancamentoCommandHandler>.Instance);

        var comando = new RegistrarLancamentoCommand(
            TipoLancamento.Credito,
            200m,
            DateOnly.FromDateTime(DateTime.UtcNow),
            "Recebimento de cliente");

        var resultado = await handler.Handle(comando, CancellationToken.None);

        resultado.Valor.Should().Be(200m);
        resultado.Tipo.Should().Be("Credito");
        await repositorio.Received(1).AdicionarAsync(Arg.Any<Lancamento>(), Arg.Any<CancellationToken>());
        await uow.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
        await publicador.Received(1).PublicarLancamentoRegistradoAsync(
            Arg.Is<LancamentoRegistradoEvento>(e => e.Valor == 200m && e.Tipo == "Credito"),
            Arg.Any<CancellationToken>());
    }
}
