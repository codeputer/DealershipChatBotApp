//DealerShipChatBot/Program.cs
using DealerWebPageBlazorWebAppShared.Managers;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

//temp state persistence for holding issued DealershipChatBot JWT tokens
builder.Services.AddMemoryCache();

builder.Services.AddSingleton<IBotFrameworkHttpAdapter, CloudAdapter>();
builder.Services.AddSingleton<DealershipChatBot.AppSettings>();
builder.Services.AddSingleton<MinimalAPIRouteManager>();
builder.Services.AddSingleton<TokenHelper>();



builder.Services.AddTransient<IBot, DealershipBot>();
builder.Services.AddTransient<IRouteHandlerDelegate<IResult>, DealershipChatBot.APIRouteHandlers.VersionAPIRouteHandler>();
builder.Services.AddTransient<IRouteHandlerDelegate<IResult>, DealershipChatBot.APIRouteHandlers.WebchatMessagesAPIRouteHandler>();
builder.Services.AddTransient<IRouteHandlerDelegate<IResult>, DealershipChatBot.APIRouteHandlers.GenerateTokenAPIRouteHandler>();
builder.Services.AddTransient<IRouteHandlerDelegate<IResult>, DealershipChatBot.APIRouteHandlers.DecryptTokenAPIRouteHandler>();
builder.Services.AddTransient<IRouteHandlerDelegate<IResult>, DealershipChatBot.APIRouteHandlers.GetWebChatArtifactsAPIRouteHandler>();

builder.Services.AddTransient<DealerShipTokenCache>();

//get the configuration of the application
var appSettings = new DealershipChatBot.AppSettings(builder.Configuration);
var tokenHelper = new TokenHelper(appSettings);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
      var jwtCredentials = tokenHelper.GetTokenSigningCredentials();
      var encryptionCredentials = tokenHelper.GetEncryptingCredentials();

      options.TokenValidationParameters = tokenHelper.GetTokenValidationParameters();

      options.Events = new JwtBearerEvents
      {
        OnAuthenticationFailed = context =>
        {
          if (context.Exception is SecurityTokenExpiredException)
          {
            context.Response.Headers.Append("Token-Expired", "true");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            // Optionally log the failure or take custom action
            return context.Response.WriteAsync("Token has expired.");
          }
          context.Response.StatusCode = 401;
          context.Response.ContentType = "application/json";
          var result = System.Text.Json.JsonSerializer.Serialize(new { message = "Authentication failed" });
          return context.Response.WriteAsync(result);
        },
        OnChallenge = context =>
        {
          context.Response.StatusCode = 401;
          context.Response.ContentType = "application/json";
          var result = System.Text.Json.JsonSerializer.Serialize(new { message = "You are not authorized" });
          return context.Response.WriteAsync(result);
        }
      };
    });

#if DEBUG
builder.Services.AddHttpClient("DefaultClient", client =>
{
  client.Timeout = TimeSpan.FromMinutes(5); // Set the timeout to 5 minutes or any desired duration
});
#endif

//todo: authorization is policy based, where a policy has one or more claims
//builder.Services.AddAuthorizationBuilder()
//  .AddPolicy("provideWebChatToken", policy =>
//      policy.RequireClaim("provideWebChatToken"));


builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

var minimalAPIRouteManager = app.Services.GetRequiredService<MinimalAPIRouteManager>();
minimalAPIRouteManager.RegisterRoutes(app);

app.MapDefaultEndpoints();

var validateConfiguration = app.Services.GetRequiredService<DealershipChatBot.AppSettings>();

app.UseAuthentication();
app.UseAuthorization();

//app.MapPost("/refresh-token", async (HttpContext context) =>
//{
//  var refreshToken = context.Request.Headers["Refresh-Token"].ToString();

//  // Validate the refresh token (e.g., check against a database or in-memory store)
//  if (IsValidRefreshToken(refreshToken)) // Custom validation logic
//  {
//    // Generate a new JWT
//    var newJwt = GenerateNewJwtToken();
//    return Results.Ok(new { Token = newJwt });
//  }

//  return Results.Unauthorized();
//});


app.Run();


