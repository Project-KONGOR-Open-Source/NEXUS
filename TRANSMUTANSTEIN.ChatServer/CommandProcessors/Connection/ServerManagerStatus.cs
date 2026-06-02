namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

[ChatCommand(ChatProtocol.ServerManagerToChatServer.NET_CHAT_SM_STATUS)]
public class ServerManagerStatus(IDatabase distributedCacheStore) : IAsynchronousCommandProcessor<MatchServerManagerChatSession>
{
    public async Task Process(MatchServerManagerChatSession session, ChatBuffer buffer)
    {
        ServerManagerStatusRequestData requestData = new (buffer);

        Log.Debug(@"Received Status Update From Server Manager ID ""{MatchServerManagerID}"" - Name: ""{MatchServerManagerName}"", Address: ""{MatchServerManagerAddress}:{MatchServerManagerPort}"", Location: ""{Location}"", Version: ""{Version}"", Shutting Down: {ShuttingDown}",
            requestData.ServerManagerID, requestData.Name, requestData.Address, requestData.Port, requestData.Location, requestData.Version, requestData.ShuttingDown);

        // A Match Server Manager Announcing That It Is Shutting Down Is A Graceful Departure
        // So We Terminate The Session, Which Removes It From The Pool And Distributed Cache And Releases Any Restricted Open-Password Hosting Lease It Held
        if (requestData.ShuttingDown)
        {
            Log.Information(@"Match Server Manager ID ""{MatchServerManagerID}"" Reported That It Is Shutting Down And Will Be Removed", requestData.ServerManagerID);

            await session.Terminate(distributedCacheStore);

            return;
        }

        // A Routine Heartbeat Carries No Match Server Manager Data That Is Persisted In The Distributed Cache
        // The Match Server Manager Liveness Is Tracked Via The In-Memory Session Pool And Reconciled By The "StaleHostReaper"
    }
}

file class ServerManagerStatusRequestData
{
    public byte[] CommandBytes { get; init; }

    public int ServerManagerID { get; init; }

    public string ServerLogin { get; init; }

    public string Location { get; init; }

    public string Name { get; init; }

    public string Version { get; init; }

    public string Address { get; init; }

    public short Port { get; init; }

    public bool ShuttingDown { get; init; }

    public ServerManagerStatusRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ServerManagerID = buffer.ReadInt32();
        ServerLogin = buffer.ReadString();
        Location = buffer.ReadString();
        Name = buffer.ReadString();
        Version = buffer.ReadString();
        Address = buffer.ReadString();
        Port = buffer.ReadInt16();
        ShuttingDown = buffer.ReadInt8() is not 0;
    }
}
