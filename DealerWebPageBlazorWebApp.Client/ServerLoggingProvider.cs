namespace DealerWebPageBlazorWebAppClient;

using Microsoft.Extensions.Logging;

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

public class ServerLoggingProvider : ILoggerProvider
{
  private readonly Func<IServiceProvider, HttpClient> _httpClientFactory;

  public ServerLoggingProvider(Func<IServiceProvider, HttpClient> httpClientFactory)
  {
    _httpClientFactory = httpClientFactory;
  }

  public ILogger CreateLogger(string categoryName)
  {
    return new ServerLogger(_httpClientFactory, categoryName);
  }

  public void Dispose()
  {
    // Dispose resources if needed
  }

  private class ServerLogger : ILogger
  {
    private readonly Func<IServiceProvider, HttpClient> _httpClientFactory;
    private readonly string _categoryName;

    public ServerLogger(Func<IServiceProvider, HttpClient> httpClientFactory, string categoryName)
    {
      _httpClientFactory = httpClientFactory;
      _categoryName = categoryName;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
      return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
      return true; // Adjust log level filtering as needed
    }

    public async void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
      if (!IsEnabled(logLevel))
      {
        return;
      }

      var message = formatter(state, exception);
      var logEntry = new
      {
        LogLevel = logLevel.ToString(),
        Category = _categoryName,
        Message = message,
        Exception = exception?.ToString()
      };

      var httpClient = _httpClientFactory.Invoke(null);
      await httpClient.PostAsJsonAsync("api/logs", logEntry);
    }
  }
}
