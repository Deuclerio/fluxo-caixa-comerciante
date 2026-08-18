using FluentAssertions;
using Lancamentos.Application.Comum;

namespace Lancamentos.UnitTests.Aplicacao;

public class PaginaTests
{
    [Fact]
    public void Criar_deve_calcular_total_de_paginas()
    {
        var pagina = ResultadoPaginado<int>.Criar([1, 2], pagina: 1, tamanhoPagina: 2, totalItens: 5);

        pagina.Itens.Should().HaveCount(2);
        pagina.Pagina.Should().Be(1);
        pagina.TamanhoPagina.Should().Be(2);
        pagina.TotalItens.Should().Be(5);
        pagina.TotalPaginas.Should().Be(3);
    }

    [Fact]
    public void Criar_sem_itens_deve_ter_zero_paginas()
    {
        var pagina = ResultadoPaginado<int>.Criar([], pagina: 1, tamanhoPagina: 20, totalItens: 0);
        pagina.TotalPaginas.Should().Be(0);
    }
}
