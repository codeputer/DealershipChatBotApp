using DealerWebPageBlazorWebApp;
using DealerWebPageBlazorWebApp.Components;

//DealerWebPageBlazorWebApp/Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

//temp state persistence for holding issued DealershipChatBot JWT tokens
builder.Services.AddMemoryCache();

builder.Services.AddSingleton<DealerShipTokenCache>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

#if DEBUG
// added this to allow for longer timeout for debugging
//todo: note that this is not working! Aspire is still creating a client with a 30 second timeout.
builder.Services.AddHttpClient("DefaultClient", client =>
{
  client.Timeout = TimeSpan.FromMinutes(5); // Set the timeout to 5 minutes or any desired duration
});
#endif

builder.Services.AddSingleton<AppSettings>();

var appSettings = new AppSettings(builder.Configuration);

builder.Services.AddHttpClient("DealerWebPageBlazorWebApp.DealershipChatBot", client =>
{
  client.BaseAddress = new Uri(appSettings.ChatbotServiceConfiguration.ChatbotServiceUrl);
#if DEBUG
  client.Timeout = TimeSpan.FromMinutes(5);
#endif
});


var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
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
    ;

//will fail if the appsettings.json file is not present
_ = app.Services.GetRequiredService<AppSettings>();

app.Run();
