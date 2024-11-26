using DealerWebPageBlazorWebApp.Components.Resources;

using Microsoft.AspNetCore.Components;

using System.Net.Http;

namespace DealerWebPageBlazorWebApp.Components.Pages;

public partial class JwtTokenManagement
{
  [Inject]
  public required IHttpClientFactory httpClientFactory { get; init; }

  [Inject]
  public required AppSettings AppSettings { get; init; }

  [Inject]
  public required DealerShipTokenCache DealerShipTokenCache { get; init; }

  private string dealershipId = string.Empty;
  private string jwtEncryptedBase64EncodedPanel = string.Empty;
  private string tokenType = TokenTypes.WebChatToken;
  private string decryptedTokenPanel = string.Empty;

  public async Task CreateNewToken()
  {
    decryptedTokenPanel = string.Empty;
    jwtEncryptedBase64EncodedPanel = string.Empty;

    var newJWTToken = await GetNewJwtToken();

    DealerShipTokenCache.SetToken(dealershipId, tokenType, newJWTToken);

    jwtEncryptedBase64EncodedPanel = newJWTToken;
  }

  public void UseCachedToken()
  {
    this.jwtEncryptedBase64EncodedPanel = string.Empty;
    this.decryptedTokenPanel = string.Empty;

    jwtEncryptedBase64EncodedPanel = GetCachedJwtToken();
  }

  private string GetCachedJwtToken()
  {
    if (string.IsNullOrEmpty(dealershipId))
    {
      return "No dealership id provided.";
    } 

    if (string.IsNullOrEmpty(tokenType))
    {
      return "No token type provided.";
    }

    return DealerShipTokenCache.GetToken(dealershipId, tokenType) ?? $"No token found in cache for Dealership Id:>{dealershipId}< & TokenType:>{tokenType}<";
  }

  private async Task<string> GetNewJwtToken()
  {
    var uriBuilder = new UriBuilder($"{AppSettings.ChatbotServiceConfiguration.ChatbotServiceUrl}/api/GenerateToken");
    var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
    query["dealershipId"] = dealershipId;
    query["tokenType"] = tokenType;
    uriBuilder.Query = query.ToString();
    var url = uriBuilder.ToString();

    var httpClient = httpClientFactory.CreateClient();
    var response = await httpClient.GetAsync(url);
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

    var uriBuilder = new UriBuilder($"{AppSettings.ChatbotServiceConfiguration.ChatbotServiceUrl}/api/DecryptToken");
    var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
    query["encryptedToken"] = jwtEncryptedBase64EncodedPanel;
    uriBuilder.Query = query.ToString();
    var url = uriBuilder.ToString();

    var httpDecryptJWTTokenClient = httpClientFactory.CreateClient();
    var response = await httpDecryptJWTTokenClient.GetAsync(url);
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
