
namespace DealershipChatBot.APIRouteHandlers;

public class GetDealerChatWindowScriptForDemo : IRouteHandlerDelegate<IResult>
{
  private readonly AppSettings _appSettings;
  public GetDealerChatWindowScriptForDemo(AppSettings appSettings)
  {
    _appSettings = appSettings;
  }

  public string RouteName => APIRoutes.DealershipChatBotAPIRoutes.GetDealershipChatWindowScriptForDemoAPI.ToString();
  public string RoutePath => APIRoutes.GetUrlPath(APIRoutes.DealershipChatBotAPIRoutes.GetDealershipChatWindowScriptForDemoAPI);
  public Delegate DelegateHandler => GetDealershipChatWindowScriptForDemoAPIDelegate;
  public HttpMethod? HttpMethod => HttpMethod.Get;
  public bool ExcludeFromAPIDescription => false;
  public bool RequireAuthorization => false;

  private IResult GetDealershipChatWindowScriptForDemoAPIDelegate([FromQuery] string dealerShipId,
                                                                   [FromServices] TokenHelper tokenHelper,
                                                                   [FromServices] DealerShipTokenCache dealerShipTokenCache
                                                                   )
  {

    if (string.IsNullOrWhiteSpace(dealerShipId))
      return Results.Unauthorized();

    var dealerShipJwtToken = dealerShipTokenCache.GetJwtToken(dealerShipId, TokenTypeValues.DealershipToken);
    if (string.IsNullOrWhiteSpace(dealerShipJwtToken))
    {
      throw new Exception($"Dealership Id: {dealerShipId} needs to be created");
    }

    var templateFunction = _appSettings.DealershipChatBotConfiguration.ReadWebChatFunctionTemplate();
    if (string.IsNullOrWhiteSpace(templateFunction))
      return Results.InternalServerError("WebChat function template is missing");

    var getWebTokenUri = APIRoutes.GetAbsoluteUri(APIRoutes.DealershipChatBotAPIRoutes.GetWebTokenAPI, _appSettings.DealershipChatBotConfiguration.HostURL);
    var getWebChatMessageUri = APIRoutes.GetAbsoluteUri(APIRoutes.DealershipChatBotAPIRoutes.WebChatMessagesAPI, _appSettings.DealershipChatBotConfiguration.HostURL);

    ArgumentNullException.ThrowIfNull(getWebTokenUri);
    ArgumentNullException.ThrowIfNull(getWebChatMessageUri);

    // Use StringBuilder for efficient string replacement
    var tailoredTemplateFunction = new StringBuilder(templateFunction)
        .Replace("{dealerJWTToken}", dealerShipJwtToken)
        .Replace("{webTokenUrl}", getWebTokenUri.ToString())
        .Replace("{chatEndpointUrl}", getWebChatMessageUri.ToString())
        .ToString();

    return Results.Json(CustomizedDealerFunctionDTO.CustomizedDealerFunctionDTOFactory(tailoredTemplateFunction));
  }
}


