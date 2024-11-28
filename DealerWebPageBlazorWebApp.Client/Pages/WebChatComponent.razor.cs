using System.Net.Http;

using DealerWebPageBlazorWebAppShared.DTOModels;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DealerWebPageBlazorWebAppClient.Pages;

public partial class WebChatComponent
{
  
  [CascadingParameter]
  public required JWTTokensDTO JWTTokensDTO { get; set; }

  protected override Task OnInitializedAsync()
  {
    return base.OnInitializedAsync();
  }
}
