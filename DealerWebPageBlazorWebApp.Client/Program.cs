using DealerWebPageBlazorWebAppShared.DTOModels;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using DealerWebPageBlazorWebAppClient; // Add this using directive

//todo: review why this is the entry point never runs for the client side blazor applications!
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configure logging
builder.Logging.SetMinimumLevel(LogLevel.Trace);
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

// Add a custom logging provider to send logs to the server
builder.Logging.AddProvider(new ServerLoggingProvider(sp => sp.GetRequiredService<HttpClient>()));

var webAssembly = builder.Build();

await webAssembly.RunAsync();
