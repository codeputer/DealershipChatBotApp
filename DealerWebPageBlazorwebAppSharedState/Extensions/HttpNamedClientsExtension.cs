namespace DealerWebPageBlazorWebAppShared.Extensions;

public static class HttpNamedClientsExtension
{
  public static HttpClient CreatedNamedHttpClient(this IHttpClientFactory httpClientFactory, HttpNamedClients namedClient)
  {
    if (namedClient == HttpNamedClients.Unknown)
    {
      throw new ArgumentException("Unknown named client requested.");
    }

    return httpClientFactory.CreateClient(namedClient.ToString());
  }
}
