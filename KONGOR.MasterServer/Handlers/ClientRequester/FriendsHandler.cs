using System.Globalization;

using KONGOR.MasterServer.Services.Requester;
// For PhpSerialization

namespace KONGOR.MasterServer.Handlers.ClientRequester;

public partial class FriendsHandler(
    MerrickContext databaseContext,
    IDatabase distributedCache,
    ILogger<FriendsHandler> logger) : IClientRequestHandler
{
    private MerrickContext MerrickContext { get; } = databaseContext;
    private IDatabase DistributedCache { get; } = distributedCache;
    private ILogger Logger { get; } = logger;

    [LoggerMessage(Level = LogLevel.Warning, Message = "[FriendsHandler] Unknown function '{FunctionName}'.")]
    private partial void LogUnknownFunction(string functionName);

    [LoggerMessage(Level = LogLevel.Information, Message = "[FriendsHandler] RemoveFriend: BuddyID={BuddyId}")]
    private partial void LogRemoveFriend(string buddyId);

    [LoggerMessage(Level = LogLevel.Information, Message = "[FriendsHandler] Removed friendship between {AccountName} and {BuddyName}")]
    private partial void LogFriendshipRemoved(string accountName, string buddyName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[FriendsHandler] Friendship not found between {AccountName} and {BuddyName}")]
    private partial void LogFriendshipNotFound(string accountName, string buddyName);

    public async Task<IActionResult> HandleRequestAsync(HttpContext context)
    {
        HttpRequest Request = context.Request;
        string? functionName = Request.Query["f"].FirstOrDefault() ?? Request.Form["f"].FirstOrDefault();

        // Dispatch based on function name
        if (functionName == "remove_buddy")
        {
            return await HandleRemoveFriend(context, Request);
        }

        LogUnknownFunction(functionName ?? "NULL");
        return new BadRequestObjectResult("Unknown function");
    }

    private async Task<IActionResult> HandleRemoveFriend(HttpContext context, HttpRequest Request)
    {
        string? cookie = ClientRequestHelper.GetCookie(Request);
        string? buddyIdStr = Request.Form["buddy_id"];

        LogRemoveFriend(buddyIdStr ?? "NULL");

        if (cookie == null || buddyIdStr == null)
        {
            return new BadRequestObjectResult("Missing parameters");
        }

        if (!int.TryParse(buddyIdStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int buddyId))
        {
            return new BadRequestObjectResult("Invalid Buddy ID");
        }

        string? accountName = context.Items["SessionAccountName"] as string
                              ?? await DistributedCache.GetAccountNameForSessionCookie(cookie);

        if (accountName is null)
        {
            return new UnauthorizedObjectResult("Session Not Found");
        }

        Account? account = await MerrickContext.Accounts
            .Include(a => a.FriendedPeers)
            .FirstOrDefaultAsync(a => a.Name == accountName);

        if (account is null)
        {
            return new NotFoundObjectResult("Account Not Found");
        }

        // Load Buddy with their FriendedPeers to ensure bidirectional removal
        Account? buddy = await MerrickContext.Accounts
            .Include(a => a.FriendedPeers)
            .FirstOrDefaultAsync(a => a.ID == buddyId);

        if (buddy is null)
        {
            return new NotFoundObjectResult("Buddy Not Found");
        }

        bool removed = false;

        // Remove Buddy from Account's list
        FriendedPeer? peerInAccount = account.FriendedPeers.FirstOrDefault(p => p.ID == buddyId);
        if (peerInAccount is not null)
        {
            account.FriendedPeers.Remove(peerInAccount);
            removed = true;
        }

        // Remove Account from Buddy's list (Bidirectional)
        FriendedPeer? peerInBuddy = buddy.FriendedPeers.FirstOrDefault(p => p.ID == account.ID);
        if (peerInBuddy is not null)
        {
            buddy.FriendedPeers.Remove(peerInBuddy);
            removed = true;
        }

        if (removed)
        {
            await MerrickContext.SaveChangesAsync();
            LogFriendshipRemoved(account.Name, buddy.Name);

            // Construct notification payload (random key -> buddy ID)
            // This mirrors legacy behavior where the client receives a notification trigger
            string notificationKey = Random.Shared.Next(1000000, 9999999).ToString(CultureInfo.InvariantCulture);

            Dictionary<string, object> response = new()
            {
                { "remove_buddy", "OK" },
                { "notification", new Dictionary<string, int> { { notificationKey, buddy.ID } } }
            };

            return new OkObjectResult(PhpSerialization.Serialize(response));
        }
        else
        {
            LogFriendshipNotFound(account.Name, buddy.Name);
            // Return OK even if nothing removed, to match legacy "idempotent" behavior often found
            return new OkObjectResult(PhpSerialization.Serialize(new { remove_buddy = "OK" }));
        }
    }
}
