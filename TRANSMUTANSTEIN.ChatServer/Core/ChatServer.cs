﻿namespace TRANSMUTANSTEIN.ChatServer.Core;

public class ChatServer(IPAddress address, int port, IServiceProvider serviceProvider) : TCPServer(address, port)
{
    private IServiceProvider ServiceProvider { get; set; } = serviceProvider;

    protected override TCPSession CreateSession() => new ChatSession(this, ServiceProvider);

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"Chat Server Caught A Socket Error With Code {error}");
    }
}