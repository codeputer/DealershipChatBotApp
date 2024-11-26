using System.Net.Http;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DealerWebPageBlazorWebApp.Components;

[Authorize()]
public partial class WebChatComponent 
{
  private string? token;

  [Inject]
  public  required IHttpClientFactory httpClientFactory { get; set; }

  [Inject]
  public  required NavigationManager Navigation { get; set; }

  [Inject]
  public required IJSRuntime JS { get; set; }

  private HttpClient httpWebChatBotHttpClient => httpClientFactory.CreateClient("DealerWebPageBlazorWebApp.DealershipChatBot");

  protected override async Task OnInitializedAsync()
  {
    // Fetch the JWT token from your authentication service or API
    token = await GetJwtTokenAsync();

    if (!string.IsNullOrEmpty(token))
    {
      // Initialize the web chat with the token
      await InitializeWebChat(token);
    }
  }

  private async Task<string> GetJwtTokenAsync()
  {
    // Replace with your logic to get the JWT token
    var response = await httpWebChatBotHttpClient.GetAsync("api/GenerateToken?dealershipId=123&tokenType=webchat");
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsStringAsync();
  }

  private async Task InitializeWebChat(string token)
  {
    var host = Navigation.BaseUri;
    var botApiUrl = $"{host}api/messages";

    var styleOptions = new
    {
      botAvatarInitials = "BF",
      userAvatarInitials = "You",
      backgroundColor = "#F0F0F0",
      bubbleBackground = "#E0E0E0",
      bubbleFromUserBackground = "#D0D0D0",
      bubbleTextColor = "#000000",
      bubbleFromUserTextColor = "#000000",
      bubbleBorderRadius = 10,
      bubbleFromUserBorderRadius = 10,
      fontSize = "16px",
      primaryFont = "Arial, sans-serif"
    };

    var store = new
    {
      // Add any store configuration here
    };

    var webChatConfig = new
    {
      directLine = new
      {
        token = token,
        domain = botApiUrl
      },
      store,
      styleOptions
    };

    await JS.InvokeVoidAsync("BlazorWebChat.renderWebChat", webChatConfig);
  }
}
