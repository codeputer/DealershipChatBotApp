using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DealershipChatBot;
using Microsoft.AspNetCore.Mvc;

//DealerShipChatBot/Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSingleton<IBotFrameworkHttpAdapter, CloudAdapter>();
builder.Services.AddSingleton<AppSettings>();

builder.Services.AddTransient<IBot, DealershipBot>();

builder.Services.AddScoped<TokenHelper>();

//get the configuration of the application
var appSettings = new AppSettings(builder.Configuration);
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

//builder.Services.AddAuthorizationBuilder()
//  .AddPolicy("provideWebChatToken", policy =>
//      policy.RequireClaim("provideWebChatToken"));

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

app.MapDefaultEndpoints();

var validateConfiguration = app.Services.GetRequiredService<AppSettings>();

app.UseAuthentication();
app.UseAuthorization();


app.MapGet("/Version", () => "1.0");

app.MapGet("/api/GenerateToken", ([FromQuery] string dealershipId, [FromQuery] string tokenType, [FromServices] TokenHelper tokenHelper) =>
{
  if (string.IsNullOrWhiteSpace(dealershipId))
  {
    return Results.BadRequest("DealershipId is required");
  }

  if (string.IsNullOrWhiteSpace(tokenType))
  {
    return Results.BadRequest("TokenType is required");
  }

  var jwtSignedToken = tokenHelper.GenerateJWTToken(dealershipId, tokenType);

  var encyptedJWTToken = tokenHelper.EncryptJwtToken(jwtSignedToken);

  var decryptedToken = tokenHelper.DecryptJWTTokenForClaimsPrincipal(encyptedJWTToken);

  if (decryptedToken is null)
  {
    return Results.BadRequest("Token could not be decrypted");
  }

  //base 64 encode for transport
  var base64JWTEncryptedToken = TokenHelper.Base64Encode(encyptedJWTToken);

  return Results.Ok(base64JWTEncryptedToken);
});

app.MapGet("/api/DecryptToken", ([FromQuery] string encryptedToken, [FromServices] TokenHelper tokenHelper) =>
{
  if (string.IsNullOrWhiteSpace(encryptedToken))
  {
    return Results.BadRequest("Token is required");
  }

  var asciiEncryptedToken = TokenHelper.Base64Decode(encryptedToken);

  var claimsPrincipal = tokenHelper.DecryptJWTTokenForClaimsPrincipal(asciiEncryptedToken);
  var claims = claimsPrincipal?.Claims.Select(c => new { c.Type, c.Value })?.ToList() ?? [];

  return Results.Ok(claims);

});

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


//Protected API, must have a WebChatToken, issued by a DealershipToken
app.MapPost("/api/messages", async (IBotFrameworkHttpAdapter adapter, IBot bot, ILogger<Program> logger, HttpRequest request) =>
{
  ArgumentNullException.ThrowIfNull(adapter, nameof(adapter));
  ArgumentNullException.ThrowIfNull(bot, nameof(bot));
  ArgumentNullException.ThrowIfNull(logger, nameof(logger));
  ArgumentNullException.ThrowIfNull(request?.HttpContext?.User?.Identity, nameof(request));

  if (request.HttpContext.User.Identity.IsAuthenticated)
  {
    var dealershipId = request.HttpContext.User.FindFirst("dealershipId")?.Value;
    var tokenType = request.HttpContext.User.FindFirst("tokenType")?.Value;
    if (dealershipId == null)
    { 
      logger.LogError("dealershipId is missing from the token claims");
      request.HttpContext.Response.StatusCode = 401; return; 
    }
    if (tokenType == null)
    {
      logger.LogError("tokenType is missing from the token claims");
      request.HttpContext.Response.StatusCode = 401; return;
    }

    // Use dealershipId to fetch dealership-specific data
    await adapter.ProcessAsync(request, request.HttpContext.Response, bot);
  }
  else
  {
    request.HttpContext.Response.StatusCode = 401;
  }
});

app.Run();
