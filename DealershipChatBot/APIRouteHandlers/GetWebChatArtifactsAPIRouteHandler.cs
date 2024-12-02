using System;
using System.IdentityModel.Tokens.Jwt;

using DealerWebPageBlazorWebAppShared.DTOModels;

using Microsoft.Extensions.Primitives;

namespace DealershipChatBot.APIRouteHandlers;

public class GetWebChatArtifactsAPIRouteHandler : IRouteHandlerDelegate<IResult>
{
  private readonly AppSettings _appSettings;
    public GetWebChatArtifactsAPIRouteHandler(AppSettings appSettings)
  {
    _appSettings = appSettings;
  }

  public string RouteName => APIRoutes.DealershipChatBotAPIRoutes.GetWebChatArtifacts.ToString();
  public string RoutePath => APIRoutes.GetUrlPath(APIRoutes.DealershipChatBotAPIRoutes.GetWebChatArtifacts);
  public Delegate DelegateHandler => GetWebChatArtifactsDelegate;
  public HttpMethod? HttpMethod => HttpMethod.Get;
  public bool ExcludeFromAPIDescription => false;
  public bool RequireAuthorization => false;

  private IResult GetWebChatArtifactsDelegate(HttpContext httpContext,
                                             [FromServices] TokenHelper tokenHelper
                                             )
  {

    var request = httpContext.Request;

   // var clientIp = request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

    StringValues authHeader = string.Empty;
    // Step 1: Validate the Dealer Token
    if (request.Headers.TryGetValue("Authorization", out authHeader) == false|| string.IsNullOrWhiteSpace(authHeader))
    {
      return Results.Unauthorized();
    }

    if (string.IsNullOrWhiteSpace(authHeader))
      return Results.Unauthorized();

    authHeader = TokenHelper.RemoveBearerToken(authHeader!);
    var dealershipTokenClaims = tokenHelper.DecryptJWTTokenForClaimsPrincipal(authHeader!);  

    var dealershipId = string.Empty;
    if (dealershipTokenClaims is not null)
    {
      dealershipId = tokenHelper.GetClaimValue(dealershipTokenClaims, ClaimKeyValues.DealershipId);
    }

    if (string.IsNullOrWhiteSpace(dealershipId))
      return Results.Unauthorized();

    var templateFunction = _appSettings.DealershipChatBotConfiguration.ReadWebChatFunctionTemplate();
    if (string.IsNullOrWhiteSpace(templateFunction))
      return Results.InternalServerError("WebChat function template is missing");

    var getWebTokenUri = APIRoutes.GetAbsoluteUri(APIRoutes.DealershipChatBotAPIRoutes.GetWebToken, _appSettings.DealershipChatBotConfiguration.HostURL);
    var getWebChatMessageUri = APIRoutes.GetAbsoluteUri(APIRoutes.DealershipChatBotAPIRoutes.WebChatMessagesAPI, _appSettings.DealershipChatBotConfiguration.HostURL);

    ArgumentNullException.ThrowIfNull(getWebTokenUri);
    ArgumentNullException.ThrowIfNull(getWebChatMessageUri);

    // Use StringBuilder for efficient string replacement
    var tailoredTemplateFunction = new StringBuilder(templateFunction)
        .Replace("{dealerJWTToken}", authHeader)
        .Replace("{webTokenUrl}", getWebTokenUri.ToString())
        .Replace("{chatEndpointUrl}", getWebTokenUri.ToString())
        .ToString();

    return Results.Json(CustomizedDealerFunctionDTO.CustomizedDealerFunctionDTOFactory(tailoredTemplateFunction));
  }
}


