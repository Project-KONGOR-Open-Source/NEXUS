namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    private async Task<IActionResult> RemoveFriend()
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        string? friendIDString = Request.Form["buddy_id"];

        if (friendIDString is null)
            return BadRequest(@"Missing Value For Form Parameter ""buddy_id""");

        if (int.TryParse(friendIDString, out int friendID).Equals(false))
            return BadRequest(@"Invalid Value For Form Parameter ""buddy_id""");

        // Validate Session Cookie And Get Account Name
        (bool isValid, string? accountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);

        if (isValid.Equals(false) || accountName is null)
        {
            Logger.LogWarning($@"IP Address ""{Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN"}"" Attempted To Remove Friend With Invalid Cookie ""{cookie}""");

            return Unauthorized($@"Unrecognised Cookie ""{cookie}""");
        }

        // Load Requester's Account With Friends List
        Account requesterAccount = await MerrickContext.Accounts
            .Include(account => account.FriendedPeers)
            .SingleAsync(account => account.Name.Equals(accountName));

        // Find Requester Account's Friend To Remove
        FriendedPeer? friendToRemoveFromRequesterAccount = requesterAccount.FriendedPeers
            .SingleOrDefault(friend => friend.ID == friendID);

        // Requester Account Does Not Have Target As A Friend
        if (friendToRemoveFromRequesterAccount is null)
        {
            Logger.LogError(@"[BUG] Account ""{AccountName}"" (ID: {AccountID}) Attempted To Remove Non-Existent Friend ""{FriendName}""",
                requesterAccount.Name, requesterAccount.ID, accountName);

            // Return Success Even If Friend Not Found, For Idempotency
            return Ok(PhpSerialization.Serialize(new Dictionary<string, string> { { "remove_buddy", "OK" } }));
        }

        // Remove Requester Account's Friend
        requesterAccount.FriendedPeers.Remove(friendToRemoveFromRequesterAccount);

        // Load Target Account With Friends List
        Account targetAccount = await MerrickContext.Accounts
            .Include(account => account.FriendedPeers)
            .SingleAsync(account => account.ID == friendToRemoveFromRequesterAccount.ID);

        // Find Target Account's Friend To Remove
        FriendedPeer? friendToRemoveFromTargetAccount = targetAccount.FriendedPeers
            .SingleOrDefault(friend => friend.ID == requesterAccount.ID);

        // Target Account Does Not Have Requester As A Friend
        if (friendToRemoveFromTargetAccount is null)
        {
            Logger.LogError(@"[BUG] Account ""{AccountName}"" (ID: {AccountID}) Attempted To Remove Friend ""{FriendName}"", But Target Account Does Not Have Them As A Friend",
                targetAccount.Name, targetAccount.ID, requesterAccount.Name);

            // Return Success Even If Friend Not Found, For Idempotency
            return Ok(PhpSerialization.Serialize(new Dictionary<string, string> { { "remove_buddy", "OK" } }));
        }

        // Remove Target Account's Friend
        targetAccount.FriendedPeers.Remove(friendToRemoveFromTargetAccount);

        // Persist Changes To Database
        await MerrickContext.SaveChangesAsync();

        // If Notification Data Is Present In The Response, The Game Client Then Sends A CHAT_CMD_NOTIFY_BUDDY_REMOVE Request To The Chat Server
        Dictionary<string, object> response = new ()
        {
            { "remove_buddy", "OK" },
            { "notification", new Dictionary<string, int>
                {
                    { $"{Guid.CreateVersion7().GetDeterministicInt32Hash()}", requesterAccount.ID },  // ID Of The Notification Sent To The Friend Removal Requester
                    { $"{Guid.CreateVersion7().GetDeterministicInt32Hash()}", targetAccount.ID }      // ID Of The Notification Sent To The Removed Friend
                }
            }
        };

        return Ok(PhpSerialization.Serialize(response));

        // NOTE: There Is A Bug In The Game Client Where It Fails To Remove The Last Friend In The Friends List Without Performing A Logout/Login Cycle
    }
}
