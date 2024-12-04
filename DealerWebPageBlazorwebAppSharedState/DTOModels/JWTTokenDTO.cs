using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DealerWebPageBlazorWebAppShared.DTOModels;
public  class JWTTokenDTO
{
  [JsonPropertyName("jwttoken")]
  public required string JWTToken { get; init; }  
}
