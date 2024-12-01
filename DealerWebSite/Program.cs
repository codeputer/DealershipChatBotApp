var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var appSettings = new AppSettings(builder.Configuration); 
builder.Services.AddHttpClient(HttpNamedClients.DealershipChatBot.ToString(), client =>
{
  client.BaseAddress = new Uri(appSettings.ChatbotServiceConfiguration.ChatbotServiceUrl);
});

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddSingleton<AppSettings>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
