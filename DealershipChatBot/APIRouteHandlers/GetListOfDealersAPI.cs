
namespace DealershipChatBot.APIRouteHandlers;

public class GetListOfDealersAPI : IRouteHandlerDelegate<IResult>
{
  public string RouteName => APIRoutes.DealershipChatBotAPIRoutes.GetListOfDealersAPI.ToString();
  public string RoutePath => APIRoutes.GetUrlPath(APIRoutes.DealershipChatBotAPIRoutes.GetListOfDealersAPI);
  public Delegate DelegateHandler => GetListOfDealersDelegate;
  public HttpMethod? HttpMethod => HttpMethod.Get;
  public bool ExcludeFromAPIDescription => false;
  public bool RequireAuthorization => false;
  public IResult GetListOfDealersDelegate([FromServices] DealerShipTokenCache dealerShipTokenCache)
  {
    ArgumentNullException.ThrowIfNull(dealerShipTokenCache, nameof(dealerShipTokenCache));
    if (dealerShipTokenCache.DealerJWTTokenList.Count == 0 )
    {
      return Results.Json(new List<string>() {"No Dealers Available"});
    }
    
    return Results.Json(dealerShipTokenCache.DealerJWTTokenList);
  }
}
