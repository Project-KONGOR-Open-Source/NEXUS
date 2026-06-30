namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests.Communication;

/// <summary>
///     Covers the join validation on <see cref="ChatChannel.Join"/>: a channel flagged <see cref="ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_UNJOINABLE"/> and a full channel both reject a manual join and notify the requesting client, while the internal member additions used to populate matchmaking group and match channels are still accepted.
///     The notifying paths run the session over an in-memory connection so the test reads the actual notice frame back off the wire, confirming it is genuinely sent rather than discarded.
/// </summary>
public sealed class ChatChannelJoinTests
{
    [Test]
    public async Task Manual_Join_Of_An_Unjoinable_Channel_Is_Rejected_And_Notifies_The_Client()
    {
        ChatChannel channel = ChatChannel.GetOrCreateGroupChannel(900_001);

        await using RunningClientSession client = RunningClientSession.Start(CreateAccount("UnjoinableJoiner"));

        channel.Join(client.Session);

        (ushort command, string message) = await client.ReadNotice();

        using (Assert.Multiple())
        {
            await Assert.That(channel.Members.ContainsKey(client.Session.Account.Name)).IsFalse();
            await Assert.That(client.Session.CurrentChannels.Count).IsEqualTo(0);
            await Assert.That(command).IsEqualTo((ushort) ChatProtocol.Command.CHAT_CMD_MESSAGE_ALL);
            await Assert.That(message).IsEqualTo("This Channel Cannot Be Joined");
        }
    }

    [Test]
    public async Task An_Unjoinable_Channel_Still_Accepts_Internal_Member_Additions()
    {
        ChatChannel channel = ChatChannel.GetOrCreateGroupChannel(900_002);

        ClientChatSession session = CreateSession("InternalJoiner");

        ChatChannelMember member = new (session, channel);

        bool added = channel.Members.TryAdd(session.Account.Name, member);

        await Assert.That(added).IsTrue();
        await Assert.That(channel.Members.ContainsKey(session.Account.Name)).IsTrue();
    }

    [Test]
    public async Task Manual_Join_Of_A_Full_Channel_Is_Rejected_And_Notifies_The_Client()
    {
        ChatChannel channel = ChatChannel.GetOrCreate(CreateSession("FullChannelCreator"), "Full Channel Test");

        // Fill The Channel To Its Member Cap Via The Internal Path, Which Bypasses Join
        for (int memberIndex = 0; memberIndex < ChatProtocol.MAX_USERS_PER_CHANNEL; memberIndex++)
        {
            ClientChatSession existingSession = CreateSession($"FullChannelMember{memberIndex}");

            channel.Members.TryAdd(existingSession.Account.Name, new ChatChannelMember(existingSession, channel));
        }

        await using RunningClientSession client = RunningClientSession.Start(CreateAccount("LateJoiner"));

        channel.Join(client.Session);

        (ushort command, string message) = await client.ReadNotice();

        using (Assert.Multiple())
        {
            await Assert.That(channel.Members.ContainsKey(client.Session.Account.Name)).IsFalse();
            await Assert.That(command).IsEqualTo((ushort) ChatProtocol.Command.CHAT_CMD_MESSAGE_ALL);
            await Assert.That(message).IsEqualTo("This Channel Is Full");
        }
    }

    /// <summary>
    ///     Constructs a stand-in <see cref="ClientChatSession"/> that bypasses the production constructor, populating only the account and channel-tracking state that <see cref="ChatChannel.Join"/> reads on the paths that do not send to the client.
    /// </summary>
    private static ClientChatSession CreateSession(string accountName)
    {
        ClientChatSession session = (ClientChatSession)RuntimeHelpers.GetUninitializedObject(typeof(ClientChatSession));

        session.Account = CreateAccount(accountName);
        session.CurrentChannels = [];

        return session;
    }

    private static Account CreateAccount(string accountName)
    {
        Role role = new () { Name = "Player" };

        User user = new ()
        {
            EmailAddress    = $"{accountName}@test.local",
            Role            = role,
            SRPPasswordSalt = string.Empty,
            SRPPasswordHash = string.Empty
        };

        return new Account
        {
            ID     = Math.Abs(Guid.NewGuid().GetHashCode()),
            Name   = accountName,
            User   = user,
            IsMain = true,
            Type   = AccountType.Normal
        };
    }
}

/// <summary>
///     Drives a real <see cref="ClientChatSession"/> over a pair of in-memory pipes, standing in for the duplex pipe that Kestrel would otherwise supply, so that a test can invoke channel operations directly and read the frames the session sends back to the client.
/// </summary>
file sealed class RunningClientSession : IAsyncDisposable
{
    public ClientChatSession Session { get; }

    private WebApplication Host { get; }

    private Pipe Inbound { get; }

    private Pipe Outbound { get; }

    private Task SessionTask { get; }

    private RunningClientSession(ClientChatSession session, WebApplication host, Pipe inbound, Pipe outbound, Task sessionTask)
    {
        Session = session;
        Host = host;
        Inbound = inbound;
        Outbound = outbound;
        SessionTask = sessionTask;
    }

    public static RunningClientSession Start(Account account)
    {
        WebApplication host = WebApplication.CreateSlimBuilder().Build();

        // The Chat Server's Static Logger Facade Throws Until Initialised; A Sink-Less Serilog Logger Satisfies It Without Producing Output
        Log.Initialise(new Serilog.LoggerConfiguration().CreateLogger());

        Pipe inbound = new ();
        Pipe outbound = new ();

        DefaultConnectionContext connection = new ()
        {
            Transport = new InMemoryDuplexPipe(inbound.Reader, outbound.Writer)
        };

        ClientChatSession session = new (connection, host.Services) { Account = account };

        Task sessionTask = session.RunAsync();

        return new RunningClientSession(session, host, inbound, outbound, sessionTask);
    }

    /// <summary>
    ///     Reads the next frame the session sends and returns its command code and the message string a <see cref="ChatProtocol.Command.CHAT_CMD_MESSAGE_ALL"/> notice carries.
    /// </summary>
    public async Task<(ushort Command, string Message)> ReadNotice()
    {
        byte[] payload = await ReadFrame();

        ushort command = BitConverter.ToUInt16(payload, 0);

        // A Notice Frame Is The Command Followed By Two Null-Terminated UTF-8 Strings: The Sender Name And The Message
        List<string> strings = ParseNullTerminatedStrings(payload, offset: 2);

        string message = strings.Count > 1 ? strings[1] : string.Empty;

        return (command, message);
    }

    private async Task<byte[]> ReadFrame()
    {
        using CancellationTokenSource timeout = new (TimeSpan.FromSeconds(5));

        while (true)
        {
            ReadResult result = await Outbound.Reader.ReadAsync(timeout.Token);
            ReadOnlySequence<byte> buffer = result.Buffer;

            if (buffer.Length >= 2)
            {
                ushort length = BitConverter.ToUInt16(buffer.Slice(0, 2).ToArray(), 0);

                if (buffer.Length >= 2 + length)
                {
                    byte[] payload = buffer.Slice(2, length).ToArray();

                    Outbound.Reader.AdvanceTo(buffer.GetPosition(2 + length));

                    return payload;
                }
            }

            Outbound.Reader.AdvanceTo(buffer.Start, buffer.End);

            if (result.IsCompleted)
                throw new InvalidOperationException("The Connection Closed Before A Complete Frame Was Received");
        }
    }

    private static List<string> ParseNullTerminatedStrings(byte[] payload, int offset)
    {
        List<string> strings = [];

        int start = offset;

        for (int index = offset; index < payload.Length; index++)
        {
            if (payload[index] is not 0)
                continue;

            strings.Add(System.Text.Encoding.UTF8.GetString(payload, start, index - start));

            start = index + 1;
        }

        return strings;
    }

    public async ValueTask DisposeAsync()
    {
        await Inbound.Writer.CompleteAsync();

        try { await SessionTask.WaitAsync(TimeSpan.FromSeconds(5)); }

        catch { }

        await Host.DisposeAsync();
    }
}

file sealed class InMemoryDuplexPipe(PipeReader input, PipeWriter output) : IDuplexPipe
{
    public PipeReader Input { get; } = input;

    public PipeWriter Output { get; } = output;
}
