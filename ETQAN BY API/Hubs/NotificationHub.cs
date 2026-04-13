using Microsoft.AspNetCore.SignalR;

namespace Etqan.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task JoinNotificationGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
    }
}