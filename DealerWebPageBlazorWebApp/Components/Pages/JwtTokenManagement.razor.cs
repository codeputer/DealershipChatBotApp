namespace DealerWebPageBlazorWebApp.Components.Pages;

public partial class JwtTokenManagement
{
  [Inject]
  public required IHttpClientFactory httpClientFactory { get; init; }

  [Inject]
  public required AppSettings AppSettings { get; init; }

  [Inject]
  public required DealerShipTokenCache DealerShipTokenCache { get; init; }

  [Inject]
  public required JWTTokensDTO JWTTokensDTO { get; set; }

  private string dealershipId = string.Empty;
  private string jwtEncryptedBase64EncodedPanel = string.Empty;
  private TokenTypeValues defaultTokenTypeValue = TokenTypeValues.DealershipToken;
  private string decryptedTokenPanel = string.Empty;

  public async Task CreateNewToken()
  {
    if (ValidateDealerIdAndReset() == false)
    {
      return;
    }

    var newJWTToken = await GetNewJwtToken();

    DealerShipTokenCache.SetToken(dealershipId, defaultTokenTypeValue, newJWTToken);

    jwtEncryptedBase64EncodedPanel = newJWTToken;
    
  }
  

  /// <summary>
  /// Validates that we have a DealerID, and if so, we reset the UX and process
  /// </summary>
  /// <returns></returns>
  public bool ValidateDealerIdAndReset()
  {
    if (string.IsNullOrEmpty(dealershipId))
    {
      decryptedTokenPanel = "No dealership id provided.";
      return false;
    }

    this.jwtEncryptedBase64EncodedPanel = string.Empty;
    this.decryptedTokenPanel = string.Empty;
    return true;
  }

  public void UseCachedToken()
  {
    if (ValidateDealerIdAndReset()==false)
      return;

    jwtEncryptedBase64EncodedPanel = GetCachedJwtToken();
    
  }

  private string GetCachedJwtToken()
  {
    if (string.IsNullOrEmpty(dealershipId))
    {
      return "No dealership id provided.";
    }

    if (defaultTokenTypeValue == TokenTypeValues.Unknown)
    {
      return "No token type provided.";
    }

    return DealerShipTokenCache.GetToken(dealershipId, defaultTokenTypeValue) ?? $"No token found in cache for Dealership Id:>{dealershipId}< & TokenType:>{defaultTokenTypeValue}<";
  }

  private async Task<string> GetNewJwtToken()
  {
    var generateTokenHostUrl = APIRoutes.GetUrlPath(APIRoutes.DealershipChatBotAPIRoutes.GenerateToken);  

    IDictionary<string, string?> queryParams = new Dictionary<string, string?>
        {
            { ClaimKeyValues.DealershipId.ToString(), dealershipId },
            { ClaimKeyValues.TokenType.ToString(), defaultTokenTypeValue.ToString() }
        };
    var urlWithQueryString = QueryHelpers.AddQueryString(generateTokenHostUrl, queryParams);

    var dealershipChatBotHttpClient = httpClientFactory.GetNamedHttpClient(HttpNamedClients.DealershipChatBot);
    var response = await dealershipChatBotHttpClient.GetAsync(urlWithQueryString);
    var jwtToken = string.Empty;
    if (response.IsSuccessStatusCode)
    {
      jwtToken = await response.Content.ReadAsStringAsync();
      jwtToken = jwtToken.Trim('"'); //the token and jwtpanel are in base 64 encoded format, no quotes
    }
    else
    {
      jwtToken = "Failed to generate token.";
    }

    return jwtToken;
  }

  private async Task DecryptBase64Token()
  {
    //panel is in base 64 encoded format, no q
    if (string.IsNullOrEmpty(jwtEncryptedBase64EncodedPanel))
    {
      decryptedTokenPanel = "No token to decrypt.";
      return;
    }

    var endpoint = "/api/DecryptToken";
    IDictionary<string, string?> queryParams = new Dictionary<string, string?>
        {
            { "encryptedToken", jwtEncryptedBase64EncodedPanel }
        };
    var urlWithQueryString = QueryHelpers.AddQueryString(endpoint, queryParams);

    var httpDecryptJWTTokenClient = httpClientFactory.CreateClient("DealerWebPageBlazorWebApp.DealershipChatBot");
    var response = await httpDecryptJWTTokenClient.GetAsync(urlWithQueryString);
    if (response.IsSuccessStatusCode)
    {
      var responseContent = await response.Content.ReadAsStringAsync();
      decryptedTokenPanel = responseContent.Trim('"'); // Remove quotes from the response
    }
    else
    {
      decryptedTokenPanel = "Failed to decrypt token.";
    }
  }
}
