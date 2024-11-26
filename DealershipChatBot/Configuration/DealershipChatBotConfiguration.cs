using System.Text.Json.Serialization;

namespace DealershipChatBot.Configuration;

public class DealershipChatBotConfiguration
{
  public required string MicrosoftAppId { get; set; }
  public required string MicrosoftAppPassword { get; set; }
  public required string JwtSecret { get; set; }

  public required string EncryptionKey { get; set; }  
  public required string HostURL { get; set; }

  public required string AudienceURL { get; set; }

  public int TokenExpirationMinutes { get; set; } 

  [JsonIgnore]
  public TimeSpan TokenExpirationTimeSpan => TimeSpan.FromMinutes(TokenExpirationMinutes);


}


