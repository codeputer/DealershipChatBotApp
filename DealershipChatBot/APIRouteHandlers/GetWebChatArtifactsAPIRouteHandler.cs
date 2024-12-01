using System.IdentityModel.Tokens.Jwt;

namespace DealershipChatBot.APIRouteHandlers;

public class GetWebChatArtifactsAPIRouteHandler(AppSettings appSettings) : IRouteHandlerDelegate<IResult>
{
  private readonly AppSettings _appSettings = appSettings;

  public string RouteName => APIRoutes.DealershipChatBotAPIRoutes.GetWebChatArtifacts.ToString();
  public string RoutePath => APIRoutes.GetUrlPath(APIRoutes.DealershipChatBotAPIRoutes.GetWebChatArtifacts);
  public Delegate DelegateHandler => GetWebChatArtifactsDelegate;
  public HttpMethod? HttpMethod => HttpMethod.Get;
  public bool ExcludeFromAPIDescription => false;
  public bool RequireAuthorization => true;
  public IResult GetWebChatArtifactsDelegate(HttpContext httpContext,
                                             [FromQuery] string dealershipId,
                                             [FromServices] TokenHelper tokenHelper
                                             )
  {

    var request = httpContext.Request;

    var clientIp = request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

    // Step 1: Validate the Dealer Token
    if (!request.Headers.TryGetValue("Authorization", out var authHeader) || string.IsNullOrWhiteSpace(authHeader))
    {
      return Results.Unauthorized();
    }

    var claims = tokenHelper.GenerateClaims(TokenTypeValues.WebChatToken, dealershipId, "DealerName", clientIp);
    var webchatToken = tokenHelper.GenerateJWTToken(claims);

    var webchatMessagesAPI = string.Empty;
    if (Uri.TryCreate(_appSettings.DealershipChatBotConfiguration.HostURL, UriKind.Absolute, out Uri? hostUrl))
    {
      var port = hostUrl.Port > 0 ? hostUrl.Port : 80;
      webchatMessagesAPI = new UriBuilder(hostUrl.Scheme, hostUrl.Host, port, pathValue: APIRoutes.GetUrlPath(APIRoutes.DealershipChatBotAPIRoutes.WebChatMessages)).Uri.ToString();
    }
    else
      ArgumentNullException.ThrowIfNull(hostUrl);

    var html = $@"
        <div id='webchat-container'>
            <div id='webchat-window'></div>
            <textarea id='webchat-input'></textarea>
            <button id='webchat-submit'>Send</button>
        </div>";
    var css = @"
        #webchat-container { /* Your CSS here */ }
        #webchat-window { /* Styling for chat window */ }
        #webchat-input { /* Input styles */ }
        #webchat-submit { /* Button styles */ }";
    var javaScript = $@"
        document.addEventListener('DOMContentLoaded', () => {{
            const chatWindow = document.getElementById('webchat-window');
            const inputField = document.getElementById('webchat-input');
            const submitButton = document.getElementById('webchat-submit');

            function addMessage(text, isUser) {{
                const message = document.createElement('div');
                message.textContent = text;
                message.style.textAlign = isUser ? 'left' : 'right';
                chatWindow.appendChild(message);
            }}

            submitButton.addEventListener('click', async () => {{
                const question = inputField.value.trim();
                if (!question) return;

                addMessage(question, true);
                inputField.value = '';

                try {{
                    const response = await fetch('{webchatMessagesAPI}', {{
                        method: 'POST',
                        headers: {{
                            'Content-Type': 'application/json',
                            'Authorization': `Bearer {webchatToken}`
                        }},
                        body: JSON.stringify({{ latest: question }})
                    }});

                    if (!response.ok) throw new Error('Failed to send message');
                    const data = await response.json();
                    addMessage(data.reply, false);
                }} catch (error) {{
                    addMessage('Error: Unable to send message.', false);
                }}
            }});
        }});";

    return Results.Ok(new
    {
      webchatToken,
      html,
      css,
      javaScript
    });
  }
}

