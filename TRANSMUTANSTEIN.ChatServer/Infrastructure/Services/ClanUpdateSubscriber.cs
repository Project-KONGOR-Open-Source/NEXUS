using System.Globalization;

using MERRICK.DatabaseContext.Extensions;

using TRANSMUTANSTEIN.ChatServer.Domain.Clans;
using TRANSMUTANSTEIN.ChatServer.Internals;

namespace TRANSMUTANSTEIN.ChatServer.Infrastructure.Services;

public partial class ClanUpdateSubscriber : IHostedService
{
    private const string ChannelName = "nexus.clan.updates";
    private readonly ILogger<ClanUpdateSubscriber> _logger;
    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceProvider _serviceProvider;
    private readonly IChatContext _chatContext;
    private ISubscriber? _subscriber;

    public ClanUpdateSubscriber(IConnectionMultiplexer redis, ILogger<ClanUpdateSubscriber> logger,
        IServiceProvider serviceProvider, IChatContext chatContext)
    {
        _redis = redis;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _chatContext = chatContext;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "[ClanUpdateSubscriber] Subscribed to Redis channel: {Channel}")]
    private partial void LogSubscribedToRedisChannel(string channel);

    [LoggerMessage(Level = LogLevel.Information, Message = "[ClanUpdateSubscriber] Received Update [RAW]: '{Payload}'")]
    private partial void LogReceivedUpdateRaw(string payload);

    [LoggerMessage(Level = LogLevel.Error, Message = "[ClanUpdateSubscriber] Invalid payload format. Expected 5 parts, got {Count}. Payload: {Payload}")]
    private partial void LogInvalidPayloadFormat(int count, string payload);

    [LoggerMessage(Level = LogLevel.Error, Message = "[ClanUpdateSubscriber] Parse Error: TargetID '{Val}' is not int.")]
    private partial void LogParseErrorTargetId(string val);

    [LoggerMessage(Level = LogLevel.Error, Message = "[ClanUpdateSubscriber] Parse Error: Rank '{Val}' is not valid ClanTier.")]
    private partial void LogParseErrorRank(string val);

    [LoggerMessage(Level = LogLevel.Error, Message = "[ClanUpdateSubscriber] Parse Error: RequesterID '{Val}' is not int.")]
    private partial void LogParseErrorRequesterId(string val);

    [LoggerMessage(Level = LogLevel.Information, Message = "[ClanUpdateSubscriber] Parsed: Target={Tid} Rank={Rank} Req={Rid} Clan={CName}")]
    private partial void LogParsedUpdate(int tid, ClanTier rank, int rid, string cName);

    [LoggerMessage(Level = LogLevel.Error, Message = "[ClanUpdateSubscriber] CRITICAL Error processing message.")]
    private partial void LogCriticalErrorProcessingMessage(Exception ex);

    [LoggerMessage(Level = LogLevel.Information, Message = "[ClanUpdateSubscriber] Processing Update for Target {TargetID}...")]
    private partial void LogProcessingUpdateForTarget(int targetID);

    [LoggerMessage(Level = LogLevel.Information, Message = "[ClanUpdateSubscriber] Target {TargetID} ({Name}) is ONLINE. Updating Session Rank to {Rank}.")]
    private partial void LogTargetOnlineUpdatingSession(int targetID, string name, ClanTier rank);

    [LoggerMessage(Level = LogLevel.Information, Message = "[ClanUpdateSubscriber] Target removed from clan locally.")]
    private partial void LogTargetRemovedFromClanLocally();

    [LoggerMessage(Level = LogLevel.Information, Message = "[ClanUpdateSubscriber] Sent RankChangeResponse to Target {TargetID}.")]
    private partial void LogSentRankChangeResponse(int targetID);

    [LoggerMessage(Level = LogLevel.Information, Message = "[ClanUpdateSubscriber] Removing target from Channel '{ChannelName}'.")]
    private partial void LogRemovingTargetFromChannel(string channelName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[ClanUpdateSubscriber] Channel 'Clan {ClanName}' not found for removal.")]
    private partial void LogChannelNotFoundForRemoval(string clanName);

    [LoggerMessage(Level = LogLevel.Information, Message = "[ClanUpdateSubscriber] Target {TargetID} is OFFLINE (No active session found).")]
    private partial void LogTargetOffline(int targetID);

    [LoggerMessage(Level = LogLevel.Information, Message = "[ClanUpdateSubscriber] Broadcasting RankChangeResponse to Clan Channel '{ChannelName}'.")]
    private partial void LogBroadcastingRankChangeResponse(string channelName);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _subscriber = _redis.GetSubscriber();
        _subscriber.Subscribe(RedisChannel.Literal(ChannelName), OnMessageReceived);
        LogSubscribedToRedisChannel(ChannelName);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _subscriber?.Unsubscribe(RedisChannel.Literal(ChannelName));
        return Task.CompletedTask;
    }

    private void OnMessageReceived(RedisChannel channel, RedisValue message)
    {
        try
        {
            // Format: "TargetID:Rank:RequesterID:ClanID:ClanName"
            // Example: "12345:Officer:67890:5:MyClan"
            string payload = message.ToString();
            LogReceivedUpdateRaw(payload);

            string[] parts = payload.Split(':');
            if (parts.Length < 5)
            {
                LogInvalidPayloadFormat(parts.Length, payload);
                return;
            }

            if (!int.TryParse(parts[0], out int targetId))
            {
                LogParseErrorTargetId(parts[0]);
                return;
            }

            if (!Enum.TryParse(parts[1], out ClanTier newRank))
            {
                LogParseErrorRank(parts[1]);
                return;
            }

            if (!int.TryParse(parts[2], out int requesterId))
            {
                LogParseErrorRequesterId(parts[2]);
                return;
            }

            string clanName = parts[4];
            LogParsedUpdate(targetId, newRank, requesterId, clanName);

            ProcessUpdate(targetId, newRank, requesterId, clanName);
        }
        catch (Exception ex)
        {
            LogCriticalErrorProcessingMessage(ex);
        }
    }

    private void ProcessUpdate(int targetId, ClanTier newRank, int requesterId, string clanName)
    {
        LogProcessingUpdateForTarget(targetId);

        // 1. Update Target Session if Online
        ChatSession? targetSession = _chatContext.ClientChatSessions.Values.FirstOrDefault(cs => cs.Account.ID == targetId);

        if (targetSession != null)
        {
            LogTargetOnlineUpdatingSession(targetId, targetSession.Account.Name, newRank);

            // Update Local State
            targetSession.Account.ClanTier = newRank;
            if (newRank == ClanTier.None)
            {
                targetSession.Account.Clan = null;
                LogTargetRemovedFromClanLocally();
            }

            // Broadcast Status (Name Color etc)
            targetSession.BroadcastConnectionStatusUpdate(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED);

            // NOTIFY TARGET USER of the change (Kick/Promote)
            // This is crucial because if they are kicked, they are removed from channel below and won't get the broadcast.
            ClanRankChangeResponse selfNotify = new(targetId, newRank, requesterId);
            targetSession.Send(selfNotify);
            LogSentRankChangeResponse(targetId);

            // If removed, leave channel
            if (newRank == ClanTier.None)
            {
                ChatChannel? channel = _chatContext.ChatChannels.Values.FirstOrDefault(c => c.Name == $"Clan {clanName}");
                if (channel != null)
                {
                    LogRemovingTargetFromChannel(channel.Name);
                    channel.Leave(_chatContext, targetSession);
                }
                else
                {
                    LogChannelNotFoundForRemoval(clanName);
                }
            }
        }
        else
        {
            LogTargetOffline(targetId);
        }

        // Setup Status Update Packet (if needed)
        ChatBuffer? statusUpdate = null;
        if (newRank == ClanTier.None && targetSession != null)
        {
            statusUpdate = new ChatBuffer();
            statusUpdate.WriteCommand(ChatProtocol.Command.CHAT_CMD_UPDATE_STATUS);
            statusUpdate.WriteInt32(targetSession.Account.ID);
            statusUpdate.WriteInt8(Convert.ToByte(targetSession.ClientMetadata.LastKnownClientState, CultureInfo.InvariantCulture));
            statusUpdate.WriteInt8(targetSession.Account.GetChatClientFlags());
            statusUpdate.WriteInt32(0); // ClanID 0
            statusUpdate.WriteString(""); // ClanName Empty
            statusUpdate.WriteString(targetSession.Account.GetChatSymbolNoPrefixCode());
            statusUpdate.WriteString(targetSession.Account.GetNameColourNoPrefixCode());
            statusUpdate.WriteString(targetSession.Account.GetIconNoPrefixCode());

            if (targetSession.ClientMetadata.LastKnownClientState is ChatProtocol.ChatClientStatus
                    .CHAT_CLIENT_STATUS_IN_GAME or ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_JOINING_GAME)
            {
                statusUpdate.WriteString(targetSession.ClientMetadata.MatchServerAddress ?? "0.0.0.0:0");
                if (targetSession.ClientMetadata.LastKnownClientState is ChatProtocol.ChatClientStatus
                        .CHAT_CLIENT_STATUS_IN_GAME)
                {
                    statusUpdate.WriteString(""); // Match Name
                    statusUpdate.WriteInt32(targetSession.ClientMetadata.MatchID ?? 0);
                    statusUpdate.WriteBool(false);
                }
            }

            statusUpdate.WriteInt32(targetSession.Account.AscensionLevel);
        }

        // 2. Broadcast to ALL Channels the user is in (to update Tags visually)
        // DISABLED FOR DEBUGGING: Checking if 0x000C packet causes Client Freeze.
        /*
        if (statusUpdate != null && targetSession != null && targetSession.Account != null)
        {
             List<ChatChannel> userChannels = _chatContext.ChatChannels.Values
                 .Where(c => c.Members.ContainsKey(targetSession.Account.Name))
                 .ToList();

             foreach (ChatChannel channel in userChannels)
             {
                  _logger.LogInformation("[ClanUpdateSubscriber] Broadcasting Tag Update (Status) to Channel '{ChannelName}'.", channel.Name);
                  channel.BroadcastMessage(statusUpdate);
             }
        }
        */

        // 3. Specifically handle the Clan Channel (Legacy logic + Kick notification)
        // Even if the user was just removed from it locally (and thus not in userChannels above),
        // we might still need to send the 'RankChange' (Kick) notification to the remaining members.
        ChatChannel? clanChannel = _chatContext.ChatChannels.Values.FirstOrDefault(c => c.Name == $"Clan {clanName}");
        if (clanChannel != null)
        {
            ClanRankChangeResponse response = new(targetId, newRank, requesterId);
            LogBroadcastingRankChangeResponse(clanChannel.Name);

            foreach (ChatChannelMember member in clanChannel.Members.Values)
            {
                member.Session.Send(response);
            }
        }
        // Allowed: Channel might be empty/closed
    }
}