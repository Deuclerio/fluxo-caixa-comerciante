using Lancamentos.Application.Comum;
using Lancamentos.Application.Lancamentos.Consultas.ObterLancamento;
using MediatR;

namespace Lancamentos.Application.Lancamentos.Consultas.ListarLancamentos;

public sealed record ListarLancamentosQuery(DateOnly Data, int Pagina, int TamanhoPagina)
    : IRequest<ResultadoPaginado<LancamentoDto>>;
