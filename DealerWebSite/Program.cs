var builder = WebApplication.CreateBuilder(args);

var logger = LoggerFactory.Create(config =>
{
  config.AddConsole();
}).CreateLogger("Program");

builder.AddServiceDefaults(logger);

builder.Services.AddHttpClient();

// Add services to the container.
builder.Services.AddRazorPages();

var appSettings = new AppSettings(builder.Configuration);

builder.Services.AddHttpClient(HttpNamedClients.DealershipChatBot.ToString(), client =>
{
  client.BaseAddress = new Uri(appSettings.ChatbotServiceConfiguration.ChatbotServiceUrl);
});

builder.Services.AddSingleton<AppSettings>(appSettings);

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
