using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DealershipBlazorApp;
using Microsoft.Extensions.DependencyInjection; // Add this using directive

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//to call the DealershipChatBot service, for a token
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7021") });
//the api is: "/api/GenerateToken/{dealershipId}"

await builder.Build().RunAsync();
