using DealerWebPageBlazorWebAppShared.APIEndpoints;

using Microsoft.AspNetCore.Authorization;

namespace DealershipChatBot.APIRouteHandlers;

public class WebchatMessagesAPIRouteHandler : IRouteHandlerDelegate<IResult>
{
  private readonly ILogger<WebchatMessagesAPIRouteHandler> _logger;
  private readonly TokenHelper _tokenHelper;

  public WebchatMessagesAPIRouteHandler(ILogger<WebchatMessagesAPIRouteHandler> logger, TokenHelper tokenHelper)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _tokenHelper = tokenHelper ?? throw new ArgumentNullException(nameof(tokenHelper));
  }

  public string RouteName => APIRoutes.DealershipChatBotAPIRoutes.WebChatMessagesAPI.ToString();
  public string RoutePath => APIRoutes.GetUrlPath(APIRoutes.DealershipChatBotAPIRoutes.WebChatMessagesAPI);
  public Delegate DelegateHandler => GetWebChatMessages;
  public HttpMethod? HttpMethod => HttpMethod.Post;
  public bool ExcludeFromAPIDescription => false;
  public bool RequireAuthorization => false;

  [Authorize("WebChatTokenPolicy")]
  public async Task<IResult> GetWebChatMessages(HttpRequest request, [FromServices] IBotFrameworkHttpAdapter adapter, [FromServices] IBot bot)
  {
    _ = DealerWebPageBlazorWebAppShared.Policies.Policies.TokenTypePolicyValues.WebChatTokenPolicy;

    ArgumentNullException.ThrowIfNull(adapter, nameof(adapter));
    ArgumentNullException.ThrowIfNull(bot, nameof(bot));
    ArgumentNullException.ThrowIfNull(request?.HttpContext?.User?.Identity, nameof(request));
    _logger.LogDebug("Received WebChat Request");

    if (request.HttpContext.User.Identity.IsAuthenticated)
    {
      // Retrieve JWT Token from header
      var authorizationHeader = request.Headers["Authorization"].ToString();
      ClaimsPrincipal? claimsPrincipal = _tokenHelper.DecryptJWTTokenForClaimsPrincipal(authorizationHeader);
      if (claimsPrincipal is null || claimsPrincipal.Claims.Any() == false)
      {
        return Results.Unauthorized();
      }

      var dealershipId = _tokenHelper.GetClaimValue(claimsPrincipal, ClaimKeyValues.DealershipId);

      //todo: Use dealershipId to fetch dealership-specific data

      await adapter.ProcessAsync(request, request.HttpContext.Response, bot);
    }
    else
    {
      return Results.Unauthorized();
    }

    return Results.Ok();
  }
}
