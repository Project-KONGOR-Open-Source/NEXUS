using StackExchange.Redis;
using TRANSMUTANSTEIN.ChatServer.Domain.Clans;
using TRANSMUTANSTEIN.ChatServer.Domain.Core;
using TRANSMUTANSTEIN.ChatServer.Domain.Matchmaking;

namespace TRANSMUTANSTEIN.ChatServer.Infrastructure.Services
{
    public class ClanUpdateSubscriber : IHostedService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<ClanUpdateSubscriber> _logger;
        private readonly IServiceProvider _serviceProvider;
        private ISubscriber? _subscriber;
        private const string ChannelName = "nexus.clan.updates";

        public ClanUpdateSubscriber(IConnectionMultiplexer redis, ILogger<ClanUpdateSubscriber> logger, IServiceProvider serviceProvider)
        {
            _redis = redis;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _subscriber = _redis.GetSubscriber();
            _subscriber.Subscribe(RedisChannel.Literal(ChannelName), OnMessageReceived);
            _logger.LogInformation("[ClanUpdateSubscriber] Subscribed to Redis channel: {Channel}", ChannelName);
            return Task.CompletedTask;
        }

        private void OnMessageReceived(RedisChannel channel, RedisValue message)
        {
            try
            {
                // Format: "TargetID:Rank:RequesterID:ClanID:ClanName"
                // Example: "12345:Officer:67890:5:MyClan"
                string payload = message.ToString();
                _logger.LogInformation("[ClanUpdateSubscriber] Received Update [RAW]: '{Payload}'", payload);

                string[] parts = payload.Split(':');
                if (parts.Length < 5) 
                {
                    _logger.LogError("[ClanUpdateSubscriber] Invalid payload format. Expected 5 parts, got {Count}. Payload: {Payload}", parts.Length, payload);
                    return;
                }

                if (!int.TryParse(parts[0], out int targetId)) { _logger.LogError("[ClanUpdateSubscriber] Parse Error: TargetID '{Val}' is not int.", parts[0]); return; }
                if (!Enum.TryParse(parts[1], out ClanTier newRank)) { _logger.LogError("[ClanUpdateSubscriber] Parse Error: Rank '{Val}' is not valid ClanTier.", parts[1]); return; }
                if (!int.TryParse(parts[2], out int requesterId)) { _logger.LogError("[ClanUpdateSubscriber] Parse Error: RequesterID '{Val}' is not int.", parts[2]); return; }
                
                string clanName = parts[4];
                _logger.LogInformation("[ClanUpdateSubscriber] Parsed: Target={Tid} Rank={Rank} Req={Rid} Clan={CName}", targetId, newRank, requesterId, clanName);

                ProcessUpdate(targetId, newRank, requesterId, clanName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ClanUpdateSubscriber] CRITICAL Error processing message.");
            }
        }

        private void ProcessUpdate(int targetId, ClanTier newRank, int requesterId, string clanName)
        {
            _logger.LogInformation("[ClanUpdateSubscriber] Processing Update for Target {TargetID}...", targetId);

            // 1. Update Target Session if Online
            ChatSession? targetSession = Context.ClientChatSessions.Values.FirstOrDefault(cs => cs.Account?.ID == targetId);

            if (targetSession != null && targetSession.Account != null)
            {
                _logger.LogInformation("[ClanUpdateSubscriber] Target {TargetID} ({Name}) is ONLINE. Updating Session Rank to {Rank}.", targetId, targetSession.Account.Name, newRank);
                
                // Update Local State
                targetSession.Account.ClanTier = newRank;
                if (newRank == ClanTier.None)
                {
                    targetSession.Account.Clan = null;
                    _logger.LogInformation("[ClanUpdateSubscriber] Target removed from clan locally.");
                }
                
                // Broadcast Status (Name Color etc)
                targetSession.BroadcastConnectionStatusUpdate(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED);
                
                // NOTIFY TARGET USER of the change (Kick/Promote)
                // This is crucial because if they are kicked, they are removed from channel below and won't get the broadcast.
                ClanRankChangeResponse selfNotify = new(targetId, newRank, requesterId);
                targetSession.Send(selfNotify);
                _logger.LogInformation("[ClanUpdateSubscriber] Sent RankChangeResponse to Target {TargetID}.", targetId);
                
                // If removed, leave channel
                if (newRank == ClanTier.None)
                {
                    ChatChannel? channel = Context.ChatChannels.Values.FirstOrDefault(c => c.Name == $"Clan {clanName}");
                    if (channel != null)
                    {
                         _logger.LogInformation("[ClanUpdateSubscriber] Removing target from Channel '{ChannelName}'.", channel.Name);
                         channel.Leave(targetSession);
                    }
                    else
                    {
                         _logger.LogWarning("[ClanUpdateSubscriber] Channel 'Clan {ClanName}' not found for removal.", clanName);
                    }
                }
            }
            else
            {
                _logger.LogInformation("[ClanUpdateSubscriber] Target {TargetID} is OFFLINE (No active session found).", targetId);
            }

            // 2. Broadcast to Clan Channel (Always, unless disbanded?)
            // We need to look up the channel.
            ChatChannel? clanChannel = Context.ChatChannels.Values.FirstOrDefault(c => c.Name == $"Clan {clanName}");
            if (clanChannel != null)
            {
                 // Check if Target is ALREADY updated in the DB? Yes, MasterServer did it.
                 // We just need to notify clients.
                 
                 ClanRankChangeResponse response = new(targetId, newRank, requesterId);
                 _logger.LogInformation("[ClanUpdateSubscriber] Broadcasting RankChangeResponse to Channel '{ChannelName}' with {Count} members.", clanChannel.Name, clanChannel.Members.Count);
                 
                 // If target was removed, we also need to force a Status Update to all members so the clan tag disappears visually.
                 ChatBuffer? statusUpdate = null;
                 if (newRank == ClanTier.None && targetSession != null && targetSession.Account != null)
                 {
                      statusUpdate = new ChatBuffer();
                      statusUpdate.WriteCommand(ChatProtocol.Command.CHAT_CMD_UPDATE_STATUS);
                      statusUpdate.WriteInt32(targetSession.Account.ID);
                      statusUpdate.WriteInt8(Convert.ToByte(targetSession.ClientMetadata.LastKnownClientState));
                      statusUpdate.WriteInt8(targetSession.Account.GetChatClientFlags());
                      statusUpdate.WriteInt32(0); // ClanID 0
                      statusUpdate.WriteString(""); // ClanName Empty
                      statusUpdate.WriteString(targetSession.Account.ChatSymbolNoPrefixCode);
                      statusUpdate.WriteString(targetSession.Account.NameColourNoPrefixCode);
                      statusUpdate.WriteString(targetSession.Account.IconNoPrefixCode);
                      // Match Info (Simplified fallback) - If in game, this might be missing IP/Port logic
                      // But for "Tag Removal" just sending 0.0.0.0:0 is usually fine or we copy logic.
                      // Let's use 0.0.0.0:0 for safety unless we want to copy full logic.
                      // Safe minimal implementation:
                      if (targetSession.ClientMetadata.LastKnownClientState is ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_IN_GAME or ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_JOINING_GAME)
                      {
                           statusUpdate.WriteString(targetSession.ClientMetadata.MatchServerAddress ?? "0.0.0.0:0");
                           if (targetSession.ClientMetadata.LastKnownClientState is ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_IN_GAME)
                           {
                                statusUpdate.WriteString(""); // Match Name
                                statusUpdate.WriteInt32(targetSession.ClientMetadata.MatchID ?? 0);
                                statusUpdate.WriteBool(false);
                           }
                      }
                      
                      statusUpdate.WriteInt32(targetSession.Account.AscensionLevel);
                 }

                 foreach (ChatChannelMember member in clanChannel.Members.Values)
                 {
                     member.Session.Send(response);
                     if (statusUpdate != null)
                     {
                         member.Session.Send(statusUpdate);
                     }
                 }
            }
            else
            {
                 _logger.LogWarning("[ClanUpdateSubscriber] Channel 'Clan {ClanName}' not active (null). No broadcast sent to clanmates.", clanName);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _subscriber?.Unsubscribe(RedisChannel.Literal(ChannelName));
            return Task.CompletedTask;
        }
    }
}
