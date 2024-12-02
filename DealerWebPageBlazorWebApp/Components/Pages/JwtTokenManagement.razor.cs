namespace DealerWebPageBlazorWebApp.Components.Pages;

public partial class JwtTokenManagement
{
  public JwtTokenManagement()
  {
    
  }


  [Inject]
  public required IHttpClientFactory httpClientFactory { get; init; }

  [Inject]
  public required AppSettings AppSettings { get; init; }

  [Inject]
  public required DealerShipTokenCache DealerShipTokenCache { get; init; }

  private string selectedDealershipId = string.Empty;
  private string jwtEncryptedBase64EncodedPanel = string.Empty;
  private TokenTypeValues selectedTokenTypeValue = TokenTypeValues.DealershipToken;
  private string decryptedTokenPanel = string.Empty;

  public async Task OnSeedDataClick()
  {
    ValidateDealerIdAndReset(true);
    DealerShipTokenCache.DealerJWTTokenList.Clear();
    for (int i = 123; i <= 125; i++)
    {
      var delershipId = i.ToString();
      var newJWTToken = await GetNewJwtToken(delershipId, TokenTypeValues.DealershipToken);
      DealerShipTokenCache.UpsertJwtToken(delershipId, TokenTypeValues.DealershipToken, newJWTToken);
    }
  }

  public async Task OnGenerateTokenClick()
  {
    if (ValidateDealerIdAndReset() == false)
    {
      return;
    }

    var newJWTToken = await GetNewJwtToken(this.selectedDealershipId, this.selectedTokenTypeValue);

    if (string.IsNullOrEmpty(newJWTToken))
    {
      jwtEncryptedBase64EncodedPanel = "Failed to generate token.";
      return;
    }

    DealerShipTokenCache.UpsertJwtToken(selectedDealershipId, selectedTokenTypeValue, newJWTToken);

    jwtEncryptedBase64EncodedPanel = newJWTToken;
    
  }
  

  /// <summary>
  /// Validates that we have a DealerID, and if so, we reset the UX and process
  /// </summary>
  /// <returns></returns>
  public bool ValidateDealerIdAndReset(bool bypassForTesting = false)
  {
    if (string.IsNullOrEmpty(selectedDealershipId) && bypassForTesting == false)
    {
      decryptedTokenPanel = "No dealership id provided.";
      return false;
    }
    
    if (bypassForTesting == true)
    {
      this.selectedDealershipId = string.Empty;
      this.selectedTokenTypeValue = TokenTypeValues.Unknown;  
    }

    this.jwtEncryptedBase64EncodedPanel = string.Empty;
    this.decryptedTokenPanel = string.Empty;
    return true;
  }

  public void GetCachedTokenClick()
  {
    if (ValidateDealerIdAndReset()==false)
      return;

    jwtEncryptedBase64EncodedPanel = GetCachedJwtToken();
    
  }

  private string GetCachedJwtToken()
  {
    if (string.IsNullOrEmpty(selectedDealershipId))
    {
      return "No dealership id provided.";
    }

    if (selectedTokenTypeValue == TokenTypeValues.Unknown)
    {
      return "No token type provided.";
    }

    return DealerShipTokenCache.GetJwtToken(selectedDealershipId, selectedTokenTypeValue) ?? $"No token found in cache for Dealership Id:>{selectedDealershipId}< & TokenType:>{selectedTokenTypeValue}<";
  }

  private async Task<string> GetNewJwtToken(string? dealershipId = null, TokenTypeValues? tokenTypeValue = TokenTypeValues.Unknown)
  {
    var generateTokenHostUrl = APIRoutes.GetUrlPath(APIRoutes.DealershipChatBotAPIRoutes.GenerateTokenAPI);

    if (string.IsNullOrEmpty(dealershipId))
    {
      dealershipId = this.selectedDealershipId;
    }

    if (tokenTypeValue == TokenTypeValues.Unknown)
    {
      tokenTypeValue = selectedTokenTypeValue;
    }

    IDictionary<string, string?> queryParams = new Dictionary<string, string?>
        {
            { ClaimKeyValues.DealershipId.ToString(), dealershipId },
            { ClaimKeyValues.TokenType.ToString(), tokenTypeValue.ToString() }
        };
    var urlWithQueryString = QueryHelpers.AddQueryString(generateTokenHostUrl, queryParams);

    var dealershipChatBotHttpClient = httpClientFactory.CreatedNamedHttpClient(HttpNamedClients.DealershipChatBot);
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

    var endpoint = APIRoutes.GetUrlPath(APIRoutes.DealershipChatBotAPIRoutes.DecryptTokenAPI);
    IDictionary<string, string?> queryParams = new Dictionary<string, string?>
        {
            { "encryptedToken", jwtEncryptedBase64EncodedPanel }
        };
    var urlWithQueryString = QueryHelpers.AddQueryString(endpoint, queryParams);

    var httpDecryptJWTTokenClient = httpClientFactory.CreatedNamedHttpClient(HttpNamedClients.DealershipChatBot);
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
