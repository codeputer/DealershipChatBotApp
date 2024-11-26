namespace DealershipChatBot;

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

using System.Threading;
using System.Threading.Tasks;

public class DealershipBot : ActivityHandler
{
  protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
  {
    var userMessage = turnContext.Activity.Text.ToLower();

    if (userMessage.Contains("sales hours"))
    {
      await turnContext.SendActivityAsync(MessageFactory.Text("Our sales hours are 9 AM - 6 PM."), cancellationToken);
    }
    else if (userMessage.Contains("service hours"))
    {
      await turnContext.SendActivityAsync(MessageFactory.Text("Our service hours are 8 AM - 5 PM."), cancellationToken);
    }
    else if (userMessage.Contains("models"))
    {
      await turnContext.SendActivityAsync(MessageFactory.Text("We have models like Camry, Corolla, and RAV4."), cancellationToken);
    }
    else
    {
      await turnContext.SendActivityAsync(MessageFactory.Text("I'm sorry, I didn't understand that. Can you please rephrase?"), cancellationToken);
    }
  }
}

