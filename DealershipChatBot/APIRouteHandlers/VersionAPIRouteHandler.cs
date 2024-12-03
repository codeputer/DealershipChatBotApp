using DealerWebPageBlazorWebAppShared.APIEndpoints;

namespace DealershipChatBot.APIRouteHandlers;

public class VersionAPIRouteHandler : IRouteHandlerDelegate<IResult>
{
  public string RouteName => APIRoutes.DealershipChatBotAPIRoutes.Version.ToString();
  public string RoutePath => APIRoutes.GetUrlPath(APIRoutes.DealershipChatBotAPIRoutes.Version);
  public Delegate DelegateHandler => this.VersionDelegate;
  public HttpMethod HttpMethod => HttpMethod.Get;
  public bool ExcludeFromAPIDescription => false;
  public bool RequireAuthorization => false;

  public IResult VersionDelegate(HttpContext httpContext)
  {
    return Results.Ok("1.0");
  }
}
