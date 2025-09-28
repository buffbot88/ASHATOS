using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Ra.Host
{
    // Static bridge so modules can broadcast without DI plumbing inside ModuleManager
    public static class FaceSignalRBridge
    {
        // Set at app startup
        public static IHubContext<FaceHub>? Hub { get; set; }

        public static Task BroadcastAsync(string eventName, object payload)
        {
            var hub = Hub;
            if (hub == null) return Task.CompletedTask;
            return hub.Clients.All.SendAsync(eventName, payload);
        }
    }
}