namespace DealershipChatBot.TokenMiddleware;

public static class JwtBearerOptionsExtensions
{
  public static void ConfigureCustomEvents(this JwtBearerOptions options, ILogger logger)
  {
    options.Events = new JwtBearerEvents
    {
      OnAuthenticationFailed = async context =>
      {
        logger.LogError("OnAuthenticationFailed:>{Exception}<", context.Exception.Message);
        if (context.Exception is SecurityTokenExpiredException)
        {
          context.Response.Headers.Append("Token-Expired", "true");
          context.Response.StatusCode = StatusCodes.Status401Unauthorized;
          await context.Response.WriteAsync("Token has expired.");
          return;
        }

        if (!context.Response.HasStarted)
        {
          context.Response.StatusCode = 401;
          context.Response.ContentType = "application/json";
          var result = System.Text.Json.JsonSerializer.Serialize(new { message = "Authentication failed" });
          await context.Response.WriteAsync(result);
        }
      },
      OnChallenge = async context =>
      {
        Console.WriteLine("OnChallenge: " + context.AuthenticateFailure?.Message);
        logger.LogError("OnChallenge:>{AuthenticationFailure}<", context.AuthenticateFailure?.Message);
        if (!context.Response.HasStarted)
        {
          context.Response.StatusCode = 401;
          context.Response.ContentType = "application/json";
          var result = System.Text.Json.JsonSerializer.Serialize(new { message = "You are not authorized" });
          await context.Response.WriteAsync(result);
        }
      }
    };
  }
}

