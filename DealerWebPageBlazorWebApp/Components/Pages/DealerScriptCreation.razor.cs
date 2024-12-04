using System.Runtime.CompilerServices;

using DealerWebPageBlazorWebAppShared.APIEndpoints;
using DealerWebPageBlazorWebAppShared.DTOModels;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace DealerWebPageBlazorWebApp.Components.Pages;

public partial class DealerScriptCreation : IDisposable
{
  public EventHandler? OnDealershipIdChanged;
  private IHttpClientFactory _HttpClientFactory;
  public int RenderCount { get; private set; }
  private bool AllowRender = true;

  public void SetAllowRender(bool allowRender)
  {
    if (AllowRender != allowRender)
    {
      AllowRender = allowRender;
      logger.LogDebug("SetAllowRender. AllowRender: {AllowRender}", AllowRender.ToString());

      //a Render count of greater than zero means that we suppressed a render, and we have to now catchup
      if (AllowRender && RenderCount > 0)
      {
        logger.LogDebug("Render Count was:>{RenderCount}<. Asking for an async render please. Render Count will be reset",RenderCount);
        StateHasChanged();
        RenderCount = 0;
      }
    }
  }

  public DealerScriptCreation(IHttpClientFactory httpClientFactory)
  {
    _HttpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    SubscribeToDealershipIdChanged(true);
  }

  public void UpdateRenderCount()
  {
    RenderCount++;
    if (RenderCount > 0 && AllowRender == false)
      logger.LogWarning("Suppressing of Async rendering activated. Render Count:>{RenderCount}< Allow Rendering:>{AllowRendering}<", RenderCount, AllowRender.ToString());
    else
      logger.LogDebug("ShouldRender. Updated Render Count: {RenderCount}", RenderCount);

  }

  protected override bool ShouldRender()
  {
    UpdateRenderCount();
    return AllowRender;
  }

  protected async override Task OnAfterRenderAsync(bool firstRender)
  {
    if (AllowRender == false)
      logger.LogError("Rendering was unexpected! Allow Render:>{AllowRender}<", AllowRender.ToString());

    logger.LogDebug("OnAfterRenderingAsync Before \"await base.OnAfterRenderAsync\" First Render: >{FirstRender}<. RenderCount:>{RenderCount}<", firstRender.ToString(), RenderCount);
    await base.OnAfterRenderAsync(firstRender);

    RenderCount = 0;
    logger.LogDebug("OnAfterRenderingAsync After  \"await base.OnAfterRenderAsync\" First Render: >{FirstRender}<. RenderCount is reset!", firstRender.ToString() );

  }

  private string? selectedDealershipId;
  private string? SelectedDealerId
  {
    get
    {
      return selectedDealershipId;
    }
    set
    {
      selectedDealershipId = value;
      logger.LogDebug("SelectedDealerId Updated. Select Dealership Id: {SelectedDealerId} Render Count>{RenderCount}<", selectedDealershipId, RenderCount);
      OnDealershipIdChanged?.Invoke(this, EventArgs.Empty);
    }
  }
  public string GeneratedScript
  {
    get => generatedScript;
    set
    {
      logger.LogDebug("Before Generating Script Set To:{Contents} Render:{RenderingStatus} IsInteractive:{Interactive} Should Render Count:>{RenderCount}<", value[..5], RendererInfo.Name, RendererInfo.IsInteractive, RenderCount);
      generatedScript = value;
      logger.LogDebug("After Generating Script Set To:{Contents} Render:{RenderingStatus} IsInteractive:{Interactive} Should Render:>{RenderCount}<", value[..5], RendererInfo.Name, RendererInfo.IsInteractive, RenderCount);
    }

  }

  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Inject]
  public required AppSettings AppSettings { get; set; }

  [Inject]
  public required DealerShipTokenCache DealerShipTokenCache { get; init; }

  [Inject]
  public required ILogger<DealerScriptCreation> logger { get; init; }

  private string CopyButtonText = "Copy to Clipboard";
  private string CopyButtonStyle = "background-color: blue; color: white;";
  private string generatedScript = string.Empty;

  private string DealerJWTToken { get; set; } = "exampleDealerJWTToken";

  public async Task DealershipIdChanged(object? sender, EventArgs e)
  {
    try
    {
      SetAllowRender(false); //wait when the event is over
      logger.LogDebug("DealershipIdChanged Event: {SelectedDealerId} Render Count:>{RenderCount}<", SelectedDealerId, RenderCount);
      await GenerateDealershipWebchatWindowScript();
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error in DealershipIdChanged Event: {SelectedDealerId} Render Count:>{RenderCount}<", SelectedDealerId, RenderCount);
    }
    finally
    {
      SetAllowRender(true);
    }
  }

  protected override void OnInitialized()
  {
    logger.LogDebug("OnInitialized at Time:>{Time}< RenderInfo:>{Render}< Render Count:>{RenderCount}<", DateTime.Now, RendererInfo.Name, RenderCount);
  }

  private void SubscribeToDealershipIdChanged(bool subscribe)
  {
    if (subscribe)
    {
      OnDealershipIdChanged += async (sender, e) => await DealershipIdChanged(sender, e);
    }
    else
    {
      OnDealershipIdChanged -= async (sender, e) => await DealershipIdChanged(sender, e);
    }
  }

  private async Task GenerateDealershipWebchatWindowScript()
  {
    logger.LogDebug("GenerateDealershipWebchatWindowScript - Starting to Generate Dealership Webchat Window Script: {SelectedDealerId} Render Count:>{RenderCount}<", SelectedDealerId, RenderCount);
    if (string.IsNullOrWhiteSpace(this.SelectedDealerId))
    {
      this.GeneratedScript = "Please select a dealer ID";
      return;
    }

    this.DealerJWTToken = DealerShipTokenCache.GetJwtToken(this.SelectedDealerId, TokenTypeValues.DealershipToken) ?? string.Empty;
    if (string.IsNullOrWhiteSpace(this.DealerJWTToken))
    {
      this.GeneratedScript = "GenerateDealershipWebchatWindowScript - Dealer JWT Token not found";
      return;
    }

    logger.LogDebug("GenerateDealershipWebchatWindowScript - Creating Dealership GetWebChatArtifacts");
    //setup the request headers
    var httpClient = _HttpClientFactory.CreatedNamedHttpClient(HttpNamedClients.DealershipChatBot);
    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.DealerJWTToken);

    var webChatArtifactsAPIUri = APIRoutes.GetAbsoluteUri(APIRoutes.DealershipChatBotAPIRoutes.GetDealershipChatWindowScriptAPI, AppSettings.ChatbotServiceConfiguration.ChatbotServiceUrl);
    CustomizedDealerFunctionDTO? customizedDealerFunctionDTO;
    try
    {
      logger.LogDebug("GenerateDealershipWebchatWindowScript - Before Fetching Dealership Webchat Window Script: {SelectedDealerId} Render Count:>{RenderCount}<", SelectedDealerId, RenderCount);
      customizedDealerFunctionDTO = await httpClient.GetFromJsonAsync<CustomizedDealerFunctionDTO>(webChatArtifactsAPIUri);
      logger.LogDebug("GenerateDealershipWebchatWindowScript - After  Fetching Dealership Webchat Window Script: {SelectedDealerId} Render Count:>{RenderCount}<", SelectedDealerId, RenderCount);
    }
    catch (Exception ex)
    {
      this.GeneratedScript = $"Failed to fetch the script: {ex.Message}";
      return;
    }

    if (customizedDealerFunctionDTO is not null && string.IsNullOrWhiteSpace(customizedDealerFunctionDTO.CustomizedScript) == false)
    {
      this.GeneratedScript = customizedDealerFunctionDTO.DecodeFromBase64();
    }
    else
    {
      this.GeneratedScript = "Failed to fetch the script";
    }
    logger.LogDebug("Ending to Generate Dealership Webchat Window Script: {SelectedDealerId} Render Count:>{RenderCount}<", SelectedDealerId, RenderCount);
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
    GC.SuppressFinalize(this);
  }
}
