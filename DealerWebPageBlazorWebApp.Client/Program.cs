using DealerWebPageBlazorWebAppShared.DTOModels;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;

//todo: review why this is the entry point never runs for the client side blazor applications!
var builder = WebAssemblyHostBuilder.CreateDefault(args);

var webAssembly = builder.Build();

IConfigurationRoot configuration = (IConfigurationRoot)webAssembly.Configuration;

await webAssembly.RunAsync();
