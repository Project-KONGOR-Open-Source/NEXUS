using System.Net;
using System.Net.Sockets;

namespace TRANSMUTANSTEIN.ChatServer.Services;

public class TCPListenerService : IHostedService, IDisposable
{
    private const string IP = "0.0.0.0";
    private const int Port = 11031;
    private static readonly TcpListener Listener = new(IPAddress.Parse(IP), Port);

    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        Listener.Start();
        Console.WriteLine($"TCP Listener Running On {IP}:{Port}");

        TcpClient client = Listener.AcceptTcpClient();

        if (client.Connected.Equals(false)) return Task.FromException(new EndOfStreamException());

        NetworkStream stream = client.GetStream();
        List<byte> data = new List<byte>();

        while (stream.DataAvailable.Equals(false))
            Thread.Sleep(10);

        while (stream.CanRead && stream.CanWrite)
        {
            while (stream.DataAvailable) // TODO: Find A Better Way To Read The Stream, Maybe ReadToEnd() Or Use A Buffer And Wait For 0x0000
                data.Add(Convert.ToByte(stream.ReadByte()));

            if (data.Count is 0)
            {
                Thread.Sleep(10);

                continue;
            }

            byte[] instruction = { data[0], data[1] };
            int commandSize = BitConverter.ToInt16(instruction, 0);

            Console.WriteLine($"Command: {data[0]}, {data[1]}");
            Console.WriteLine($"Length In Bytes: {commandSize}");

            if (data.Count < instruction.Length + commandSize)
            {
                Thread.Sleep(10);

                continue;
            }
        }

        stream.Close();
        client.Close();

        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        Listener.Stop();
        return Task.CompletedTask;
    }

    void IDisposable.Dispose()
    {
        Listener.Dispose();
    }
}
