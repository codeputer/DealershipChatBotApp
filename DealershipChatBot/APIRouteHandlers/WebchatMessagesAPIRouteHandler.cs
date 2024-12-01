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

  public string RouteName => APIRoutes.DealershipChatBotAPIRoutes.WebChatMessages.ToString();
  public string RoutePath => APIRoutes.GetUrlPath(APIRoutes.DealershipChatBotAPIRoutes.WebChatMessages);
  public Delegate DelegateHandler => GetWebChatMessages;
  public HttpMethod? HttpMethod => HttpMethod.Post;
  public bool ExcludeFromAPIDescription => false;
  public bool RequireAuthorization => true;

  public async Task<IResult> GetWebChatMessages(HttpRequest request, [FromServices] IBotFrameworkHttpAdapter adapter, [FromServices] IBot bot)
  {
    ArgumentNullException.ThrowIfNull(adapter, nameof(adapter));
    ArgumentNullException.ThrowIfNull(bot, nameof(bot));
    ArgumentNullException.ThrowIfNull(request?.HttpContext?.User?.Identity, nameof(request));

    if (request.HttpContext.User.Identity.IsAuthenticated)
    {
      // Retrieve JWT Token from header
      var token = request.Headers["Authorization"].ToString().Replace("Bearer ", "");
      ClaimsPrincipal? claimsPrincipal = _tokenHelper.DecryptJWTTokenForClaimsPrincipal(token);
      if (claimsPrincipal is null || claimsPrincipal.Claims.Any() == false)
      {
        return Results.Unauthorized();
      }

      var dealershipId = _tokenHelper.GetClaimValue(claimsPrincipal, "dealershipId");

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
