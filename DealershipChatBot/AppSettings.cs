
using DealershipChatBotAppSettings = DealershipChatBot.AppSettings;
using DealershipChatBot.Configuration;

namespace DealershipChatBot;
public class AppSettings
{
  public AppSettings(IConfiguration configuration)
  {
    this.DealershipChatBotConfiguration = configuration.GetSection(nameof(DealershipChatBotConfiguration)).Get<DealershipChatBotConfiguration>()
      ?? throw new ArgumentNullException(nameof(configuration));
  }

  public DealershipChatBotConfiguration DealershipChatBotConfiguration { get; init; }
}


