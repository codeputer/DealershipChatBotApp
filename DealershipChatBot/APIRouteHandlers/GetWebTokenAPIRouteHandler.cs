namespace DealershipChatBot.APIRouteHandlers;

public class GetWebTokenAPIRouteHandler(AppSettings appSettings) : IRouteHandlerDelegate<IResult>
{
  private readonly AppSettings _appSettings = appSettings;

  public string RouteName => APIRoutes.DealershipChatBotAPIRoutes.GetWebToken.ToString();
  public string RoutePath => APIRoutes.GetUrlPath(APIRoutes.DealershipChatBotAPIRoutes.GetWebToken);
  public Delegate DelegateHandler => GetWebTokenUsingDealerJWT;
  public HttpMethod? HttpMethod => HttpMethod.Get;
  public bool ExcludeFromAPIDescription => false;
  public bool RequireAuthorization => false;
  public IResult GetWebTokenUsingDealerJWT(HttpContext httpContext,
                                             [FromServices] TokenHelper tokenHelper
                                             )
  {

    var request = httpContext.Request;

    var clientIp = request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

    // Step 1: Validate the Dealer Token
    if (!request.Headers.TryGetValue("Authorization", out var authHeader) || string.IsNullOrWhiteSpace(authHeader))
    {
      return Results.Unauthorized();
    }

    if (string.IsNullOrWhiteSpace(authHeader))
      return Results.Unauthorized();

    var decryptedDealerToken = tokenHelper.DecryptJWTTokenForClaimsPrincipal(authHeader!);

    var dealershipId = string.Empty;
    if (decryptedDealerToken is not null)
    {
      dealershipId = tokenHelper.GetClaimValue(decryptedDealerToken, ClaimKeyValues.DealershipId);
    }

    if (string.IsNullOrWhiteSpace(dealershipId))
      return Results.Unauthorized();

    var claims = tokenHelper.GenerateClaims(TokenTypeValues.WebChatToken, dealershipId, "DealerName", clientIp);
    var webchatToken = tokenHelper.GenerateJWTToken(claims);

    if (string.IsNullOrWhiteSpace(webchatToken))
      return Results.Empty;

    return Results.Ok(webchatToken);
  }
}


