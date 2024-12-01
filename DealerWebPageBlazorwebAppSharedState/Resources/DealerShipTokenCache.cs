using Microsoft.Extensions.Caching.Memory;

namespace DealerWebPageBlazorWebAppShared.Resources;

public class DealerShipTokenCache(IMemoryCache memoryCache)
{
  private readonly IMemoryCache _memoryCache = memoryCache;
  public record class DealershipTokenRecord(string DealerId, string TokenType);

  public void SetToken(string dealerId, TokenTypeValues tokenTypeValue, string dealerJWTToken)
  {
    if (tokenTypeValue == TokenTypeValues.Unknown)
    {
      throw new ArgumentException("TokenType is required or is unknown", nameof(tokenTypeValue));
    }

    var cacheKey = GetCacheKey(dealerId, tokenTypeValue);
    _memoryCache.Set(cacheKey, dealerJWTToken);
  }

  public string? GetToken(string dealerId, TokenTypeValues tokenType)
  {
    var cacheKey = GetCacheKey(dealerId, tokenType);
    return _memoryCache.Get<string>(cacheKey);
  }

  private string GetCacheKey(string dealerId, TokenTypeValues  tokenTypeValue)
  {
    if (tokenTypeValue == TokenTypeValues.Unknown)
    {
      throw new ArgumentException("TokenType is required or is unknown", nameof(tokenTypeValue));
    }

    var dealershipTokenRecord = new DealershipTokenRecord(dealerId, tokenTypeValue.ToString());
    return dealershipTokenRecord.ToString();
  }
}

