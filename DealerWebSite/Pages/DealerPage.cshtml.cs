using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DealerWebSite.Pages;

using System.Data.SqlTypes;

using DealerWebPageBlazorWebAppShared.Configuration;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class DealerPageModel : PageModel
{
  [Inject]
  public AppSettings AppSettings { get; set; } = default!;

  public string DealerName { get; private set; } = default!;
  public string EncryptedJwt { get; private set; } = default!;

  public string WebChatDealerRequest { get; private set; } = default!;

  public string WebchatMessagesAPI { get; private set; } = default!;

  public void OnGet()
  {
    // Set dealer-specific values (these can come from configuration, database, or external services)
    DealerName = "Dealer ABC"; // Example: Replace with dynamic dealer name
    EncryptedJwt = "eyJhbGciOiJIUzI1NiIsIn..."; // Replace with the dealer's encrypted JWT

    if (Uri.TryCreate(AppSettings.ChatbotServiceConfiguration.ChatbotServiceUrl, UriKind.Absolute, out Uri? hostUrl))
    {
      var port = hostUrl.Port > 0 ? hostUrl.Port : 80;
      WebChatDealerRequest = new UriBuilder(hostUrl.Scheme, hostUrl.Host, port, pathValue: "api/GetWebChatArtifacts").Uri.ToString();
      WebchatMessagesAPI = new UriBuilder(hostUrl.Scheme, hostUrl.Host, port, pathValue: "/api/webchatmessages").Uri.ToString();
    }
    else
      ArgumentNullException.ThrowIfNull(hostUrl);

  }
}

