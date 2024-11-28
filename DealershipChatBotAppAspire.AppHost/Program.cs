var builder = DistributedApplication.CreateBuilder(args);

const string DealershipChatBot = "DealershipChatBot";
const string DealerWebPageBlazorWebApp = "DealerWebPageBlazorWebApp";
const string DealerWebPageBlazorWebAppClient = "DealerWebPageBlazorWebAppClient";

//chat bot is the backend service with the capability to generate tokens as well as conducts the chat
var dealershipChatBotService = builder.AddProject<Projects.DealershipChatBot>(DealershipChatBot)
  .WithExternalHttpEndpoints()
  ;

//var launchProfileName = ShouldUseHttpForEndpoints() ? "http" : "https";
var launchProfileName = "https";

var chatbotService_Endpoint = dealershipChatBotService.GetEndpoint(launchProfileName);

//dealer site needs to know about where the chat bot is
var dealerWebPageBlazorWebAppService = builder.AddProject<Projects.DealerWebPageBlazorWebApp>(DealerWebPageBlazorWebApp)
  .WithEnvironment("ChatbotServiceConfiguration__chatbotServiceUrl", chatbotService_Endpoint)
  .WaitFor(dealershipChatBotService)
  ;

var dealerWebPageBlazorWebApp_endpoint = dealerWebPageBlazorWebAppService.GetEndpoint(launchProfileName);

//chat bot needs to know about itself, as well as the audience
dealershipChatBotService
  .WithEnvironment("DealershipChatBotConfiguration__HostURL", chatbotService_Endpoint)
  .WithEnvironment("DealershipChatBotConfiguration__AudienceURL", dealerWebPageBlazorWebApp_endpoint)
  ;

//var dealerWebPageBlazorWebApp_Client = builder.AddProject<Projects.DealerWebPageBlazorWebAppClient>(DealerWebPageBlazorWebAppClient)
//  .WithEnvironment("DealershipChatBotConfiguration__HostURL", chatbotService_Endpoint)
//  .WithEnvironment("DealershipChatBotConfiguration__AudienceURL", dealerWebPageBlazorWebApp_endpoint);

builder.Build().Run();
