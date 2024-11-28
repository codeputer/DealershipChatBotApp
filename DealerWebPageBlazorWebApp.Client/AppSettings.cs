using DealerWebPageBlazorWebAppClient.Configuration;

namespace DealerWebPageBlazorWebAppClient;


public class AppSettings
{
  public AppSettings(IConfiguration configuration)
  {
    ChatbotServiceConfiguration = configuration.GetSection(nameof(ChatbotServiceConfiguration)).Get<ChatbotServiceConfiguration>()
      ?? throw new ArgumentNullException(nameof(configuration));
  }

  public ChatbotServiceConfiguration ChatbotServiceConfiguration { get; init; }
}
