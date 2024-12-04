
namespace DealerWebPageBlazorWebAppShared.Interfaces;

public interface IRouteHandlerDelegate<TResult> where TResult : class, IResult
{

  /// <summary>
  /// Route Name is not significant from a routing perspective, but in the use of this interface
  /// it must be unique, and is used to identify the route handler in the API description
  /// </summary>
  public string RouteName { get; }

  public string RoutePath { get; }

  public Delegate DelegateHandler { get; }

  /// <summary>
  /// Null signifies that the route handler does not care about the HTTP method
  /// </summary>
  public HttpMethod? HttpMethod { get; }

  /// <summary>
  /// True signifies that the route handler should be excluded from the API description
  /// The default is to always include the route handler in the API description
  /// </summary>
  public bool ExcludeFromAPIDescription { get; }

  public bool RequireAuthorization { get; }
}
