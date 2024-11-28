using Microsoft.Extensions.Caching.Memory;

namespace DealerWebPageBlazorWebApp;

public class DealerShipTokenCache(IMemoryCache memoryCache)
{
  private readonly IMemoryCache _memoryCache = memoryCache;
  public record class DealershipTokenRecord(string DealerId, string TokenType);

  public void SetToken(string dealerId, string tokenType, string dealerJWTToken)
  {
    var cacheKey = GetCacheKey(dealerId, tokenType);
    _memoryCache.Set(cacheKey, dealerJWTToken);
  }
  public string? GetToken(string dealerId, string tokenType)
  {
    var cacheKey = GetCacheKey(dealerId, tokenType);
    return _memoryCache.Get<string>(cacheKey);
  }

  private string GetCacheKey(string dealerId, string tokenType)
  {
    var dealershipTokenRecord = new DealershipTokenRecord(dealerId, tokenType);
    return dealershipTokenRecord.ToString();
  }
}

