﻿using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace DealershipChatBot.Configuration;

public partial class DealershipChatBotConfiguration
{
  public required string MicrosoftAppId { get; init; }
  public required string MicrosoftAppPassword { get; init; }
  public required string JwtSecret { get; init; }

  public required string EncryptionKey { get; init; }
  public required string HostURL { get; init; }

  public required string AudienceURL { get; init; }

  public int TokenExpirationMinutes { get; init; }

  public required string FunctionTemplateFileName { get; init; }

  [JsonIgnore]
  public TimeSpan TokenExpirationTimeSpan => TimeSpan.FromMinutes(TokenExpirationMinutes);

  public string ReadWebChatFunctionTemplate()
  {
    return DealershipChatBotConfiguration.ReadWebChatWindowFunctionTemplateFileName(this.FunctionTemplateFileName);
  }

  public static string ReadWebChatWindowFunctionTemplateFileName(string templateFile)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(templateFile, nameof(templateFile));
    string currentDirectory = AppContext.BaseDirectory;

    if (templateFile.StartsWith("\\"))
    {
      templateFile = templateFile[1..];
    }

    string? subDirectory = Path.GetDirectoryName(templateFile);
    string? filename = Path.GetFileName(templateFile);

    string fullPath = string.Empty;
    if (string.IsNullOrWhiteSpace(subDirectory)==false)
    {
      fullPath = Path.Combine(currentDirectory, subDirectory, filename);
    }
    else
    {
      fullPath = Path.Combine(currentDirectory, filename);
    }

    if (!File.Exists(fullPath))
    {
      throw new FileNotFoundException($"The file '{templateFile}' does not exist in the current directory.");
    }

    var jsFileContent = File.ReadAllText(fullPath);

    //// Clean the file content (remove unwanted characters like carriage returns and line feeds)
    //string cleanedContent = jsFileContent.Replace("\r", "").Replace("\n", "");

    //// Ensure no escape sequences are introduced
    //cleanedContent = cleanedContent.Replace("\\\"", "\""); // Convert \" to "

    return jsFileContent;
  }

}


