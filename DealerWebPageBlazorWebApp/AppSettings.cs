using DealerWebPageBlazorWebApp.Configuration;

namespace DealerWebPageBlazorWebApp;


public class AppSettings
{
  public AppSettings(IConfiguration configuration)
  {
    this.ChatbotServiceConfiguration = configuration.GetSection(nameof(ChatbotServiceConfiguration)).Get<ChatbotServiceConfiguration>()
      ?? throw new ArgumentNullException(nameof(configuration));
  }

  public ChatbotServiceConfiguration ChatbotServiceConfiguration { get; init; }
}
