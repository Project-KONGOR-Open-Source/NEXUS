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

        // TODO: Update Server Load Metrics For Load Balancing Decisions
    }
}

public class ServerStatusData
{
    public byte[] CommandBytes;

    // Core Server Information
    public int ServerID;
    public string Address;
    public short Port;
    public string Location;
    public string Name;
    public int SlaveID;
    public int MatchID;
    public ChatProtocol.ServerStatus Status;
    public byte ArrangedMatchType;

    // Match Configuration (Protocol >= 59 Uses Packed Flags)
    public ChatProtocol.ServerType Flags;                  // Packed flags (ushort, 2 bytes)

    // Decoded Flag Values (For Convenience)
    public byte Official;                      // 0 = Unofficial, 1 = Official, 2 = Official With Stats
    public bool NoLeaver;                      // No Leavers Allowed
    public bool VerifiedOnly;                  // Verified Accounts Only
    public bool ServerAccess;                  // Private Server
    public byte Tier;                          // 0 = Noobs Only, 1 = Noobs Allowed, 2 = Pro
    public bool AllHeroes;                     // All Heroes Enabled
    public bool CasualMode;                    // Casual Mode
    public bool Gated;                         // Gated Content
    public bool ForceRandom;                   // Force Random (Deprecated)
    public bool AutoBalanced;                  // Auto Balanced
    public bool AdvancedOptions;               // Advanced Options
    public bool DevHeroes;                     // Dev Heroes
    public bool Hardcore;                      // Hardcore Mode

    // Match Details
    public string MapName;
    public string GameName;
    public string GameMode;
    public byte TeamSize;
    public short MinimumPSR;                  // Minimum Public Skill Rating
    public short MaximumPSR;                  // Maximum Public Skill Rating
    public int CurrentGameTime;                // Current Game Time In Milliseconds
    public int CurrentGamePhase;               // Current Game Phase

    // Team And Player Information (12 Strings: 2 Team Infos + 10 Player Infos)
    public string LegionTeamInfo;
    public string HellbourneTeamInfo;
    public string Player0Info;
    public string Player1Info;
    public string Player2Info;
    public string Player3Info;
    public string Player4Info;
    public string Player5Info;
    public string Player6Info;
    public string Player7Info;
    public string Player8Info;
    public string Player9Info;

    // Server Performance Metrics
    public int ServerLoad;                     // Server Load Percentage
    public int LongServerFrameCount;           // Number Of Long Server Frames (Performance Issues)
    public long FreePhysicalMemory;            // Free Physical Memory In Bytes
    public long TotalPhysicalMemory;           // Total Physical Memory In Bytes
    public long DriveFreeSpace;                // Drive Free Space In Bytes
    public long DriveTotalSpace;               // Drive Total Space In Bytes
    public string ServerVersion;               // Server Version String

    // Per-Client Network Statistics (Protocol Version >= 59)
    public int ClientCount;                    // Number Of Clients With Diagnostic Data
    public float LoadAverage;                  // Current Server Load Average
    public string Hostname;                    // Server Hostname (CRITICAL: This Field Was Missing!)
    public List<ClientNetworkStats> ClientStats;

    public ServerStatusData(ChatBuffer buffer)
    {
        Log.Information(@"[PROTOCOL DEBUG] Starting ServerStatusData Parse - Buffer Size: {Size}, Offset: {Offset}", buffer.Size, buffer.Offset);

        CommandBytes = buffer.ReadCommandBytes();
        Log.Information(@"[PROTOCOL DEBUG] After CommandBytes - Offset: {Offset}", buffer.Offset);

        // Core Server Information
        ServerID = buffer.ReadInt32();
        Log.Information(@"[PROTOCOL DEBUG] After ServerID ({Value}) - Offset: {Offset}", ServerID, buffer.Offset);

        Address = buffer.ReadString();
        Log.Information(@"[PROTOCOL DEBUG] After Address ({Value}) - Offset: {Offset}", Address, buffer.Offset);

        Port = buffer.ReadInt16();
        Log.Information(@"[PROTOCOL DEBUG] After Port ({Value}) - Offset: {Offset}", Port, buffer.Offset);

        Location = buffer.ReadString();
        Log.Information(@"[PROTOCOL DEBUG] After Location ({Value}) - Offset: {Offset}", Location, buffer.Offset);

        Name = buffer.ReadString();
        Log.Information(@"[PROTOCOL DEBUG] After Name ({Value}) - Offset: {Offset}", Name, buffer.Offset);

        SlaveID = buffer.ReadInt32();
        Log.Information(@"[PROTOCOL DEBUG] After SlaveID ({Value}) - Offset: {Offset}", SlaveID, buffer.Offset);

        MatchID = buffer.ReadInt32();
        Log.Information(@"[PROTOCOL DEBUG] After MatchID ({Value}) - Offset: {Offset}", MatchID, buffer.Offset);

        Status = (ChatProtocol.ServerStatus)buffer.ReadInt8();
        Log.Information(@"[PROTOCOL DEBUG] After Status ({Value}) - Offset: {Offset}", Status, buffer.Offset);

        ArrangedMatchType = buffer.ReadInt8();
        Log.Information(@"[PROTOCOL DEBUG] After ArrangedMatchType ({Value}) - Offset: {Offset}", ArrangedMatchType, buffer.Offset);

        // Match Configuration (Protocol >= 59 Format)
        // Read Packed ServerFlags (ushort, 2 bytes)
        Flags = (ChatProtocol.ServerType) buffer.ReadInt16();
        Log.Information(@"[PROTOCOL DEBUG] After ServerFlags (0x{Value:X4}) - Offset: {Offset}", (ushort)Flags, buffer.Offset);

        // Decode Flags Into Individual Properties
        DecodeServerFlags();

        // Read Match Details (Same Structure For Both ACTIVE And Non-ACTIVE States)
        MapName = buffer.ReadString();
        GameName = buffer.ReadString();
        GameMode = buffer.ReadString();
        TeamSize = buffer.ReadInt8();
        MinimumPSR = buffer.ReadInt16();
        MaximumPSR = buffer.ReadInt16();
        CurrentGameTime = buffer.ReadInt32();
        CurrentGamePhase = buffer.ReadInt32();

        // Team And Player Information
        LegionTeamInfo = buffer.ReadString();
        HellbourneTeamInfo = buffer.ReadString();
        Player0Info = buffer.ReadString();
        Player1Info = buffer.ReadString();
        Player2Info = buffer.ReadString();
        Player3Info = buffer.ReadString();
        Player4Info = buffer.ReadString();
        Player5Info = buffer.ReadString();
        Player6Info = buffer.ReadString();
        Player7Info = buffer.ReadString();
        Player8Info = buffer.ReadString();
        Player9Info = buffer.ReadString();

        // Server Performance Metrics
        ServerLoad = buffer.ReadInt32();
        Log.Information(@"[PROTOCOL DEBUG] After ServerLoad ({Value}) - Offset: {Offset}", ServerLoad, buffer.Offset);

        LongServerFrameCount = buffer.ReadInt32();
        Log.Information(@"[PROTOCOL DEBUG] After LongServerFrameCount ({Value}) - Offset: {Offset}", LongServerFrameCount, buffer.Offset);

        FreePhysicalMemory = buffer.ReadInt64();
        Log.Information(@"[PROTOCOL DEBUG] After FreePhysicalMemory ({Value}) - Offset: {Offset}", FreePhysicalMemory, buffer.Offset);

        TotalPhysicalMemory = buffer.ReadInt64();
        Log.Information(@"[PROTOCOL DEBUG] After TotalPhysicalMemory ({Value}) - Offset: {Offset}", TotalPhysicalMemory, buffer.Offset);

        DriveFreeSpace = buffer.ReadInt64();
        Log.Information(@"[PROTOCOL DEBUG] After DriveFreeSpace ({Value}) - Offset: {Offset}", DriveFreeSpace, buffer.Offset);

        DriveTotalSpace = buffer.ReadInt64();
        Log.Information(@"[PROTOCOL DEBUG] After DriveTotalSpace ({Value}) - Offset: {Offset}", DriveTotalSpace, buffer.Offset);

        ServerVersion = buffer.ReadString();
        Log.Information(@"[PROTOCOL DEBUG] After ServerVersion ({Value}) - Offset: {Offset}, Remaining: {Remaining}",
            ServerVersion, buffer.Offset, buffer.Size - buffer.Offset);

        // Per-Client Network Statistics (Optional - Only Present If Server Sends Detailed Stats)
        ClientStats = new List<ClientNetworkStats>();

        long remainingBytes = buffer.Size - buffer.Offset;
        Log.Information(@"[PROTOCOL DEBUG] Before Client Stats Check - Remaining Bytes: {RemainingBytes}", remainingBytes);

        if (buffer.HasRemainingData())
        {
            // Log The Remaining Bytes For Analysis
            byte[] remainingData = buffer.Data[(int)buffer.Offset .. (int)buffer.Size];
            string remainingHex = BitConverter.ToString(remainingData).Replace("-", " ");
            Log.Information(@"[PROTOCOL DEBUG] Remaining Buffer Hex: {HexDump}", remainingHex);
            Log.Information(@"[PROTOCOL DEBUG] Remaining Buffer ASCII: {AsciiView}",
                System.Text.Encoding.ASCII.GetString(remainingData).Replace("\0", "\\0"));

            // CRITICAL FIX: According To HoN Source Code (c_serverchatconnection.cpp Line 516)
            // Protocol Version >= 59 Sends A Hostname String Between LoadAverage And Client Stats
            // This Field Was Missing From The Original Implementation!

            ClientCount = buffer.ReadInt32();
            Log.Information(@"[PROTOCOL DEBUG] After ClientCount ({Value}) - Offset: {Offset}, Remaining: {Remaining}",
                ClientCount, buffer.Offset, buffer.Size - buffer.Offset);

            LoadAverage = buffer.ReadFloat32();
            Log.Information(@"[PROTOCOL DEBUG] After LoadAverage ({Value}) - Offset: {Offset}, Remaining: {Remaining}",
                LoadAverage, buffer.Offset, buffer.Size - buffer.Offset);

            // READ THE MISSING HOSTNAME FIELD!
            // This String Field Was Missing And Causing The Buffer Overflow
            Hostname = buffer.ReadString();
            Log.Information(@"[PROTOCOL DEBUG] After Hostname ({Value}) - Offset: {Offset}, Remaining: {Remaining}",
                Hostname, buffer.Offset, buffer.Size - buffer.Offset);

            Log.Information(@"[PROTOCOL DEBUG] Starting Client Stats Loop - ClientCount: {ClientCount}", ClientCount);

            // Validate ClientCount Is Reasonable (Sanity Check)
            if (ClientCount < 0 || ClientCount > 1000)
            {
                Log.Error(@"[PROTOCOL DEBUG] Invalid ClientCount {Count} - Skipping Client Stats", ClientCount);
                ClientCount = 0;
            }
            else
            {
                for (int clientIndex = 0; clientIndex < ClientCount; clientIndex++)
                {
                    Log.Information(@"[PROTOCOL DEBUG] Reading Client {Index} - Offset: {Offset}, Remaining: {Remaining}",
                        clientIndex, buffer.Offset, buffer.Size - buffer.Offset);

                    // Validate We Have Enough Data For This Client
                    // Protocol >= 59: AccountID (4) + Address (min 1) + 3 Pings (6) + 8 Longs (64) = 75 bytes minimum
                    long bytesNeededMinimum = 4 + 1 + 6 + (8 * 8);
                    if (buffer.Size - buffer.Offset < bytesNeededMinimum)
                    {
                        Log.Error(@"[PROTOCOL DEBUG] Insufficient Data For Client {Index} - Need {Need}, Have {Have}",
                            clientIndex, bytesNeededMinimum, buffer.Size - buffer.Offset);
                        break;
                    }

                    try
                    {
                        ClientStats.Add(new ClientNetworkStats
                        {
                            AccountID = buffer.ReadInt32(),
                            Address = buffer.ReadString(),
                            PingMinimum = buffer.ReadInt16(),
                            PingAverage = buffer.ReadInt16(),
                            PingMaximum = buffer.ReadInt16(),
                            ReliablePacketsSent = buffer.ReadInt64(),
                            ReliablePacketsAcked = buffer.ReadInt64(),
                            ReliablePacketsPeerSent = buffer.ReadInt64(),
                            ReliablePacketsPeerAcked = buffer.ReadInt64(),
                            UnreliablePacketsSent = buffer.ReadInt64(),
                            UnreliablePacketsPeerReceived = buffer.ReadInt64(),
                            UnreliablePacketsPeerSent = buffer.ReadInt64(),
                            UnreliablePacketsReceived = buffer.ReadInt64()
                        });

                        Log.Information(@"[PROTOCOL DEBUG] Finished Client {Index} - Offset: {Offset}", clientIndex, buffer.Offset);
                    }
                    catch (Exception exception)
                    {
                        Log.Error(exception, @"[PROTOCOL DEBUG] Error Reading Client {Index} Stats", clientIndex);
                        break;
                    }
                }
            }
        }
        else
        {
            Log.Information(@"[PROTOCOL DEBUG] No Remaining Data For Client Stats");
            ClientCount = 0;
            LoadAverage = 0.0f;
            Hostname = string.Empty;
        }

        Log.Information(@"[PROTOCOL DEBUG] ServerStatusData Parse Complete - Final Offset: {Offset}", buffer.Offset);
    }

    /// <summary>
    ///     Decode the packed ServerFlags bitfield into individual properties.
    /// </summary>
    private void DecodeServerFlags()
    {
        // Official Status (2-bit field encoded in bits 0-1)
        if (Flags.HasFlag(ChatProtocol.ServerType.OfficialWithStats))
            Official = 2;  // Official With Stats
        else if (Flags.HasFlag(ChatProtocol.ServerType.Official))
            Official = 1;  // Official Without Stats
        else
            Official = 0;  // Unofficial

        // Boolean Flags
        NoLeaver = Flags.HasFlag(ChatProtocol.ServerType.NoLeaver);
        VerifiedOnly = Flags.HasFlag(ChatProtocol.ServerType.VerifiedOnly);
        ServerAccess = Flags.HasFlag(ChatProtocol.ServerType.Private);
        AllHeroes = Flags.HasFlag(ChatProtocol.ServerType.AllHeroes);
        CasualMode = Flags.HasFlag(ChatProtocol.ServerType.Casual);
        Gated = Flags.HasFlag(ChatProtocol.ServerType.Gated);
        ForceRandom = Flags.HasFlag(ChatProtocol.ServerType.ForceRandom);
        AutoBalanced = Flags.HasFlag(ChatProtocol.ServerType.AutoBalance);
        AdvancedOptions = Flags.HasFlag(ChatProtocol.ServerType.AdvancedOptions);
        DevHeroes = Flags.HasFlag(ChatProtocol.ServerType.DevHeroes);
        Hardcore = Flags.HasFlag(ChatProtocol.ServerType.Hardcore);

        // Tier (2-bit field encoded in bits 5-6)
        if (Flags.HasFlag(ChatProtocol.ServerType.TierPro))
            Tier = 2;  // Pro
        else if (Flags.HasFlag(ChatProtocol.ServerType.TierNoobsAllowed))
            Tier = 1;  // Noobs Allowed
        else
            Tier = 0;  // Noobs Only
    }
}

public class ClientNetworkStats
{
    public int AccountID { get; set; }
    public string Address { get; set; } = string.Empty;
    public short PingMinimum { get; set; }
    public short PingAverage { get; set; }
    public short PingMaximum { get; set; }
    public long ReliablePacketsSent { get; set; }
    public long ReliablePacketsAcked { get; set; }
    public long ReliablePacketsPeerSent { get; set; }
    public long ReliablePacketsPeerAcked { get; set; }
    public long UnreliablePacketsSent { get; set; }
    public long UnreliablePacketsPeerReceived { get; set; }
    public long UnreliablePacketsPeerSent { get; set; }
    public long UnreliablePacketsReceived { get; set; }
}
