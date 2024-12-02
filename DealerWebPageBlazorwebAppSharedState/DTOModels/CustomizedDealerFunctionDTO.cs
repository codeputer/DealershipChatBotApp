using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using DealerWebPageBlazorWebAppShared.Utilities.String;

namespace DealerWebPageBlazorWebAppShared.DTOModels;
public class CustomizedDealerFunctionDTO
{
  public required string CustomizedScript { get; init; }

  public static CustomizedDealerFunctionDTO CustomizedDealerFunctionDTOFactory(string javaScriptFunction)
  {
    ArgumentNullException.ThrowIfNullOrWhiteSpace(javaScriptFunction, nameof(javaScriptFunction));

    return new CustomizedDealerFunctionDTO()
    {
      CustomizedScript = Base64Utilities.Base64Encode(javaScriptFunction)
    };
  }
  public string DecodeFromBase64()
  {
    if (string.IsNullOrWhiteSpace(CustomizedScript))
      return string.Empty;
    else
      return Base64Utilities.Base64Decode(CustomizedScript);
  }
}
