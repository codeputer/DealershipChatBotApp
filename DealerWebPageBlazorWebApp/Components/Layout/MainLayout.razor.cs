using DealerWebPageBlazorWebAppShared.DTOModels;

using Microsoft.AspNetCore.Components;

namespace DealerWebPageBlazorWebApp.Components.Layout;

public partial class MainLayout
{
  [Inject]
  public required JWTTokensDTO JWTTokensDTO { get; set; }

  protected override Task OnInitializedAsync()
  {
    this.JWTTokensDTO.DealerJwtToken = "DealerJwtToken";
    this.JWTTokensDTO.WebchatJwtToken = "WebchatJwtToken";

    return base.OnInitializedAsync();
  }
}
