using System.Net.Http;

using DealerWebPageBlazorWebAppShared.DTOModels;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace DealerWebPageBlazorWebAppClient.Pages;

public partial class WebChatComponent
{

  [Inject]
  public required ILogger<WebChatComponent> Logger { get; set; }


  [Inject]
  public required IJSRuntime JSRuntime { get; set; }

  [Inject]
  public required NavigationManager NavigationManager { get; set; }

  [CascadingParameter]
  public required JWTTokensDTO JWTTokensDTO
  {
    get
    {
      Logger.Log(LogLevel.Debug, "Getting JWTToken Dealer:>{DealerJwtToken}< WebChat:>{WebchatJwtToken}<", jWTTokensDTO?.DealerJwtToken ?? "Null", jWTTokensDTO?.WebchatJwtToken ?? "Null");
      if ( jWTTokensDTO is null)
        jWTTokensDTO =  new JWTTokensDTO() { DealerJwtToken = string.Empty, WebchatJwtToken = string.Empty };

      return jWTTokensDTO;
    }
    set
    {
      if (value is null)
      {
        System.Diagnostics.Debug.WriteLine("JWTTokensDTO is null in setter");
      }
      jWTTokensDTO = value;
      Logger.Log(LogLevel.Debug, "Setting JWTToken Dealer:>{DealerJwtToken}< WebChat:>{WebchatJwtToken}<", jWTTokensDTO?.DealerJwtToken ?? "Null", jWTTokensDTO?.WebchatJwtToken ?? "Null");
    }
  }

  private string? currentUri;

  private ErrorBoundary? errorBoundary;
  private string? errorMessage;
  private string? errorStackTrace;
  public required JWTTokensDTO jWTTokensDTO;

  private void HandleError(Exception exception)
  {
    errorMessage = exception.Message;
    errorStackTrace = exception.StackTrace;
  }

  protected override Task OnInitializedAsync()
  {
    currentUri = NavigationManager.Uri;
    return base.OnInitializedAsync();
  }


  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    if (firstRender)
    {
      Logger.Log(LogLevel.Information, $"WebChatComponent.OnAfterRenderAsync: firstRender: {firstRender}");

      try
      {
        if (JWTTokensDTO == null)
        {
          throw new NullReferenceException("JWTTokensDTO is null");
        }

        if (string.IsNullOrEmpty(JWTTokensDTO.WebchatJwtToken))
        {
          throw new NullReferenceException("WebchatJwtToken is null or empty");
        }

        if (string.IsNullOrEmpty(JWTTokensDTO.DealerJwtToken))
        {
          throw new NullReferenceException("DealerJwtToken is null or empty");
        }

        var config = new
        {
          directLine = new
          {
            token = JWTTokensDTO.WebchatJwtToken,
            domain = "https://directline.botframework.com/"
          },
          styleOptions = new { },
          store = new { }
        };

        await JSRuntime.InvokeVoidAsync("BlazorWebChat.renderWebChat", config);
      }
      catch (Exception ex)
      {
        Logger.LogError(ex, "An error occurred during OnAfterRenderAsync");
      }
    }
  }

  protected override bool ShouldRender()
  {
    return base.ShouldRender();
  }

  protected override void OnParametersSet()
  {
    if (JWTTokensDTO == null)
    {
      Logger.LogWarning("JWTTokensDTO is null in OnParametersSet");
    }
    base.OnParametersSet();
  }


}


