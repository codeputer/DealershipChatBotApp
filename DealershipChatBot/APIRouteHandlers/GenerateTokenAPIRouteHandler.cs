using DealerWebPageBlazorWebAppShared.APIEndpoints;
using DealerWebPageBlazorWebAppShared.Resources;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;

namespace DealershipChatBot.APIRouteHandlers;

public class GenerateTokenAPIRouteHandler (DealerShipTokenCache dealerShipTokenCache) : IRouteHandlerDelegate<IResult>
{
  private readonly DealerShipTokenCache _DealershipTokenCache = dealerShipTokenCache;
  public string RouteName => APIRoutes.DealershipChatBotAPIRoutes.GenerateTokenAPI.ToString();
  public string RoutePath => APIRoutes.GetUrlPath(APIRoutes.DealershipChatBotAPIRoutes.GenerateTokenAPI);
  public Delegate DelegateHandler => GenerateTokenDelegate;
  public HttpMethod? HttpMethod => HttpMethod.Get;
  public bool ExcludeFromAPIDescription => false;

  public bool RequireAuthorization => false;

  public IResult GenerateTokenDelegate([FromQuery] string dealershipId,
                                       [FromQuery] TokenTypeValues tokenType,
                                       [FromServices] TokenHelper tokenHelper,
                                       [FromServices] DealerShipTokenCache dealerShipTokenCache,
                                       string clientIPAddress = "")
  {
    _ = DealerWebPageBlazorWebAppShared.Policies.Policies.TokenTypePolicyValues.DealershipChatTokenPolicy;


    if (string.IsNullOrWhiteSpace(dealershipId))
    {
      return Results.BadRequest("DealershipId is required");
    }

    if (TokenTypeValues.Unknown == tokenType)
    {
      return Results.BadRequest("TokenType is required or is unknown");
    }

    var jwtToken = dealerShipTokenCache.GetJwtToken(dealershipId, tokenType);
    
    //todo: get dealer name from dealershipid by running query to db? 
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

    //simulates persistence
    _DealershipTokenCache.UpsertJwtToken(dealershipId, tokenType, encryptedJWTToken);

    return Results.Json(new JWTTokenDTO() { JWTToken = encryptedJWTToken });
  }
}
