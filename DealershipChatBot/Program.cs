//DealerShipChatBot/Program.cs
using DealershipChatBot.APIRouteHandlers;

using DealerWebPageBlazorWebAppShared.Managers;
using DealerWebPageBlazorWebAppShared.Policies;

var builder = WebApplication.CreateBuilder(args);

var logger = LoggerFactory.Create(config =>
{
  config.AddConsole();
}).CreateLogger("Program");

builder.AddServiceDefaults(logger);

builder.Services.AddSingleton<IBotFrameworkHttpAdapter, CloudAdapter>();
builder.Services.AddSingleton<DealershipChatBot.AppSettings>();
builder.Services.AddSingleton<MinimalAPIRouteManager>();
builder.Services.AddSingleton<TokenHelper>();

builder.Services.AddTransient<IBot, DealershipBot>();
builder.Services.AddTransient<IRouteHandlerDelegate<IResult>, VersionAPIRouteHandler>();
builder.Services.AddTransient<IRouteHandlerDelegate<IResult>, WebchatMessagesAPIRouteHandler>();
builder.Services.AddTransient<IRouteHandlerDelegate<IResult>, GenerateTokenAPIRouteHandler>();
builder.Services.AddTransient<IRouteHandlerDelegate<IResult>, DecryptTokenAPIRouteHandler>();
builder.Services.AddTransient<IRouteHandlerDelegate<IResult>, GetWebChatArtifactsAPIRouteHandler>();
builder.Services.AddTransient<IRouteHandlerDelegate<IResult>, GetWebTokenAPIRouteHandler>();

builder.Services.AddTransient<DealerShipTokenCache>();

// Add CORS policy
builder.Services.AddCors(options =>
{
  options.AddPolicy("RelaxedCorsPolicy", builder =>
  {
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
  });
});

//get the configuration of the application
var appSettings = new DealershipChatBot.AppSettings(builder.Configuration);
var tokenHelper = new TokenHelper(appSettings);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
      options.TokenValidationParameters = tokenHelper.GetTokenValidationParameters();

      options.Events = new JwtBearerEvents
      {
        OnAuthenticationFailed = context =>
        {
          Console.WriteLine("OnAuthenticationFailed: " + context.Exception.Message);
          if (context.Exception is SecurityTokenExpiredException)
          {
            context.Response.Headers.Append("Token-Expired", "true");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            // Optionally log the failure or take custom action
            return context.Response.WriteAsync("Token has expired.");
          }

          options.Events = new JwtBearerEvents
          {
            OnAuthenticationFailed = context =>
            {
              Console.WriteLine("OnAuthenticationFailed: " + context.Exception.Message);
              if (context.Exception is SecurityTokenExpiredException)
              {
                context.Response.Headers.Append("Token-Expired", "true");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                // Optionally log the failure or take custom action
                return context.Response.WriteAsync("Token has expired.");
              }
              if (context.Response.HasStarted == false)
              {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var result = System.Text.Json.JsonSerializer.Serialize(new { message = "Authentication failed" });
                return context.Response.WriteAsync(result);
              }
              return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
              Console.WriteLine("OnChallenge: " + context.AuthenticateFailure?.Message);
              if (context.Response.HasStarted == false)
              {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var result = System.Text.Json.JsonSerializer.Serialize(new { message = "You are not authorized" });
                return context.Response.WriteAsync(result);
              }
              return Task.CompletedTask;
            }
          };
          return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
          Console.WriteLine("OnChallenge: " + context.AuthenticateFailure?.Message);
          if (context.Response.HasStarted == false)
          {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new { message = "You are not authorized" });
            return context.Response.WriteAsync(result);
          }
          return Task.CompletedTask;
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
builder.Services.AddAuthorizationBuilder()
  .AddPolicy(Policies.TokenTypePolicyValues.DealershipChatTokenPolicy.ToString(), policy =>
      policy.RequireClaim(ClaimKeyValues.TokenType.ToString(), TokenTypeValues.DealershipToken.ToString()));
 
builder.Services.AddAuthorizationBuilder()
  .AddPolicy(Policies.TokenTypePolicyValues.WebChatTokenPolicy.ToString(), policy =>
      policy.RequireClaim(ClaimKeyValues.TokenType.ToString(), TokenTypeValues.WebChatToken.ToString()));

builder.Services.AddAuthorization();

var app = builder.Build();

var minimalAPIRouteManager = app.Services.GetRequiredService<MinimalAPIRouteManager>();
minimalAPIRouteManager.RegisterRoutes(app);

app.MapDefaultEndpoints();

var validateConfiguration = app.Services.GetRequiredService<DealershipChatBot.AppSettings>();

app.UseCors("RelaxedCorsPolicy");
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


