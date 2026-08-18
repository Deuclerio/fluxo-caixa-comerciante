namespace Lancamentos.Application.Comum;

public sealed record ResultadoPaginado<T>(
    IReadOnlyList<T> Itens,
    int Pagina,
    int TamanhoPagina,
    int TotalItens,
    int TotalPaginas)
{
    public static ResultadoPaginado<T> Criar(IReadOnlyList<T> itens, int pagina, int tamanhoPagina, int totalItens)
    {
        var totalPaginas = totalItens == 0
            ? 0
            : (int)Math.Ceiling(totalItens / (double)tamanhoPagina);

        return new ResultadoPaginado<T>(itens, pagina, tamanhoPagina, totalItens, totalPaginas);
    }
}
