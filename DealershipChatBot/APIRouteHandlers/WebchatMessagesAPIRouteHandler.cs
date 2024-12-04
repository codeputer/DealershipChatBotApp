using System.Text.Json;

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
  public IResult GetWebChatMessages(HttpContext httpContext, ChatWindowDTO chatWindowDTO)
  {
    _ = DealerWebPageBlazorWebAppShared.Policies.Policies.TokenTypePolicyValues.WebChatTokenPolicy;
    if (chatWindowDTO is null)
    {
      return Results.BadRequest("Invalid Request");
    }
    var request = httpContext.Request;
    ArgumentNullException.ThrowIfNull(request?.HttpContext?.User?.Identity, nameof(request));
    _logger.LogDebug("Received WebChat Request");

    if (request.HttpContext.User.Identity.IsAuthenticated == false)
    {
      return Results.Unauthorized();
    }

    // Retrieve JWT Token from header
    var authorizationHeader = request.Headers["Authorization"].ToString();
    ClaimsPrincipal? claimsPrincipal = _tokenHelper.DecryptJWTTokenForClaimsPrincipal(authorizationHeader);
    if (claimsPrincipal is null || claimsPrincipal.Claims.Any() == false)
    {
      return Results.Unauthorized();
    }

    var dealershipId = _tokenHelper.GetClaimValue(claimsPrincipal, ClaimKeyValues.DealershipId);
    var dealerName = _tokenHelper.GetClaimValue(claimsPrincipal, ClaimKeyValues.DealerName);
    dealerName = dealershipId switch
    {
      "123" => "Dealer 123",
      "124" => "Dealer 124",
      "125" => "Dealer 125",
      _ => dealerName,
    };

    if (chatWindowDTO is null || chatWindowDTO.Questions is null)
    {
      return Results.BadRequest("Invalid Request");
    }

    if (chatWindowDTO.Questions.Count == 0 )
    {
      var startingQuestion = new Question
      {
        Asked = "Hello?",
        Answer = $"Hello from {dealerName ?? "???"}! How can I help you today?"
      };

      chatWindowDTO.Questions.Add(startingQuestion);
      return Results.Json(chatWindowDTO);
    }


    var firstQuestion = chatWindowDTO.Questions.First(); 
    if (firstQuestion.Asked.StartsWith("Hello") && string.IsNullOrEmpty(firstQuestion.Answer) )
    {
      firstQuestion.Asked = "Hello?";
      firstQuestion.Answer = $"Hello from {dealerName ?? "???"}! How can I help you today?";
      
      return Results.Json(chatWindowDTO);
    }


    var lastQuestion = chatWindowDTO.Questions.Last();
    if (lastQuestion.Answer is null)
    {
      lastQuestion.Asked = "Nothing was asked";
      lastQuestion.Answer = "Please ask a question that I can answer";
      return Results.Json(chatWindowDTO);
    }

    if (string.IsNullOrWhiteSpace(lastQuestion.Answer))
    {
      lastQuestion.Answer = $"After thinking about question for a long time. The answer is 42! :)";
      return Results.Json(chatWindowDTO);
    }

    return Results.Json(chatWindowDTO);
  }
}
