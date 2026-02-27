using Microsoft.AspNetCore.SignalR;

namespace TaskManager.API.Hubs
{
    public class TaskHub : Hub
    {
        public async Task SendUpdateNotification()
        {
            await Clients.All.SendAsync("TaskUpdated");
        }
        //public override Task OnConnectedAsync()
        //{
        //    Console.WriteLine("In OnconnectdAsync \n \n");
        //    return base.OnConnectedAsync();
        //}
    }
}
