using System.Reactive.Linq;
using System.Threading.Channels;

using Microsoft.AspNetCore.SignalR;

namespace TRANSMUTANSTEIN.ChatServer.Hubs;

public class Streaming : Hub
{
    public async IAsyncEnumerable<int> AsyncEnumerableCounter(int count, double delay)
    {
        for (var i = 0; i < count; i++)
        {
            yield return i;
            await Task.Delay(TimeSpan.FromMilliseconds(delay));
        }
    }

    public ChannelReader<int> ObservableCounter(int count, double delay)
    {
        var observable = Observable.Interval(TimeSpan.FromMilliseconds(delay))
                         .Select((_, index) => index)
                         .Take(count);

        return observable.AsChannelReader(Context.ConnectionAborted);
    }

    public ChannelReader<int> ChannelCounter(int count, double delay)
    {
        var channel = Channel.CreateUnbounded<int>();

        Task.Run(async () =>
        {
            for (var i = 0; i < count; i++)
            {
                await channel.Writer.WriteAsync(i);
                await Task.Delay(TimeSpan.FromMilliseconds(delay));
            }

            channel.Writer.TryComplete();
        });

        return channel.Reader;
    }
}
