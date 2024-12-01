

namespace DealerWebPageBlazorWebAppShared.Managers;

public class MinimalAPIRouteManager
{
  private readonly ConcurrentDictionary<string, IRouteHandlerDelegate<IResult>> _RouteHandlerDelegates = new();
  private readonly ILogger<MinimalAPIRouteManager> _logger;

  public MinimalAPIRouteManager(IEnumerable<IRouteHandlerDelegate<IResult>> pRouteHandlerDelegates, ILogger<MinimalAPIRouteManager> logger)
  {
    foreach (var pRouteHandlerDelegate in pRouteHandlerDelegates)
    {
      if (string.IsNullOrWhiteSpace(pRouteHandlerDelegate.RouteName))
      {
        throw new ArgumentException("RouteName is required", nameof(pRouteHandlerDelegate));
      }
      if (_RouteHandlerDelegates.ContainsKey(pRouteHandlerDelegate.RouteName))
      {
        throw new ArgumentException($"RouteName:>{pRouteHandlerDelegate.RouteName}< is already added", nameof(pRouteHandlerDelegate));
      }

      _RouteHandlerDelegates.AddOrUpdate(pRouteHandlerDelegate.RouteName, pRouteHandlerDelegate, (routeName, oldDelegate) => pRouteHandlerDelegate);
    }

    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public IReadOnlyDictionary<string, IRouteHandlerDelegate<IResult>> RouteHandlerDelegates => _RouteHandlerDelegates;

  public void RegisterRoutes(WebApplication pWebApplication)
  {
    foreach (var (routeName, routeHandlerDelegate) in _RouteHandlerDelegates)
    {
      string routePath = "/" + routeHandlerDelegate.RoutePath.TrimStart('/');
      _logger.LogDebug($"Injected Route:>{routePath}< using RouteName:>{routeName}< with Handler definition:>{routeHandlerDelegate.GetType().Name}< ExcludeFromAPIDescription set to:>{routeHandlerDelegate.ExcludeFromAPIDescription}<");

      RouteHandlerBuilder routeHandlerBuilder;
      if (routeHandlerDelegate.HttpMethod is null)
      {
        routeHandlerBuilder = pWebApplication.Map(routeHandlerDelegate.RoutePath, routeHandlerDelegate.DelegateHandler);
      }
      else
      {
        routeHandlerBuilder = pWebApplication.MapMethods(routePath, [routeHandlerDelegate.HttpMethod.ToString()], routeHandlerDelegate.DelegateHandler);
      }

      if (routeHandlerDelegate.ExcludeFromAPIDescription)
      {
        routeHandlerBuilder.ExcludeFromDescription();
      }
      else
      {
        routeHandlerBuilder.WithName(routeName);
      }

      if (routeHandlerDelegate.RequireAuthorization)
      {
        routeHandlerBuilder.RequireAuthorization();
      } 


    }

  }
}
