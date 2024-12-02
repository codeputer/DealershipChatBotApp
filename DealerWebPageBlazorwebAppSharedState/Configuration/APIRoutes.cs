namespace DealerWebPageBlazorWebAppShared.Configuration;
public static class APIRoutes
{
  public enum DealershipChatBotAPIRoutes  {
    Unknown, //always first to have a good default value
    Version,
    GenerateTokenAPI,
    DecryptTokenAPI,
    GetWebToken, 
    GetWebChatArtifacts,
    WebChatMessagesAPI
  }

  public static string GetUrlPath(DealershipChatBotAPIRoutes dealershipChatBotApiRoute)
  {
    var routeURL = dealershipChatBotApiRoute switch
    {
      APIRoutes.DealershipChatBotAPIRoutes.Version => string.Concat("/", APIRoutes.DealershipChatBotAPIRoutes.Version),
      DealershipChatBotAPIRoutes.GetWebToken => string.Concat("/api/", APIRoutes.DealershipChatBotAPIRoutes.GetWebToken),
      APIRoutes.DealershipChatBotAPIRoutes.GenerateTokenAPI => string.Concat("/api/", APIRoutes.DealershipChatBotAPIRoutes.GenerateTokenAPI),
      APIRoutes.DealershipChatBotAPIRoutes.DecryptTokenAPI => string.Concat("/api/", APIRoutes.DealershipChatBotAPIRoutes.DecryptTokenAPI),
      APIRoutes.DealershipChatBotAPIRoutes.GetWebChatArtifacts => string.Concat("/api/", APIRoutes.DealershipChatBotAPIRoutes.GetWebChatArtifacts),
      APIRoutes.DealershipChatBotAPIRoutes.WebChatMessagesAPI => string.Concat("/api/", APIRoutes.DealershipChatBotAPIRoutes.WebChatMessagesAPI),
      _ => "N/A",
    };

    if (routeURL == "N/A")
      throw new ArgumentNullException($"Route:>{dealershipChatBotApiRoute} is not defined<");

    return routeURL;
  }

  public static Uri? GetAbsoluteUri(DealershipChatBotAPIRoutes dealershipChatBotApiRoute, string hostUriConfiguration)
  {
    UriBuilder? returningUri = null;
    if (Uri.TryCreate(hostUriConfiguration, UriKind.Absolute, out Uri? hostUri))
    {
      var port = hostUri.Port > 0 ? hostUri.Port : 80;
      returningUri = new UriBuilder(hostUri.Scheme, hostUri.Host, port, pathValue: APIRoutes.GetUrlPath(dealershipChatBotApiRoute));
    }
    return returningUri?.Uri;
  }
}
