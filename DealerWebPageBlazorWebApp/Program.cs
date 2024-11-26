using DealerWebPageBlazorWebApp;
using DealerWebPageBlazorWebApp.Components;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
#if DEBUG
builder.Services.AddHttpClient("DefaultClient", client =>
{
  client.Timeout = TimeSpan.FromMinutes(5); // Set the timeout to 5 minutes or any desired duration
});
#endif

builder.Services.AddSingleton<AppSettings>(); 

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
    .AddInteractiveServerRenderMode();

//will fail if the appsettings.json file is not present
_ = app.Services.GetRequiredService<AppSettings>();

app.Run();
