namespace DealerWebSite.Pages;

public partial class DealerPageModel : PageModel
{
  private readonly IHttpClientFactory _httpClientFactory;
  private readonly AppSettings _appSettings;
  private readonly ILogger<IndexModel> _logger;

  public DealerPageModel(ILogger<IndexModel> logger, IHttpClientFactory httpClientFactory, AppSettings appSettings)
  {
    //use to find code that references this route
    _ = APIRoutes.DealershipChatBotAPIRoutes.GetDealershipChatWindowScriptForDemoAPI;

    _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
    _logger = logger;
  }

  [FromQuery]
  public required string DealerShipId { get; set; }

  public string DealerScript { get; set; } = string.Empty;

  public async Task OnGetAsync(string queryDealerShipId)
  {
    ArgumentNullException.ThrowIfNullOrWhiteSpace(queryDealerShipId, nameof(queryDealerShipId));

    this.DealerShipId = queryDealerShipId;
    var customizedScriptPackaged = await GetScript(queryDealerShipId) ?? throw new Exception($"Script page was unable to be created");

    DealerScript = customizedScriptPackaged.DecodeFromBase64();
  }

  public async Task<CustomizedDealerFunctionDTO> GetScript(string dealerShipId)
  {
    var httpClient = _httpClientFactory.CreatedNamedHttpClient(HttpNamedClients.DealershipChatBot);

    var url = APIRoutes.GetUrlPath(APIRoutes.DealershipChatBotAPIRoutes.GetDealershipChatWindowScriptForDemoAPI);
    var queryParams = new Dictionary<string, string?>
        {
            { nameof(DealerShipId), dealerShipId }
        };
    var urlWithQueryString = QueryHelpers.AddQueryString(url, queryParams);
        
    var customizedDealershipScript = await httpClient.GetFromJsonAsync<CustomizedDealerFunctionDTO>(urlWithQueryString);
    if (customizedDealershipScript is null)
      throw new Exception("Unable to retrieve script.");

    return customizedDealershipScript;
  }

}

