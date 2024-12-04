using System.Text.Json.Serialization;

namespace DealerWebPageBlazorWebAppShared.DTOModels;

public  class ChatWindowDTO
{
  [JsonPropertyName("questions")]
  public required List<Question> Questions { get; set; } = [];
}

public class Question
{
  [JsonPropertyName("asked")]
  public required string Asked { get; set; }

  [JsonPropertyName("answer")]
  public string? Answer { get; set; }
}
