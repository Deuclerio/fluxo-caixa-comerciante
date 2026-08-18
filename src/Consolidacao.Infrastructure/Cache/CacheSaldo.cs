using System.Text.Json;
using Consolidacao.Application.Abstracoes;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace Consolidacao.Infrastructure.Cache;

public sealed class CacheSaldoDistribuido : ICacheSaldo
{
    private readonly IDistributedCache _cache;

    public CacheSaldoDistribuido(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> ObterAsync<T>(string chave, CancellationToken cancellationToken)
    {
        var json = await _cache.GetStringAsync(chave, cancellationToken);
        return json is null ? default : JsonSerializer.Deserialize<T>(json);
    }

    public Task DefinirAsync<T>(string chave, T valor, TimeSpan expiracao, CancellationToken cancellationToken)
    {
        return _cache.SetStringAsync(
            chave,
            JsonSerializer.Serialize(valor),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiracao },
            cancellationToken);
    }

    public Task RemoverAsync(string chave, CancellationToken cancellationToken)
    {
        return _cache.RemoveAsync(chave, cancellationToken);
    }
}

public sealed class CacheSaldoMemoria : ICacheSaldo
{
    private readonly IMemoryCache _cache;

    public CacheSaldoMemoria(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> ObterAsync<T>(string chave, CancellationToken cancellationToken)
    {
        _cache.TryGetValue(chave, out T? valor);
        return Task.FromResult(valor);
    }

    public Task DefinirAsync<T>(string chave, T valor, TimeSpan expiracao, CancellationToken cancellationToken)
    {
        _cache.Set(chave, valor, expiracao);
        return Task.CompletedTask;
    }

    public Task RemoverAsync(string chave, CancellationToken cancellationToken)
    {
        _cache.Remove(chave);
        return Task.CompletedTask;
    }
}
