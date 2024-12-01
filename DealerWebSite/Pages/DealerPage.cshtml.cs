using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DealerWebSite.Pages;

using System.Data.SqlTypes;

using DealerWebPageBlazorWebAppShared.Configuration;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.RazorPages;

public partial class DealerPageModel : PageModel
{
  [Inject]
  public AppSettings AppSettings { get; set; } = default!;

  [Inject]
  public IHttpClientFactory HttpClientFactory { get; set; } = default!;

  public string DealerName { get; private set; } = "Default Dealer Name";
  public string DealerId { get; private set; } = "Default Dealer ID"; 
  public string EncryptedJwt { get; private set; } = default!;

  public string WebchatDealerRequest { get; private set; } = default!;

  public string WebchatMessagesAPI { get; private set; } = default!;

  public void OnGet()
  {

    var httpClient = HttpClientFactory.CreateClient(HttpNamedClients.DealershipChatBot.ToString());


    // Set dealer-specific values (these can come from configuration, database, or external services)
    DealerName = "Dealer ABC"; // Example: Replace with dynamic dealer name
    DealerId = "123"; // Example: Replace with dynamic dealer ID
    EncryptedJwt = "eyJhbGciOiJIUzI1NiIsIn..."; // Replace with the dealer's encrypted JWT

    if (Uri.TryCreate(AppSettings.ChatbotServiceConfiguration.ChatbotServiceUrl, UriKind.Absolute, out Uri? hostUrl))
    {
      var port = hostUrl.Port > 0 ? hostUrl.Port : 80;
      WebchatDealerRequest = new UriBuilder(hostUrl.Scheme, hostUrl.Host, port, pathValue: APIRoutes.GetUrlPath(APIRoutes.DealershipChatBotAPIRoutes.GetWebChatArtifacts)).Uri.ToString();
      WebchatMessagesAPI = new UriBuilder(hostUrl.Scheme, hostUrl.Host, port, pathValue:APIRoutes.GetUrlPath(APIRoutes.DealershipChatBotAPIRoutes.WebChatMessages)).Uri.ToString();
    }
    else
      ArgumentNullException.ThrowIfNull(hostUrl);

  }
}

