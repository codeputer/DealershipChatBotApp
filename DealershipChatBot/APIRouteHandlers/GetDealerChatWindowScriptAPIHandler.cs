﻿using System;
using System.IdentityModel.Tokens.Jwt;

using DealerWebPageBlazorWebAppShared.APIEndpoints;
using DealerWebPageBlazorWebAppShared.DTOModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Primitives;

namespace DealershipChatBot.APIRouteHandlers;

public class GetDealerChatWindowScriptAPIHandler : IRouteHandlerDelegate<IResult>
{
  private readonly AppSettings _appSettings;
    public GetDealerChatWindowScriptAPIHandler(AppSettings appSettings)
  {
    _appSettings = appSettings;
  }

  public string RouteName => APIRoutes.DealershipChatBotAPIRoutes.GetDealershipChatWindowScriptAPI.ToString();
  public string RoutePath => APIRoutes.GetUrlPath(APIRoutes.DealershipChatBotAPIRoutes.GetDealershipChatWindowScriptAPI);
  public Delegate DelegateHandler => GetWebChatArtifactsDelegate;
  public HttpMethod? HttpMethod => HttpMethod.Get;
  public bool ExcludeFromAPIDescription => false;
  public bool RequireAuthorization => true;

  //[Authorize("DealershipChatTokenPolicy")] todo: signin to MS Entra gives this claim
  private IResult GetWebChatArtifactsDelegate(HttpContext httpContext,
                                             [FromServices] TokenHelper tokenHelper
                                             )
  {
    _= DealerWebPageBlazorWebAppShared.Policies.Policies.TokenTypePolicyValues.DealershipChatTokenPolicy;
    
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

    var getWebTokenUri = APIRoutes.GetAbsoluteUri(APIRoutes.DealershipChatBotAPIRoutes.GetWebTokenAPI, _appSettings.DealershipChatBotConfiguration.HostURL);
    var getWebChatMessageUri = APIRoutes.GetAbsoluteUri(APIRoutes.DealershipChatBotAPIRoutes.WebChatMessagesAPI, _appSettings.DealershipChatBotConfiguration.HostURL);

    ArgumentNullException.ThrowIfNull(getWebTokenUri);
    ArgumentNullException.ThrowIfNull(getWebChatMessageUri);

    // Use StringBuilder for efficient string replacement
    var tailoredTemplateFunction = new StringBuilder(templateFunction)
        .Replace("{dealerJWTToken}", authHeader)
        .Replace("{webTokenUrl}", getWebTokenUri.ToString())
        .Replace("{chatEndpointUrl}", getWebChatMessageUri.ToString())
        .ToString();

    return Results.Json(CustomizedDealerFunctionDTO.CustomizedDealerFunctionDTOFactory(tailoredTemplateFunction));
  }
}


