using SlackAPI;

namespace DependencyInjectionWorkshop.Adapters
{
    public class SlackAdapter
    {
        public void Notify(string message)
        {
            var slackClient = new SlackClient("my token");
            slackClient.PostMessage(resp => { }, "my channel", message, "my bot name");
        }
    }
}