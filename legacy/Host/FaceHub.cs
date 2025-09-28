using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Ra.Host
{
    // Clients subscribe to receive face updates
    public class FaceHub : Hub
    {
        // Optional client-invokable methods could go here.
        // We only broadcast server->clients using the bridge.
        public override Task OnConnectedAsync() => base.OnConnectedAsync();
    }
}