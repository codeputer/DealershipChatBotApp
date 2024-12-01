using DealerWebPageBlazorWebAppShared.DTOModels;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using DealerWebPageBlazorWebAppClient;
using Blazored.LocalStorage;
using Blazored.LocalStorage.StorageOptions; // Add this using directive

//todo: review why this is the entry point never runs for the client side blazor applications!
var builder = WebAssemblyHostBuilder.CreateDefault(args);


var hostAddress = builder.HostEnvironment.BaseAddress;

builder.Services.AddBlazoredLocalStorage();

//// Add a custom logging provider to send logs to the server
//builder.Logging.AddProvider(new ServerLoggingProvider(sp => sp.GetRequiredService<HttpClient>()));

var webAssembly = builder.Build();

await webAssembly.RunAsync();
