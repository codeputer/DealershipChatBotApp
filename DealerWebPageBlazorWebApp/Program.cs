using DealerWebPageBlazorWebApp.Components;

using DealerWebPageBlazorWebAppShared.Configuration;
using DealerWebPageBlazorWebAppShared.DTOModels;
using DealerWebPageBlazorWebAppShared.Resources;

using Microsoft.Extensions.Http.Logging;


//DealerWebPageBlazorWebApp/Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

//temp state persistence for holding issued DealershipChatBot JWT tokens
builder.Services.AddMemoryCache();

builder.Services.AddSingleton<DealerShipTokenCache>();
builder.Services.AddSingleton<JWTTokensDTO>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

#if DEBUG
// added this to allow for longer timeout for debugging
//todo: note that this is not working! Aspire is still creating a client with a 30 second timeout.
builder.Services.AddHttpClient(HttpNamedClients.DefaultClient.ToString(), client =>
{
  client.Timeout = TimeSpan.FromMinutes(5); // Set the timeout to 5 minutes or any desired duration
});
#endif

builder.Services.AddSingleton<AppSettings>();

var appSettings = new AppSettings(builder.Configuration);

builder.Services.AddHttpClient(HttpNamedClients.DealershipChatBot.ToString(), client =>
{
  client.BaseAddress = new Uri(appSettings.ChatbotServiceConfiguration.ChatbotServiceUrl);
#if DEBUG
  client.Timeout = TimeSpan.FromMinutes(5);
#endif
});


var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseWebAssemblyDebugging();
}
else
{
  app.UseExceptionHandler("/Error", createScopeForErrors: true);
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    //.AddInteractiveWebAssemblyRenderMode()
    //.AddAdditionalAssemblies(typeof(DealerWebPageBlazorWebAppClient._Imports).Assembly)
    ;

////will fail if the appsettings.json file is not present
//_ = app.Services.GetRequiredService<AppSettings>();
//var test = app.Services.GetRequiredService<JWTTokensDTO>();
//#if DEBUG
//test.DealerJwtToken = "DealerJwtToken";
//test.WebchatJwtToken = "WebchatJwtToken";
//#endif

app.Run();
