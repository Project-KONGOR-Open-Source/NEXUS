namespace TRANSMUTANSTEIN.ChatServer.Hubs;

using Microsoft.AspNetCore.SignalR;

using System.Threading.Tasks;

public class ChatHub : Hub
{
    // Define methods to handle incoming messages from clients
    public async Task SendMessage(string user, string message)
    {
        // Broadcast the message to all clients
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public async Task HandleTcpMessage(string message)
    {
        // Broadcast the message to all clients
        await Clients.All.SendAsync("ReceiveMessage", "TCP", message);
        // Process the message further as needed
    }
}
