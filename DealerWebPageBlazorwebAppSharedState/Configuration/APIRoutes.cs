namespace DealerWebPageBlazorWebAppShared.Configuration;
public static class APIRoutes
{
  public enum DealershipChatBotAPIRoutes  {
    Unknown, //always first to have a good default value
    Version,
    GenerateToken,
    DecryptToken,
    GetWebTokenAPI,
    GetWebChatArtifacts,
    WebChatMessages
  }

  public static string GetUrlPath(DealershipChatBotAPIRoutes dealershipChatBotApiRoute)
  {
    var routeURL = dealershipChatBotApiRoute switch
    {
      APIRoutes.DealershipChatBotAPIRoutes.Version => string.Concat("/", APIRoutes.DealershipChatBotAPIRoutes.Version),
      APIRoutes.DealershipChatBotAPIRoutes.GenerateToken => string.Concat("/api/", APIRoutes.DealershipChatBotAPIRoutes.GenerateToken),
      APIRoutes.DealershipChatBotAPIRoutes.DecryptToken => string.Concat("/api/", APIRoutes.DealershipChatBotAPIRoutes.DecryptToken),
      APIRoutes.DealershipChatBotAPIRoutes.GetWebTokenAPI => string.Concat("/api/", APIRoutes.DealershipChatBotAPIRoutes.GetWebTokenAPI),
      APIRoutes.DealershipChatBotAPIRoutes.GetWebChatArtifacts => string.Concat("/api/", APIRoutes.DealershipChatBotAPIRoutes.GetWebChatArtifacts),
      APIRoutes.DealershipChatBotAPIRoutes.WebChatMessages => string.Concat("/api/", APIRoutes.DealershipChatBotAPIRoutes.WebChatMessages),
      _ => "N/A",
    };

    if (routeURL == "N/A")
      throw new ArgumentNullException($"Route:>{dealershipChatBotApiRoute} is not defined<");

    return routeURL;
  }
}
