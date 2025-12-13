namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

/// <summary>
///     Handles status updates from match servers.
///     Match servers periodically send their status to inform the chat server about their current state.
/// </summary>
[ChatCommand(ChatProtocol.GameServerToChatServer.NET_CHAT_GS_STATUS)]
public class ServerStatus : ISynchronousCommandProcessor<MatchServerChatSession>
{
    public void Process(MatchServerChatSession session, ChatBuffer buffer)
    {
        ServerStatusData statusData = new (buffer);

        Log.Debug(@"Received Status Update From Server ID ""{ServerID}"" - Name: ""{Name}"", Address: ""{Address}:{Port}"", Location: ""{Location}"", Status: {Status}",
            statusData.ServerID, statusData.Name, statusData.Address, statusData.Port, statusData.Location, statusData.Status);

        // TODO: Update Any Relevant Match Server Data

        // TODO: Update Server In Distributed Cache

        // TODO: If Status Is IDLE, Mark Server As Available For Match Allocation
        // TODO: If Status Is ACTIVE, Update Match Information And Player Availability States
        // TODO: If Status Is CRASHED Or KILLED, Remove Server From Pool And Handle Match Cleanup
    }
}

file class ServerStatusData
{
    public byte[] CommandBytes { get; init; }

    public int ServerID { get; init; }

    public string Address { get; init; }

    public short Port { get; init; }

    public string Location { get; init; }

    public string Name { get; init; }

    public int SlaveID { get; init; }

    public int MatchID { get; init; }

    public ChatProtocol.ServerStatus Status { get; init; }

    public byte ArrangedMatchType { get; init; }

    public ChatProtocol.ServerType Flags { get; init; }

    /// <summary>
    ///     0 = Unofficial, 1 = Official, 2 = Official With Stats
    /// </summary>
    public byte Official { get; private set; }

    public bool NoLeavers { get; private set; }

    public bool VerifiedOnly { get; private set; }

    public bool PrivateServer { get; private set; }

    /// <summary>
    ///     0 = Noobs Only, 1 = Noobs Allowed, 2 = Professionals Only
    /// </summary>
    public byte Tier { get; private set; }

    public bool AllHeroes { get; private set; }

    public bool CasualMode { get; private set; }

    public bool Gated { get; private set; }

    public bool ForceRandom { get; private set; }

    public bool AutoBalanced { get; private set; }

    public bool AdvancedOptions { get; private set; }

    public bool HeroesInDevelopment { get; private set; }

    public bool Hardcore { get; private set; }

    public string MapName { get; init; }

    public string GameName { get; init; }

    public string GameMode { get; init; }

    public byte TeamSize { get; init; }

    public short MinimumRating { get; init; }

    public short MaximumRating { get; init; }

    public int CurrentGameTime { get; init; }

    public int CurrentGamePhase { get; init; }

    public string LegionTeamInformation { get; init; }

    public string HellbourneTeamInformation { get; init; }

    public string PlayerInformation_01 { get; init; }

    public string PlayerInformation_02 { get; init; }

    public string PlayerInformation_03 { get; init; }

    public string PlayerInformation_04 { get; init; }

    public string PlayerInformation_05 { get; init; }

    public string PlayerInformation_06 { get; init; }

    public string PlayerInformation_07 { get; init; }

    public string PlayerInformation_08 { get; init; }

    public string PlayerInformation_09 { get; init; }

    public string PlayerInformation_10 { get; init; }

    public int ServerLoad { get; init; }

    public int LongServerFrameCount { get; init; }

    public long FreePhysicalMemory { get; init; }

    public long TotalPhysicalMemory { get; init; }

    public long DriveFreeSpace { get; init; }

    public long DriveTotalSpace { get; init; }

    public string ServerVersion { get; init; }

    public int ClientCount { get; init; }

    public float LoadAverage { get; init; }

    public string HostName { get; init; }

    public List<ClientNetworkStatistics> ClientNetworkStatistics { get; init; }

    public ServerStatusData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
        ServerID = buffer.ReadInt32();
        Address = buffer.ReadString();
        Port = buffer.ReadInt16();
        Location = buffer.ReadString();
        Name = buffer.ReadString();
        SlaveID = buffer.ReadInt32();
        MatchID = buffer.ReadInt32();
        Status = (ChatProtocol.ServerStatus) buffer.ReadInt8();
        ArrangedMatchType = buffer.ReadInt8();
        Flags = (ChatProtocol.ServerType) buffer.ReadInt16();
        MapName = buffer.ReadString();
        GameName = buffer.ReadString();
        GameMode = buffer.ReadString();
        TeamSize = buffer.ReadInt8();
        MinimumRating = buffer.ReadInt16();
        MaximumRating = buffer.ReadInt16();
        CurrentGameTime = buffer.ReadInt32();
        CurrentGamePhase = buffer.ReadInt32();
        LegionTeamInformation = buffer.ReadString();
        HellbourneTeamInformation = buffer.ReadString();
        PlayerInformation_01 = buffer.ReadString();
        PlayerInformation_02 = buffer.ReadString();
        PlayerInformation_03 = buffer.ReadString();
        PlayerInformation_04 = buffer.ReadString();
        PlayerInformation_05 = buffer.ReadString();
        PlayerInformation_06 = buffer.ReadString();
        PlayerInformation_07 = buffer.ReadString();
        PlayerInformation_08 = buffer.ReadString();
        PlayerInformation_09 = buffer.ReadString();
        PlayerInformation_10 = buffer.ReadString();
        ServerLoad = buffer.ReadInt32();
        LongServerFrameCount = buffer.ReadInt32();
        FreePhysicalMemory = buffer.ReadInt64();
        TotalPhysicalMemory = buffer.ReadInt64();
        DriveFreeSpace = buffer.ReadInt64();
        DriveTotalSpace = buffer.ReadInt64();
        ServerVersion = buffer.ReadString();
        ClientCount = buffer.ReadInt32();
        LoadAverage = buffer.ReadFloat32();
        HostName = buffer.ReadString();

        ClientNetworkStatistics = [];

        for (int clientIndex = 0; clientIndex < ClientCount; clientIndex++)
        {
            ClientNetworkStatistics.Add(new ClientNetworkStatistics
            {
                AccountID = buffer.ReadInt32(),
                Address = buffer.ReadString(),
                PingMinimum = buffer.ReadInt16(),
                PingAverage = buffer.ReadInt16(),
                PingMaximum = buffer.ReadInt16(),
                ReliablePacketsSent = buffer.ReadInt64(),
                ReliablePacketsAcknowledged = buffer.ReadInt64(),
                ReliablePacketsPeerSent = buffer.ReadInt64(),
                ReliablePacketsPeerAcknowledged = buffer.ReadInt64(),
                UnreliablePacketsSent = buffer.ReadInt64(),
                UnreliablePacketsPeerReceived = buffer.ReadInt64(),
                UnreliablePacketsPeerSent = buffer.ReadInt64(),
                UnreliablePacketsReceived = buffer.ReadInt64()
            });
        }

        DecodeServerFlags();
    }

    /// <summary>
    ///     Decode the packed ServerFlags bitfield into individual properties.
    /// </summary>
    private void DecodeServerFlags()
    {
        Official = Flags switch
        {
            _ when Flags.HasFlag(ChatProtocol.ServerType.SSF_OFFICIAL_WITH_STATS) => 2,  // Official With Stats
            _ when Flags.HasFlag(ChatProtocol.ServerType.SSF_OFFICIAL)            => 1,  // Official Without Stats
            _                                                                     => 0   // Unofficial
        };

        NoLeavers = Flags.HasFlag(ChatProtocol.ServerType.SSF_NOLEAVER);
        VerifiedOnly = Flags.HasFlag(ChatProtocol.ServerType.SSF_VERIFIED_ONLY);
        PrivateServer = Flags.HasFlag(ChatProtocol.ServerType.SSF_PRIVATE);
        AllHeroes = Flags.HasFlag(ChatProtocol.ServerType.SSF_ALL_HEROES);
        CasualMode = Flags.HasFlag(ChatProtocol.ServerType.SSF_CASUAL);
        Gated = Flags.HasFlag(ChatProtocol.ServerType.SSF_GATED);
        ForceRandom = Flags.HasFlag(ChatProtocol.ServerType.SSF_FORCE_RANDOM);
        AutoBalanced = Flags.HasFlag(ChatProtocol.ServerType.SSF_AUTO_BALANCE);
        AdvancedOptions = Flags.HasFlag(ChatProtocol.ServerType.SSF_ADV_OPTIONS);
        HeroesInDevelopment = Flags.HasFlag(ChatProtocol.ServerType.SSF_DEV_HEROES);
        Hardcore = Flags.HasFlag(ChatProtocol.ServerType.SSF_HARDCORE);

        Tier = Flags switch
        {
            _ when Flags.HasFlag(ChatProtocol.ServerType.SSF_TIER_PRO)           => 2,  // Professionals Only
            _ when Flags.HasFlag(ChatProtocol.ServerType.SSF_TIER_NOOBS_ALLOWED) => 1,  // Noobs Allowed
            _                                                                    => 0   // Noobs Only
        };
    }
}

file class ClientNetworkStatistics
{
    public int AccountID { get; init; }

    public string Address { get; init; } = string.Empty;

    public short PingMinimum { get; init; }

    public short PingAverage { get; init; }

    public short PingMaximum { get; init; }

    public long ReliablePacketsSent { get; init; }

    public long ReliablePacketsAcknowledged { get; init; }

    public long ReliablePacketsPeerSent { get; init; }

    public long ReliablePacketsPeerAcknowledged { get; init; }

    public long UnreliablePacketsSent { get; init; }

    public long UnreliablePacketsPeerReceived { get; init; }

    public long UnreliablePacketsPeerSent { get; init; }

    public long UnreliablePacketsReceived { get; init; }
}
