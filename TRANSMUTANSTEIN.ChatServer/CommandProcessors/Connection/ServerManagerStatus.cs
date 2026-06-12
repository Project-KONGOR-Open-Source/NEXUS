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
        // So We Terminate The Session, Which Removes It From The Pool And Distributed Cache And Releases Its Hosting Lease
        if (requestData.ShuttingDown)
        {
            Log.Information(@"Match Server Manager ID ""{MatchServerManagerID}"" Reported That It Is Shutting Down And Will Be Removed", requestData.ServerManagerID);

            await session.Terminate(distributedCacheStore);

            return;
        }

        // Hydrate Session Metadata With The Manager's Identity And Location, So That Operational Telemetry Can Aggregate Managers Per Region
        session.Metadata.Location = requestData.Location;
        session.Metadata.Name = requestData.Name;
        session.Metadata.Address = requestData.Address;
        session.Metadata.Port = requestData.Port;
        session.Metadata.Version = requestData.Version;

        // Broadcast The Manager's Presence To The Terminal, So That It Can Be Tracked Alongside Other Managers In Its Region
        Terminal.Broadcast(@$"Match Server Manager {requestData.ServerManagerID} (""{requestData.Name}"") Registered In Region ""{GameRegions.NormaliseServerLocation(requestData.Location)}""", Terminal.ManagersPerRegion());

        // The Match Server Manager Sends "NET_CHAT_SM_STATUS" Only Once On Connect Via "CManagerChatConnection", There Is No Periodic Manager Status Heartbeat; The Connection Is Kept Alive By PING/PONG, Not Status Updates
        // So This Handler Is Effectively One-Shot And Is Not A Lease-Renewal Point; Manager Liveness (And Therefore Its Hosting Lease) Is Tracked Via The In-Memory Session Pool And Renewed Each Sweep By <see cref="StaleHostReaper"/>
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
