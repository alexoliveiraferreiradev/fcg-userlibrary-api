namespace Fcg.UserLibrary.Application.Common.Interfaces
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string chaveCache, CancellationToken cancellation);
        Task SetAsync<T>(string chave, T Amount, TimeSpan tempoExpiracao, CancellationToken cancellation);
        Task RemoveAsync(string chaveCache);
        Task RemoveByPrefixAsync(string prefixo);
    }
}
