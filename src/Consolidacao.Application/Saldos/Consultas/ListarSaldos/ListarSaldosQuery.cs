using Consolidacao.Application.Comum;
using Consolidacao.Application.Saldos.Consultas.ObterSaldoDiario;
using MediatR;

namespace Consolidacao.Application.Saldos.Consultas.ListarSaldos;

public sealed record ListarSaldosQuery(DateOnly Inicio, DateOnly Fim, int Pagina, int TamanhoPagina)
    : IRequest<ResultadoPaginado<SaldoDiarioDto>>;
