using Microsoft.Extensions.Caching.Memory;

namespace DealerWebPageBlazorWebAppShared.Resources;

public class DealerShipTokenCache()
{
  public record class DealershipTokenRecord(string DealerId, string TokenType);

  public Dictionary<DealershipTokenRecord, string> DealerJWTTokenDictionary = [];
  public HashSet<string> DealerJWTTokenList = [];


  public void UpsertJwtToken(string dealerId, TokenTypeValues tokenTypeValue, string dealerJWTToken)
  {
    if (tokenTypeValue == TokenTypeValues.Unknown)
    {
      throw new ArgumentException("TokenType is required or is unknown", nameof(tokenTypeValue));
    }

    var cacheKey = GetCacheKey(dealerId, tokenTypeValue);
    if (!DealerJWTTokenDictionary.TryAdd(cacheKey, dealerJWTToken))
    {
      DealerJWTTokenDictionary[cacheKey] = dealerJWTToken;
    }

    if (DealerJWTTokenList.Contains(dealerId)==false)
      DealerJWTTokenList.Add(dealerId);
  }

  public string? GetJwtToken(string dealershipId, TokenTypeValues tokenType)
  {
    var cacheKey = GetCacheKey(dealershipId, tokenType);
    return this.DealerJWTTokenDictionary.TryGetValue(cacheKey, out var token) ? token : null;
  }

  private DealershipTokenRecord GetCacheKey(string dealershipId, TokenTypeValues  tokenTypeValue)
  {
    if (tokenTypeValue == TokenTypeValues.Unknown)
    {
      throw new ArgumentException("TokenType is required or is unknown", nameof(tokenTypeValue));
    }

    var dealershipTokenRecord = new DealershipTokenRecord(dealershipId, tokenTypeValue.ToString());
    return dealershipTokenRecord;
  }
}

