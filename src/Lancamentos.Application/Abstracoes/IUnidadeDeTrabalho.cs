namespace Lancamentos.Application.Abstracoes;

public interface IUnidadeDeTrabalho
{
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
}
