namespace DealerWebPageBlazorWebAppShared.APIEndpoints;
public static class APIRoutes
{
  public enum DealershipChatBotAPIRoutes
  {
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
      DealershipChatBotAPIRoutes.Version => string.Concat("/", DealershipChatBotAPIRoutes.Version),
      DealershipChatBotAPIRoutes.GetWebToken => string.Concat("/api/", DealershipChatBotAPIRoutes.GetWebToken),
      DealershipChatBotAPIRoutes.GenerateTokenAPI => string.Concat("/api/", DealershipChatBotAPIRoutes.GenerateTokenAPI),
      DealershipChatBotAPIRoutes.DecryptTokenAPI => string.Concat("/api/", DealershipChatBotAPIRoutes.DecryptTokenAPI),
      DealershipChatBotAPIRoutes.GetWebChatArtifacts => string.Concat("/api/", DealershipChatBotAPIRoutes.GetWebChatArtifacts),
      DealershipChatBotAPIRoutes.WebChatMessagesAPI => string.Concat("/api/", DealershipChatBotAPIRoutes.WebChatMessagesAPI),
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
      returningUri = new UriBuilder(hostUri.Scheme, hostUri.Host, port, pathValue: GetUrlPath(dealershipChatBotApiRoute));
    }
    return returningUri?.Uri;
  }
}
