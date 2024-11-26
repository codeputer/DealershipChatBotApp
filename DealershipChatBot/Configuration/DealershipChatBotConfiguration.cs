using System.Text.Json.Serialization;

namespace DealershipChatBot.Configuration;

public class DealershipChatBotConfiguration
{
  public required string MicrosoftAppId { get; init; }
  public required string MicrosoftAppPassword { get; init; }
  public required string JwtSecret { get; init; }

  public required string EncryptionKey { get; init; }  
  public required string HostURL { get; init; }

  public required string AudienceURL { get; init; }

  public int TokenExpirationMinutes { get; init; } 

  [JsonIgnore]
  public TimeSpan TokenExpirationTimeSpan => TimeSpan.FromMinutes(TokenExpirationMinutes);


}


