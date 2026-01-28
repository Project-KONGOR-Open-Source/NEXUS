using System.Globalization;

using KONGOR.MasterServer.Services.Requester;
// For PhpSerialization

namespace KONGOR.MasterServer.Handlers.ClientRequester;

public partial class ClanHandler(
    MerrickContext databaseContext,
    IDatabase distributedCache,
    ILogger<ClanHandler> logger) : IClientRequestHandler
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private IDatabase DistributedCache { get; } = distributedCache;
    private ILogger Logger { get; } = logger;

    [LoggerMessage(Level = LogLevel.Information, Message = "[Clans] HandleSetRank RAW Form: {DebugForm}")]
    private partial void LogRawForm(string debugForm);

    [LoggerMessage(Level = LogLevel.Error, Message = "[Clans] HandleSetRank Missing parameters: C:{HasCookie} T:{TargetId} Cl:{ClanId} R:{Rank}")]
    private partial void LogMissingParameters(bool hasCookie, string? targetId, string? clanId, string? rank);

    [LoggerMessage(Level = LogLevel.Error, Message = "[Clans] HandleSetRank Invalid ID: T:{TargetId} Cl:{ClanId}")]
    private partial void LogInvalidId(string? targetId, string? clanId);

    [LoggerMessage(Level = LogLevel.Error, Message = "[Clans] HandleSetRank Invalid Session: {Cookie}")]
    private partial void LogInvalidSession(string cookie);

    [LoggerMessage(Level = LogLevel.Error, Message = "[Clans] HandleSetRank Requester (ID:{RequesterId}) not in specified clan ({ClanId}) or null.")]
    private partial void LogRequesterNotInClan(int? requesterId, int clanId);

    [LoggerMessage(Level = LogLevel.Error, Message = "[Clans] HandleSetRank Requester (Tier:{Tier}) has insufficient permissions.")]
    private partial void LogInsufficientPermissions(ClanTier tier);

    [LoggerMessage(Level = LogLevel.Error, Message = "[Clans] HandleSetRank Target (ID:{TargetId}) not in clan ({ClanId}) or null.")]
    private partial void LogTargetNotInClan(int targetId, int clanId);

    [LoggerMessage(Level = LogLevel.Error, Message = "[Clans] HandleSetRank Officer cannot kick Officer.")]
    private partial void LogOfficerCannotKickOfficer();

    [LoggerMessage(Level = LogLevel.Information, Message = "[Clans] Account {RequesterName} kicked {TargetName} from Clan {ClanId}.")]
    private partial void LogKickedMember(string requesterName, string targetName, int clanId);

    [LoggerMessage(Level = LogLevel.Error, Message = "[Clans] HandleSetRank Unknown Rank: {Rank}")]
    private partial void LogUnknownRank(string rank);

    [LoggerMessage(Level = LogLevel.Error, Message = "[Clans] HandleSetRank Only Leader can promote to Leader.")]
    private partial void LogOnlyLeaderCanPromoteToLeader();

    [LoggerMessage(Level = LogLevel.Information, Message = "[Clans] {RequesterName} transferred Leadership to {TargetName}.")]
    private partial void LogLeadershipTransferred(string requesterName, string targetName);

    [LoggerMessage(Level = LogLevel.Error, Message = "[Clans] HandleSetRank Officer cannot promote Officer.")]
    private partial void LogOfficerCannotPromoteOfficer();

    [LoggerMessage(Level = LogLevel.Information, Message = "[Clans] {RequesterName} promoted {TargetName} to Officer.")]
    private partial void LogPromotedToOfficer(string requesterName, string targetName);

    [LoggerMessage(Level = LogLevel.Error, Message = "[Clans] HandleSetRank Cannot demote higher/equal rank. Req:{RequesterTier} Tgt:{TargetTier}")]
    private partial void LogCannotDemote(ClanTier requesterTier, ClanTier targetTier);

    [LoggerMessage(Level = LogLevel.Information, Message = "[Clans] {RequesterName} demoted {TargetName} to Member.")]
    private partial void LogDemotedToMember(string requesterName, string targetName);

    [LoggerMessage(Level = LogLevel.Information, Message = "[Clans] Published Redis Update: {Payload}")]
    private partial void LogRedisUpdate(string payload);

    [LoggerMessage(Level = LogLevel.Information, Message = "[Clans] set_rank Response: {Response}")]
    private partial void LogResponse(string response);

    public async Task<IActionResult> HandleRequestAsync(HttpContext context)
    {
        HttpRequest Request = context.Request;

        string debugForm = string.Join(", ", Request.Form.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        LogRawForm(debugForm);

        string? cookie = ClientRequestHelper.GetCookie(Request);
        string? targetIdStr = Request.Form["target_id"];
        string? clanIdStr = Request.Form["clan_id"];
        string? rankStr = Request.Form["rank"];

        if (cookie == null || targetIdStr == null || clanIdStr == null || rankStr == null)
        {
            LogMissingParameters(cookie != null, targetIdStr, clanIdStr, rankStr);
            return new BadRequestObjectResult("Missing parameters");
        }

        if (!int.TryParse(targetIdStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int targetId) ||
            !int.TryParse(clanIdStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int clanId))
        {
            LogInvalidId(targetIdStr, clanIdStr);
            return new BadRequestObjectResult("Invalid ID parameters");
        }

        (bool isValid, string? accountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);
        if (!isValid || accountName == null)
        {
            LogInvalidSession(cookie);
            return new UnauthorizedObjectResult("Invalid Session");
        }

        Account? requester = await MerrickContext.Accounts
            .Include(a => a.Clan)
            .FirstOrDefaultAsync(a => a.Name == accountName);

        if (requester == null || requester.Clan?.ID != clanId)
        {
            LogRequesterNotInClan(requester?.ID, clanId);
            return new BadRequestObjectResult("Requester not in specified clan");
        }

        if (requester.ClanTier < ClanTier.Officer)
        {
            LogInsufficientPermissions(requester.ClanTier);
            return new BadRequestObjectResult("Insufficient Permissions");
        }

        Account? target = await MerrickContext.Accounts
            .Include(a => a.Clan)
            .FirstOrDefaultAsync(a => a.ID == targetId);

        if (target == null || target.Clan?.ID != clanId)
        {
            LogTargetNotInClan(targetId, clanId);
            return new BadRequestObjectResult("Target not in clan");
        }

        bool success = false;

        rankStr = rankStr.Trim();

        if (rankStr.Equals("Remove", StringComparison.OrdinalIgnoreCase))
        {
            if (requester.ClanTier <= target.ClanTier &&
                requester.ID != target.ID)
            {
                if (requester.ClanTier == ClanTier.Officer && target.ClanTier == ClanTier.Officer)
                {
                    LogOfficerCannotKickOfficer();
                    return new BadRequestObjectResult("Officer cannot kick Officer");
                }
            }

            target.Clan = null;
            target.ClanTier = ClanTier.None;
            success = true;
            LogKickedMember(requester.Name, target.Name, clanId);
        }
        else
        {
            ClanTier newTier;
            if (rankStr.Equals("Officer", StringComparison.OrdinalIgnoreCase))
            {
                newTier = ClanTier.Officer;
            }
            else if (rankStr.Equals("Member", StringComparison.OrdinalIgnoreCase))
            {
                newTier = ClanTier.Member;
            }
            else if (rankStr.Equals("Leader", StringComparison.OrdinalIgnoreCase))
            {
                newTier = ClanTier.Leader;
            }
            else
            {
                LogUnknownRank(rankStr);
                return new BadRequestObjectResult($"Unknown Rank: {rankStr}");
            }

            if (newTier == ClanTier.Leader)
            {
                if (requester.ClanTier != ClanTier.Leader)
                {
                    LogOnlyLeaderCanPromoteToLeader();
                    return new BadRequestObjectResult("Only Leader can promote to Leader");
                }

                requester.ClanTier = ClanTier.Officer;
                target.ClanTier = ClanTier.Leader;
                success = true;
                LogLeadershipTransferred(requester.Name, target.Name);
            }
            else if (newTier == ClanTier.Officer)
            {
                if (requester.ClanTier < ClanTier.Leader && target.ClanTier == ClanTier.Officer)
                {
                    LogOfficerCannotPromoteOfficer();
                    return new BadRequestObjectResult("Officer cannot promote Officer");
                }

                target.ClanTier = ClanTier.Officer;
                success = true;
                LogPromotedToOfficer(requester.Name, target.Name);
            }
            else if (newTier == ClanTier.Member)
            {
                if (requester.ClanTier <= target.ClanTier && requester.ID != target.ID)
                {
                    LogCannotDemote(requester.ClanTier, target.ClanTier);
                    return new BadRequestObjectResult("Cannot demote higher/equal rank");
                }

                target.ClanTier = ClanTier.Member;
                success = true;
                LogDemotedToMember(requester.Name, target.Name);
            }
        }

        if (success)
        {
            await MerrickContext.SaveChangesAsync();

            string key1 = Random.Shared.Next(1000000, 9999999).ToString(CultureInfo.InvariantCulture);

            string redisPayload = $"{targetId}:{target.ClanTier}:{requester.ID}:{clanId}:{requester.Clan.Name}";
            await DistributedCache.PublishAsync(RedisChannel.Literal("nexus.clan.updates"), redisPayload);
            LogRedisUpdate(redisPayload);

            Dictionary<string, object> response = new()
            {
                { "set_rank", "OK" }, { "notification", new Dictionary<string, int> { { key1, requester.ID } } }
            };

            if (rankStr.Equals("Remove", StringComparison.OrdinalIgnoreCase))
            {
                response.Add("kick", "OK");
                response.Add("remove_member", "OK");
            }

            string serialized = PhpSerialization.Serialize(response);
            LogResponse(serialized);

            return new OkObjectResult(serialized);
        }

        return new BadRequestObjectResult("Action failed");
    }
}
