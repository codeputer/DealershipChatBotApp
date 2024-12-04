namespace DealershipChatBot.APIRouteHandlers;

public class EventGridValidationHandler : IRouteHandlerDelegate<IResult>
{
  public string RouteName => "OptionsRoot";

  public string RoutePath => "/";

  public Delegate DelegateHandler => this.EventGridValuationResponseDelegate;

  public HttpMethod HttpMethod => HttpMethod.Options;

  public bool ExcludeFromAPIDescription => true;

  public bool RequireAuthorization => false;

  public IResult EventGridValuationResponseDelegate(HttpContext httpContext)
  {
    //OPTIONS validation request is used to validate a cloud schema V1 web hook from the ACS portal when a web hook is subscribed.
    ArgumentNullException.ThrowIfNull(httpContext);

    if (
         httpContext.Request.Headers.TryGetValue("WebHook-Request-Origin", out var webhookRequestOrigin) &&
         httpContext.Request.Headers.TryGetValue("WebHook-Request-Callback", out var webhookRequestCallback)
       )
    {
      httpContext.Response.Headers.Append("WebHook-Allowed-Rate", "*");
      httpContext.Response.Headers.Append("WebHook-Allowed-Origin", webhookRequestOrigin);
      return Results.Ok();
    }

    return Results.BadRequest();
  }
}
