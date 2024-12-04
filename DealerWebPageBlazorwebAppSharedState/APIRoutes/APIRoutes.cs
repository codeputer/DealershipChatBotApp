namespace DealerWebPageBlazorWebAppShared.APIEndpoints;
public static class APIRoutes
{
  public enum DealershipChatBotAPIRoutes
  {
    Unknown, //always first to have a good default value
    VersionAPI,
    GenerateTokenAPI,
    DecryptTokenAPI,
    GetWebTokenAPI,
    GetDealershipChatWindowScriptAPI,
    GetDealershipChatWindowScriptForDemoAPI,
    WebChatMessagesAPI,
    GetListOfDealersAPI
  }

  /// <summary>
  /// List of routing endpoints used throughout this application
  /// </summary>
  /// <param name="dealershipChatBotApiRoute"></param>
  /// <returns></returns>
  /// <exception cref="ArgumentNullException"></exception>
  public static string GetUrlPath(DealershipChatBotAPIRoutes dealershipChatBotApiRoute)
  {
    var routeURL = dealershipChatBotApiRoute switch
    {
      DealershipChatBotAPIRoutes.VersionAPI => string.Concat("/", DealershipChatBotAPIRoutes.VersionAPI),
      DealershipChatBotAPIRoutes.GetWebTokenAPI => string.Concat("/api/", DealershipChatBotAPIRoutes.GetWebTokenAPI),
      DealershipChatBotAPIRoutes.GenerateTokenAPI => string.Concat("/api/", DealershipChatBotAPIRoutes.GenerateTokenAPI),
      DealershipChatBotAPIRoutes.DecryptTokenAPI => string.Concat("/api/", DealershipChatBotAPIRoutes.DecryptTokenAPI),
      DealershipChatBotAPIRoutes.GetDealershipChatWindowScriptAPI => string.Concat("/api/", DealershipChatBotAPIRoutes.GetDealershipChatWindowScriptAPI),
      DealershipChatBotAPIRoutes.GetDealershipChatWindowScriptForDemoAPI => string.Concat("/api/", DealershipChatBotAPIRoutes.GetDealershipChatWindowScriptForDemoAPI),
      DealershipChatBotAPIRoutes.WebChatMessagesAPI => string.Concat("/api/", DealershipChatBotAPIRoutes.WebChatMessagesAPI),
      DealershipChatBotAPIRoutes.GetListOfDealersAPI => string.Concat("/api/", DealershipChatBotAPIRoutes.GetListOfDealersAPI),
      _ => "N/A",
    };

    if (routeURL == "N/A")
      throw new ArgumentNullException($"Route:>{dealershipChatBotApiRoute} is not defined<");

    return routeURL;
  }

  /// <summary>
  /// Builds route endpoint, or throws an exception if it fails
  /// </summary>
  /// <param name="dealershipChatBotApiRoute"></param>
  /// <param name="hostUriConfiguration"></param>
  /// <returns></returns>
  /// <exception cref="Exception"></exception>
  public static Uri GetAbsoluteUri(DealershipChatBotAPIRoutes dealershipChatBotApiRoute, string hostUriConfiguration)
  {
    UriBuilder? returningUri = null;
    if (Uri.TryCreate(hostUriConfiguration, UriKind.Absolute, out Uri? hostUri) == false)
    {
      throw new Exception($"Unable to create URL route for:>{dealershipChatBotApiRoute}< and the host domain:>{hostUriConfiguration}<");
    }

    var port = hostUri.Port > 0 ? hostUri.Port : 80;
    returningUri = new UriBuilder(hostUri.Scheme, hostUri.Host, port, pathValue: GetUrlPath(dealershipChatBotApiRoute));

    return returningUri?.Uri ?? throw new Exception($"Url could not be created.");
  }
}
