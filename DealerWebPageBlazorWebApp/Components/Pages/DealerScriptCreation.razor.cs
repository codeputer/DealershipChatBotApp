using DealerWebPageBlazorWebAppShared.DTOModels;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.JSInterop;

namespace DealerWebPageBlazorWebApp.Components.Pages;

public partial class DealerScriptCreation : IDisposable
{
  public EventHandler? OnDealershipIdChanged;
  private IHttpClientFactory _HttpClientFactory;

  public DealerScriptCreation(IHttpClientFactory httpClientFactory)
  {
    _HttpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    SubscribeToDealershipIdChanged(true);
  }

  private string? selectedDealerId;
  private string? SelectedDealerId
  {
    get
    {
      return selectedDealerId;
    }
    set
    {
      selectedDealerId = value;
      OnDealershipIdChanged?.Invoke(this, EventArgs.Empty);
    }
  }
  private string GeneratedScript = string.Empty;

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Inject]
  public required AppSettings AppSettings { get; set; }

  [Inject]
  public required DealerShipTokenCache DealerShipTokenCache { get; init; }

  private string CopyButtonText = "Copy to Clipboard";
  private string CopyButtonStyle = "background-color: blue; color: white;";

  private string DealerJWTToken { get; set; } = "exampleDealerJWTToken";

  public async Task DealershipIdChanged(object? sender, EventArgs e)
  {
    await GenerateDealershipWebchatWindowScript();
  }

  private void SubscribeToDealershipIdChanged(bool subscribe)
  {
    if (subscribe)
    {
      OnDealershipIdChanged += async (sender,e) => await DealershipIdChanged(sender, e);  
    }
    else
    {
      OnDealershipIdChanged -= async (sender, e) =>await DealershipIdChanged(sender, e);
    }
  }

  private async Task GenerateDealershipWebchatWindowScript()
  {
    if (string.IsNullOrWhiteSpace(this.SelectedDealerId))
    {
      this.GeneratedScript = "Please select a dealer ID";
      return;
    }

    this.DealerJWTToken = DealerShipTokenCache.GetJwtToken(this.SelectedDealerId, TokenTypeValues.DealershipToken) ?? string.Empty;
    if (string.IsNullOrWhiteSpace(this.DealerJWTToken))
    {
      this.GeneratedScript = "Dealer JWT Token not found";
      return;
    }

    //setup the request headers
    var httpClient = _HttpClientFactory.CreatedNamedHttpClient(HttpNamedClients.DealershipChatBot);
    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.DealerJWTToken);

    //establish the path
    var webChatArtifactsAPIUri = APIRoutes.GetUrlPath(APIRoutes.DealershipChatBotAPIRoutes.GetWebChatArtifacts);
    CustomizedDealerFunctionDTO? customizedDealerFunctionDTO = null;
    try
    {
      customizedDealerFunctionDTO = await httpClient.GetFromJsonAsync<CustomizedDealerFunctionDTO>(webChatArtifactsAPIUri);
    }
    catch (Exception ex)
    {
      this.GeneratedScript = $"Failed to fetch the script: {ex.Message}";
      return;
    } 

    if (customizedDealerFunctionDTO is not null &&  string.IsNullOrWhiteSpace(customizedDealerFunctionDTO.CustomizedScript) == false)
    {
      this.GeneratedScript = customizedDealerFunctionDTO.DecodeFromBase64();
    }
    else
    {
      this.GeneratedScript = "Failed to fetch the script";
    }
  }
     
  private async Task CopyToClipboard()
  {
    await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", GeneratedScript);

    // Change the button style to indicate success
    CopyButtonText = "Copied!";
    CopyButtonStyle = "background-color: green; color: white;";
    StateHasChanged();

    // Wait for 2 seconds and reset the style
    await Task.Delay(2000);
    CopyButtonText = "Copy to Clipboard";
    CopyButtonStyle = "background-color: blue; color: white;";
    StateHasChanged();
  }

  public void Dispose()
  {
    SubscribeToDealershipIdChanged(false);
  }
}
