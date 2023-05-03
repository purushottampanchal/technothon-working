using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace anuvadha.Services
{
    public class ProgressHub:Hub
    {
        public static async Task SendMessageAsync(string message)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();

            hubContext.Clients.All.sendMessage(message);

        }

    }
}
