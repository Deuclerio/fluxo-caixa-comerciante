namespace Lancamentos.Domain.Exceptions;

public sealed class RegraDeNegocioException : Exception
{
    public string Codigo { get; }

    public RegraDeNegocioException(string codigo, string mensagem) : base(mensagem)
    {
        Codigo = codigo;
    }
}
