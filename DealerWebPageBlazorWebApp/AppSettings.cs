using DealerWebPageBlazorWebApp.Configuration;

namespace DealerWebPageBlazorWebApp;


public class AppSettings
{
  public AppSettings(IConfiguration configuration)
  {
    this.chatbotServiceConfiguration = configuration.GetSection(nameof(ChatbotServiceConfiguration)).Get<ChatbotServiceConfiguration>()
      ?? throw new ArgumentNullException(nameof(configuration));
  }

  public required ChatbotServiceConfiguration chatbotServiceConfiguration { get; set; }
}
