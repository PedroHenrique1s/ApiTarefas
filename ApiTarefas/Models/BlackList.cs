namespace ApiTarefas.Models;

using System.Collections.Concurrent;

public class BlackList
{
    readonly ConcurrentDictionary<string, DateTime> TokensRevogados = new();

    public Task<bool> ChecarTokenRevogado(string jti)
    {
        if (TokensRevogados.TryGetValue(jti, out var dataExp))
        {
            if (DateTime.UtcNow < dataExp) return Task.FromResult(true);
            TokensRevogados.TryRemove(jti, out _);
        }
        return Task.FromResult(false);
    }

    public Task RevogarToken(string jti, DateTime dataExp)
    {
        TokensRevogados.TryAdd(jti, dataExp);
        return Task.CompletedTask;
    }
}