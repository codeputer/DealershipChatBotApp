using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

using System.Net.Http;
using DealerWebPageBlazorWebAppShared.DTOModels;
using DealerWebPageBlazorWebAppShared.Resources;

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
  private string tokenType = TokenTypes.WebChatToken;
  private string decryptedTokenPanel = string.Empty;

  public async Task CreateNewToken()
  {
    decryptedTokenPanel = string.Empty;
    jwtEncryptedBase64EncodedPanel = string.Empty;

    var newJWTToken = await GetNewJwtToken();

    DealerShipTokenCache.SetToken(dealershipId, tokenType, newJWTToken);

    jwtEncryptedBase64EncodedPanel = newJWTToken;
    ShareStateForWebAssembly();
  }

  private void ShareStateForWebAssembly()
  {
    if (tokenType == TokenTypes.WebChatToken)
    {
      JWTTokensDTO.WebchatJwtToken = jwtEncryptedBase64EncodedPanel;
    }
    else if (tokenType == TokenTypes.DealershipToken)
    {
      JWTTokensDTO.DealerJwtToken = jwtEncryptedBase64EncodedPanel;
    }
  }

  public void UseCachedToken()
  {
    this.jwtEncryptedBase64EncodedPanel = string.Empty;
    this.decryptedTokenPanel = string.Empty;

    jwtEncryptedBase64EncodedPanel = GetCachedJwtToken();
    ShareStateForWebAssembly();
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
    var endpoint = "/api/GenerateToken";
    IDictionary<string, string?> queryParams = new Dictionary<string, string?>
        {
            { "dealershipId", dealershipId },
            { "tokenType", tokenType }
        };
    var urlWithQueryString = QueryHelpers.AddQueryString(endpoint, queryParams);

    var dealershipChatBotHttpClient = httpClientFactory.CreateClient("DealerWebPageBlazorWebApp.DealershipChatBot");
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
