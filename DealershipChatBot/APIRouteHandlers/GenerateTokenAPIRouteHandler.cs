using DealerWebPageBlazorWebAppShared.Resources;

using Microsoft.Extensions.Caching.Memory;

namespace DealershipChatBot.APIRouteHandlers;

public class GenerateTokenAPIRouteHandler : IRouteHandlerDelegate<IResult>
{
  public string RouteName => APIRoutes.DealershipChatBotAPIRoutes.GenerateToken.ToString();
  public string RoutePath => APIRoutes.GetUrlPath(APIRoutes.DealershipChatBotAPIRoutes.GenerateToken);
  public Delegate DelegateHandler => GenerateTokenDelegate;
  public HttpMethod? HttpMethod => HttpMethod.Get;
  public bool ExcludeFromAPIDescription => false;

  public bool RequireAuthorization => false;

  public IResult GenerateTokenDelegate([FromQuery] string dealershipId,
                                       [FromQuery] TokenTypeValues tokenType,
                                       [FromServices] TokenHelper tokenHelper,
                                       [FromServices] IMemoryCache memoryCache,
                                       string clientIPAddress = "")
  {
    if (string.IsNullOrWhiteSpace(dealershipId))
    {
      return Results.BadRequest("DealershipId is required");
    }

    //if we have it already in cache, return it!
    if (memoryCache.TryGetValue(dealershipId, out var token))
    {
      return Results.Ok(token);
    }

    if (TokenTypeValues.Unknown == tokenType)
    {
      return Results.BadRequest("TokenType is required or is unknown");
    }

    //todo: get dealer name from dealership name
    var claims = tokenHelper.GenerateClaims(tokenType,dealershipId, "DealerName", clientIPAddress);

    var jwtSignedToken = tokenHelper.GenerateJWTToken(claims);
    var encryptedJWTToken = tokenHelper.EncryptJwtToken(jwtSignedToken);

    //validation that we can actually decrypt this token
    try
    {
      var decryptedToken = tokenHelper.DecryptJWTTokenForClaimsPrincipal(encryptedJWTToken);
      if (decryptedToken is not null)
      {
        if (decryptedToken.Claims.Any() == false)
          throw new Exception("NO Claims in Decrypted token");
      }
    }
    catch (Exception ex)
    {
      throw new Exception("WebChatToken could not be created", ex);
    }


    var base64JWTEncryptedToken = TokenHelper.Base64Encode(encryptedJWTToken);

    //simulates persistence
    memoryCache.Set(dealershipId, base64JWTEncryptedToken);

    return Results.Ok(base64JWTEncryptedToken);
  }
}
