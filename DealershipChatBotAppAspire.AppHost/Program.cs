var builder = DistributedApplication.CreateBuilder(args);

const string DealershipChatBotAPI = "DealershipChatBot";
const string DealerWebPageBlazorWebApp = "DealerWebPageBlazorWebApp";
const string DealerWebSite = "DealerWebSite"; 

//chat bot is the backend service with the capability to generate tokens as well as conducts the chat
var dealershipChatBotServiceAPI = builder.AddProject<Projects.DealershipChatBot>(DealershipChatBotAPI)
  .WithExternalHttpEndpoints()
  ;

//var launchProfileName = ShouldUseHttpForEndpoints() ? "http" : "https";
var launchProfileName = "https";

var chatbotService_Endpoint = dealershipChatBotServiceAPI.GetEndpoint(launchProfileName);

//dealer site needs to know about where the chat bot is
var dealerWebPageBlazorWebAppService = builder.AddProject<Projects.DealerWebPageBlazorWebApp>(DealerWebPageBlazorWebApp)
  .WithReference(dealershipChatBotServiceAPI)
  .WithEnvironment("ChatbotServiceConfiguration__chatbotServiceUrl", chatbotService_Endpoint)
  .WaitFor(dealershipChatBotServiceAPI)
  ;

var dealerWebPageBlazorWebApp_endpoint = dealerWebPageBlazorWebAppService.GetEndpoint(launchProfileName);

//use environmental settings (which are read by configuration manager) to pass the chatbot service url to the client
dealershipChatBotServiceAPI
  .WithEnvironment("DealershipChatBotConfiguration__HostURL", chatbotService_Endpoint)
  .WithEnvironment("DealershipChatBotConfiguration__AudienceURL", dealerWebPageBlazorWebApp_endpoint)
  ;

builder.AddProject<Projects.DealerWebSite>(DealerWebSite)
  .WithEnvironment("ChatbotServiceConfiguration__chatbotServiceUrl", chatbotService_Endpoint);

builder.Build().Run();
