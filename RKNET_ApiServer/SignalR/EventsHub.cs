using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace RKNET_ApiServer.SignalR
{
    public class EventsHub : Hub
    {
        // контекст хаба, присваивается в Program при старте приложения
        public static IHubContext<EventsHub> Current { get; set; }
    }
}
