
namespace DealershipChatBot.APIRouteHandlers;

public class DecryptTokenAPIRouteHandler : IRouteHandlerDelegate<IResult>
{
  public string RouteName => APIRoutes.DealershipChatBotAPIRoutes.DecryptToken.ToString();
  public string RoutePath => APIRoutes.GetUrlPath(APIRoutes.DealershipChatBotAPIRoutes.DecryptToken);
  public Delegate DelegateHandler => DecryptTokenDelegate;
  public HttpMethod? HttpMethod => HttpMethod.Get;
  public bool ExcludeFromAPIDescription => false;
  public bool RequireAuthorization => false;

  public IResult DecryptTokenDelegate([FromQuery] string encryptedToken, [FromServices] TokenHelper tokenHelper)
  {
    if (string.IsNullOrWhiteSpace(encryptedToken))
    {
      return Results.BadRequest("Token is required");
    }
    var asciiEncryptedToken = TokenHelper.Base64Decode(encryptedToken);
    var claimsPrincipal = tokenHelper.DecryptJWTTokenForClaimsPrincipal(asciiEncryptedToken);
    var claims = claimsPrincipal?.Claims.Select(c => new { c.Type, c.Value })?.ToList() ?? [];
    return Results.Ok(claims);
  }
}
