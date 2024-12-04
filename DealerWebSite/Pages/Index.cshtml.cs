namespace DealerWebSite.Pages;

public class IndexModel : PageModel
{
  private readonly IHttpClientFactory _httpClientFactory;
  private readonly AppSettings _appSettings;
  private readonly ILogger<IndexModel> _logger;

  public List<string> DealershipIds { get; set; } = [];
  public string SelectedDealershipId { get; set; } = string.Empty;

  public IndexModel(ILogger<IndexModel> logger, IHttpClientFactory httpClientFactory, AppSettings appSettings)
  {
    _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
    _logger = logger;
  }

  public async Task OnGetAsync()
  {
    DealershipIds = await GetDealershipIdsAsync();
  }

  public async Task<List<string>> GetDealershipIdsAsync()
  {
    var httpClient = _httpClientFactory.CreateClient();
    var url = APIRoutes.GetAbsoluteUri(APIRoutes.DealershipChatBotAPIRoutes.GetListOfDealersAPI, _appSettings.ChatbotServiceConfiguration.ChatbotServiceUrl);
    var listOfDealerIds = await httpClient.GetFromJsonAsync<List<string>>(url) ?? [];

    if (listOfDealerIds.Count == 0)
      throw new Exception("No Dealers Available");

    return listOfDealerIds;
  }


}
