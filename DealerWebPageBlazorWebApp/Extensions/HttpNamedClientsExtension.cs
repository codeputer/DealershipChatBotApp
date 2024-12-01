namespace DealerWebPageBlazorWebApp.Extensions;

public static class HttpNamedClientsExtension
{
  public static HttpClient GetNamedHttpClient(this IHttpClientFactory httpClientFactory, HttpNamedClients namedClient)
  {
    if (namedClient == HttpNamedClients.Unknown)
    {
      throw new ArgumentException("Unknown named client requested.");
    }

    return httpClientFactory.CreateClient(namedClient.ToString());
  }
}
