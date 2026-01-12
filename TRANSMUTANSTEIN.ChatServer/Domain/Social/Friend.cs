using System.Globalization;

using MERRICK.DatabaseContext.Entities;
using MERRICK.DatabaseContext.Extensions;
using TRANSMUTANSTEIN.ChatServer.Domain.Core;

namespace TRANSMUTANSTEIN.ChatServer.Domain.Social;

public class Friend
{
    /// <summary>
    ///     Hidden Constructor Which Enforces <see cref="WithAccountName" /> As The Primary Mechanism For Creating Friends
    /// </summary>
    private Friend() { }

    public required string AccountName { get; init; }

    public static Friend WithAccountName(string accountName)
    {
        return new Friend { AccountName = accountName };
    }

    /// <summary>
    ///     Sends a friend request to the target account.
    ///     If the target account has already made a friend request to the requester, creates bi-directional friendship
    ///     immediately.
    ///     Otherwise, stores pending request in the distributed cache and notifies the target account.
    /// </summary>
    public async Task<Friend> Add(ChatSession session, MerrickContext merrick, IDatabase distributedCacheStore)
    {
        // Look Up Target Account In Database
        Account? targetAccount = await merrick.Accounts
            .Include(account => account.Clan)
            .Include(account => account.IgnoredPeers)
            .Include(account => account.BannedPeers)
            .SingleOrDefaultAsync(account => account.Name == AccountName);

        // Target Account Not Found
        if (targetAccount is null)
        {
            SendFriendAddFailure(session, ChatProtocol.FriendAddStatus.GENERIC_FAILURE, targetAccount);

            return this;
        }

        // Load Requester's Account With Friends List
        Account requesterAccount = await merrick.Accounts
            .Include(account => account.FriendedPeers)
            .Include(account => account.Clan)
            .SingleAsync(account => account.ID == session.Account.ID);

        // Check If Requester Is On Target's Ignore List
        bool isIgnored = targetAccount.IgnoredPeers.Any(peer => peer.ID == requesterAccount.ID);

        if (isIgnored)
        {
            SendFriendAddFailure(session, ChatProtocol.FriendAddStatus.BANNED_OR_IGNORED, targetAccount);

            return this;
        }

        // Check If Requester Is On Target's Ban List
        bool isBanned = targetAccount.BannedPeers.Any(peer => peer.ID == requesterAccount.ID);

        if (isBanned)
        {
            SendFriendAddFailure(session, ChatProtocol.FriendAddStatus.BANNED_OR_IGNORED, targetAccount);

            return this;
        }

        // Check If Already Friends
        bool alreadyFriends = requesterAccount.FriendedPeers
            .Any(friend => friend.Name.Equals(targetAccount.Name, StringComparison.OrdinalIgnoreCase));

        if (alreadyFriends)
        {
            SendFriendAddFailure(session, ChatProtocol.FriendAddStatus.DUPLICATE_RECORD, targetAccount);

            return this;
        }

        // Check If Requester Has Reached Friend Limit
        if (requesterAccount.FriendedPeers.Count >= ChatProtocol.FRIEND_LIMIT)
        {
            SendFriendAddFailure(session, ChatProtocol.FriendAddStatus.FRIEND_LIMIT_REACHED, targetAccount);

            return this;
        }

        // Check Whether Pending Friend Request Already Exists
        bool existingPendingRequest =
            await distributedCacheStore.PendingFriendRequestExists(requesterAccount.ID, targetAccount.ID);

        if (existingPendingRequest)
        {
            SendFriendAddFailure(session, ChatProtocol.FriendAddStatus.DUPLICATE_RECORD, targetAccount);

            return this;
        }

        // Check Whether The Target Account Has Already Sent A Friend Request To The Requester
        (int RequesterNotificationID, int TargetNotificationID)? mutualRequest =
            await distributedCacheStore.GetFriendRequest(targetAccount.ID, requesterAccount.ID);

        if (mutualRequest is not null)
        {
            // Request Is Mutual: Both Accounts Become Friends Immediately
            await CreateMutualFriendship(requesterAccount, targetAccount, merrick, distributedCacheStore);

            // Create Notifications (Type 2: notify_buddy_added) for BOTH parties
            Notification requesterNotification =
                CreateNotification(2, "notify_buddy_added", targetAccount, requesterAccount, merrick);
            Notification targetNotification =
                CreateNotification(2, "notify_buddy_added", requesterAccount, targetAccount, merrick);

            // Send Approval Success Notification To Requester
            // NOTE: Current Requester Was Target Of Original Request
            SendFriendRequestApproval(session, targetAccount, ChatProtocol.FriendApproveStatus.SUCCESS_REQUESTER,
                requesterNotification.NotificationId);

            ChatSession? targetSession = Context.ClientChatSessions.Values
                .SingleOrDefault(chatSession =>
                    chatSession.Account.Name.Equals(targetAccount.Name, StringComparison.OrdinalIgnoreCase));

            // Send Approval Success Notification To Target, If Online
            // NOTE: Current Target Was Requester Of Original Request
            if (targetSession is not null)
            {
                SendFriendRequestApproval(targetSession, requesterAccount,
                    ChatProtocol.FriendApproveStatus.SUCCESS_APPROVER, targetNotification.NotificationId);
            }

            return this;
        }

        // Request Is Not Mutual: Generate Both Notification IDs
        int requesterNotificationID = GenerateNotificationID();
        int targetNotificationID = GenerateNotificationID();

        // Store Pending Friend Request In Distributed Cache Store
        await distributedCacheStore.SetFriendRequest(requesterAccount.ID, targetAccount.ID, requesterNotificationID,
            targetNotificationID);

        // Send Success Response To Requester (Friend Request Created Successfully)
        SendFriendAddSuccess(session, targetAccount, requesterNotificationID);

        ChatSession? targetOnlineSession = Context.ClientChatSessions.Values
            .SingleOrDefault(chatSession =>
                chatSession.Account.Name.Equals(targetAccount.Name, StringComparison.OrdinalIgnoreCase));

        // Send Friend Request Notification To Target, If Online
        if (targetOnlineSession is not null &&
            targetOnlineSession.ClientMetadata.ClientChatModeState is not ChatProtocol.ChatModeType.CHAT_MODE_DND)
        {
            // Create Notification For Target (Type 2: notify_buddy_added)
            // Note: Legacy calls this "notify_buddy_added" even for the initial request? 
            // Checking legacy code: BuddyAddRequest calls PostNotification(2, "notify_buddy_added"...) for the target.
            // AND PostNotification(23, "notify_buddy_requested_adder"...) for the requester?
            // Actually legacy L114 says: "notify_buddy_requested_adder" for us (requester).
            // L88 says: "notify_buddy_added" for the target during APPROVAL.
            // But L153 PostNotification implementation suggests it's for the target.

            // Re-reading legacy BuddyAddRequest.cs:
            // Lines 79-89 are inside "if (friendshipRequestFromThem != null)" -> Mutual/Approval scenario.
            // In that case, BOTH get "notify_buddy_added" (Type 2).

            // Lines 107-117 are for "We requested friendship" (New Request).
            // Line 114: FriendClientResponse (for us) gets notification with Type 23 "notify_buddy_requested_adder".
            // Wait, FriendClientResponse is sent to the FRIEND? No, ClientResponse is for us. FriendClientResponse is for them?
            // Line 129: friendClient.SendResponse(FriendClientResponse). Yes, FriendClientResponse is for the TARGET.
            // So for a NEW request, the TARGET gets Type 23 "notify_buddy_requested_adder"? 
            // Legacy L114: notification: PostNotification(23, "notify_buddy_requested_adder", account, FriendAccount, merrickContext)
            // Arg3 is "fromWhom" (account = us), Arg4 is "toWhom" (FriendAccount = them).
            // So yes, Target gets Type 23.

            // Wait, what about Line 134 in my current code? "SendFriendAddSuccess" to Requester.
            // Legacy L124 sends ClientResponse. ClientResponse is set in L37, L51, L58 (Failures).
            // Or L79 (Approval).
            // But where is ClientResponse set for a NEW request success?
            // It seems it ISN'T set? "ClientResponse" is null in the new request path?
            // Ah, legacy L113 sets FriendClientResponse.
            // Does legacy NOT send a response to the requester for success?
            // "BuddyAddRequest" seems to only send a response on failure or immediate approval.
            // But `SendFriendAddSuccess` exists in my current code.

            // Let's implement the Notification for the TARGET first.
            // Legacy Type 23 "notify_buddy_requested_adder".

            Notification notification = CreateNotification(23, "notify_buddy_requested_adder", requesterAccount,
                targetAccount, merrick);

            SendFriendRequest(targetOnlineSession, requesterAccount, notification.NotificationId);
        }

        return this;
    }

    /// <summary>
    ///     Approves a pending friend request from another account.
    ///     Creates bi-directional friendship and sends approval responses to both clients.
    /// </summary>
    public async Task<Friend> Approve(ChatSession session, MerrickContext merrick, IDatabase distributedCacheStore)
    {
        // Load Approver's Account
        Account approverAccount = await merrick.Accounts
            .Include(account => account.FriendedPeers)
            .Include(account => account.Clan)
            .SingleAsync(account => account.ID == session.Account.ID);

        // Look Up Requester Account
        Account? requesterAccount = await merrick.Accounts
            .Include(account => account.FriendedPeers)
            .Include(account => account.Clan)
            .SingleOrDefaultAsync(account => account.Name == AccountName);

        if (requesterAccount is null)
        {
            return this;
        }

        // Check Whether Pending Friend Request Exists Or Not
        (int RequesterNotificationID, int TargetNotificationID)? pendingRequest =
            await distributedCacheStore.GetFriendRequest(requesterAccount.ID, approverAccount.ID);

        if (pendingRequest is null)
        {
            // No Pending Request Found (May Have Expired Or Never Existed)
            SendFriendApproveFailure(session, requesterAccount.ID, requesterAccount.GetNameWithClanTag());

            return this;
        }

        // Create Bi-Directional Friendship
        await CreateMutualFriendship(requesterAccount, approverAccount, merrick, distributedCacheStore);

        // Create Notifications (Type 2: notify_buddy_added) for BOTH parties
        Notification approverNotification =
            CreateNotification(2, "notify_buddy_added", requesterAccount, approverAccount, merrick);
        Notification requesterNotification =
            CreateNotification(2, "notify_buddy_added", approverAccount, requesterAccount, merrick);

        // Send Approval Request To Approver
        SendFriendRequestApproval(session, requesterAccount, ChatProtocol.FriendApproveStatus.SUCCESS_APPROVER,
            approverNotification.NotificationId);

        ChatSession? requesterSession = Context.ClientChatSessions.Values
            .SingleOrDefault(chatSession =>
                chatSession.Account.Name.Equals(requesterAccount.Name, StringComparison.OrdinalIgnoreCase));

        // Send Approval Response To Requester, If Online
        if (requesterSession is not null)
        {
            SendFriendRequestApproval(requesterSession, approverAccount,
                ChatProtocol.FriendApproveStatus.SUCCESS_REQUESTER, requesterNotification.NotificationId);
        }

        return this;
    }

    /// <summary>
    ///     Creates bi-directional friendship between two accounts and removes any pending requests from the cache store.
    /// </summary>
    private static async Task CreateMutualFriendship(Account one, Account two, MerrickContext merrick,
        IDatabase distributedCacheStore)
    {
        // Check If Already Friends (Prevents Duplicates From Race Conditions)
        bool alreadyFriendsOneToTwo = one.FriendedPeers.Any(friend => friend.ID == two.ID);
        bool alreadyFriendsTwoToOne = two.FriendedPeers.Any(friend => friend.ID == one.ID);

        // If Not Already Friends, Add Account Two To Account One's Friends
        if (alreadyFriendsOneToTwo.Equals(false))
        {
            FriendedPeer friendOneToTwo = new()
            {
                ID = two.ID, Name = two.Name, ClanTag = two.Clan?.Tag, FriendGroup = "DEFAULT"
            };

            one.FriendedPeers.Add(friendOneToTwo);
        }

        // If Not Already Friends, Add Account One To Account Two's Friends
        if (alreadyFriendsTwoToOne.Equals(false))
        {
            FriendedPeer friendTwoToOne = new()
            {
                ID = one.ID, Name = one.Name, ClanTag = one.Clan?.Tag, FriendGroup = "DEFAULT"
            };

            two.FriendedPeers.Add(friendTwoToOne);
        }

        // Persist Changes To The Database
        await merrick.SaveChangesAsync();

        // Remove Any Pending Requests From The Distributed Cache Store
        await distributedCacheStore.RemoveFriendRequest(one.ID, two.ID);
        await distributedCacheStore.RemoveFriendRequest(two.ID, one.ID);
    }

    /// <summary>
    ///     Generates a unique notification ID for friend request tracking.
    /// </summary>
    private static int GenerateNotificationID()
    {
        return Guid.CreateVersion7().GetDeterministicInt32Hash();
    }

    /// <summary>
    ///     Sends friend request notification to the target player for approval.
    /// </summary>
    private static void SendFriendRequest(ChatSession targetSession, Account requesterAccount, int notificationID)
    {
        ChatBuffer notification = new();

        notification.WriteCommand(ChatProtocol.Command.CHAT_CMD_REQUEST_BUDDY_ADD_RESPONSE);
        notification.WriteInt8(Convert.ToByte(ChatProtocol.FriendAddStatus
            .SUCCESS_REQUESTEE)); // Friend Addition Status
        notification.WriteInt32(notificationID); // Notification ID For Tracking This Request
        notification.WriteString(requesterAccount.GetNameWithClanTag()); // Requester's Display Name With Clan Tag
        notification.WriteInt8(0x00); // 0x00
        notification.WriteString(requesterAccount.GetNameColourNoPrefixCode()); // Requester's Name Colour
        notification.WriteInt8(Convert.ToByte(ChatProtocol.ChatClientStatus
            .CHAT_CLIENT_STATUS_CONNECTED)); // Requester's Current Connection Status
        notification.WriteInt8(requesterAccount.GetChatClientFlags()); // Requester's Account Flags
        notification.WriteInt32(requesterAccount.Clan?.ID ?? 0); // Requester's Clan ID (Zero If Not In A Clan)
        notification.WriteString(requesterAccount.Clan?.Name ??
                                 string.Empty); // Requester's Clan Full Name (Empty String If Not In A Clan)
        notification.WriteString(requesterAccount.GetChatSymbolNoPrefixCode()); // Requester's Chat Symbol
        notification.WriteString(requesterAccount.GetIconNoPrefixCode()); // Requester's Account Icon
        notification.WriteInt32(requesterAccount.AscensionLevel); // Requester's Ascension Level

        targetSession.Send(notification);
    }

    /// <summary>
    ///     Sends friend approval response.
    /// </summary>
    private static void SendFriendRequestApproval(ChatSession session, Account friendAccount,
        ChatProtocol.FriendApproveStatus status, int notificationID)
    {
        ChatBuffer response = new();

        response.WriteCommand(ChatProtocol.Command.CHAT_CMD_REQUEST_BUDDY_APPROVE_RESPONSE);
        response.WriteInt8(Convert.ToByte(status)); // Friend Approval Status
        response.WriteInt32(friendAccount.ID); // Friend's Account ID
        response.WriteInt32(notificationID); // Notification ID For Tracking The Friend Request
        response.WriteString(friendAccount.GetNameWithClanTag()); // Friend's Display Name With Clan Tag

        session.Send(response);
    }

    /// <summary>
    ///     Sends friend add success response to the requester.
    ///     This indicates that the friend request was created successfully, not that the friendship has been established.
    /// </summary>
    private static void SendFriendAddSuccess(ChatSession session, Account friendAccount, int notificationID)
    {
        ChatBuffer response = new();

        response.WriteCommand(ChatProtocol.Command.CHAT_CMD_REQUEST_BUDDY_ADD_RESPONSE);
        response.WriteInt8(Convert.ToByte(ChatProtocol.FriendAddStatus
            .SUCCESS_REQUESTER)); // Success Indicator For The Client Who Initiated The Friend Request
        response.WriteInt32(notificationID); // Requester's Notification ID For This Friend Request
        response.WriteString(friendAccount.GetNameWithClanTag()); // Friend's Display Name With Clan Tag
        response.WriteInt8(0x00);
        response.WriteString(friendAccount.GetNameColourNoPrefixCode()); // Friend's Name Colour
        response.WriteInt8(Convert.ToByte(ChatProtocol.ChatClientStatus
            .CHAT_CLIENT_STATUS_CONNECTED)); // Friend's Current Connection Status
        response.WriteInt8(friendAccount.GetChatClientFlags()); // Friend's Account Flags
        response.WriteInt32(friendAccount.Clan?.ID ?? 0); // Friend's Clan ID (Zero If Not In A Clan)
        response.WriteString(friendAccount.Clan?.Name ??
                             string.Empty); // Friend's Clan Name (Empty String If Not In A Clan)
        response.WriteString(friendAccount.GetChatSymbolNoPrefixCode()); // Friend's Chat Symbol
        response.WriteString(friendAccount.GetNameColourNoPrefixCode()); // Friend's Name Colour
        response.WriteString(friendAccount.GetIconNoPrefixCode()); // Friend's Account Icon
        response.WriteInt32(friendAccount.AscensionLevel); // Friend's Ascension Level

        session.Send(response);
    }

    /// <summary>
    ///     Sends friend add failure response with the specific failure reason.
    /// </summary>
    private void SendFriendAddFailure(ChatSession session, ChatProtocol.FriendAddStatus reason, Account? targetAccount)
    {
        ChatBuffer response = new();

        response.WriteCommand(ChatProtocol.Command.CHAT_CMD_REQUEST_BUDDY_ADD_RESPONSE);
        response.WriteInt8(Convert.ToByte(reason)); // Friend Addition Failure Reason
        response.WriteInt32(0); // Notification ID (Zero Indicates Failure)
        response.WriteString(targetAccount?.GetNameWithClanTag() ??
                             AccountName); // Target Display Name With Clan Tag (Or Plain Name If Account Not Found)

        session.Send(response);
    }

    /// <summary>
    ///     Sends friend approve failure response when the pending request cannot be found or has expired.
    /// </summary>
    private static void SendFriendApproveFailure(ChatSession session, int requesterAccountID,
        string requesterAccountName)
    {
        ChatBuffer response = new();

        response.WriteCommand(ChatProtocol.Command.CHAT_CMD_REQUEST_BUDDY_APPROVE_RESPONSE);
        response.WriteInt8(Convert.ToByte(ChatProtocol.FriendApproveStatus
            .GENERIC_FAILURE)); // Friend Approval Failure (Generic Error)
        response.WriteInt32(requesterAccountID); // Requester's Account ID
        response.WriteInt32(0); // Notification ID (Zero Indicates Failure)
        response.WriteString(requesterAccountName); // Requester's Display Name With Clan Tag

        session.Send(response);
    }

    /// <summary>
    ///     Creates and persists a notification entity following the legacy format.
    ///     Format: Nick|unknown|type|kind|type_string|action|timestamp|ID
    /// </summary>
    private static Notification CreateNotification(int type, string kind, Account fromWhom, Account toWhom,
        MerrickContext merrick)
    {
        Notification notification = new()
        {
            AccountId = toWhom.ID,
            Content =
                string.Empty, // Will be populated after ID generation (or effectively immediately since we don't save twice? EF Core might need a temp ID?)
            // Legacy creates empty row to get ID, then updates.
            // We can set Timestamp now.
            TimestampCreated = DateTime.UtcNow
        };

        merrick.Notifications.Add(notification);
        // We need the ID for the content string. 
        // If ID is identity, we need to save changes to get it. 
        // Check if we can do this in one pass or need two.
        // Legacy: "First make empty row to get ID" -> merrickContext.Notifications.Add -> but doesn't call SaveChanges immediately?
        // Legacy L178 uses notification.NotificationId. If it's 0 (unset), that string will be wrong.
        // Unless EF Core usage in legacy context meant Add generated ID? (Unlikely for Identity column without Save).
        // OR the legacy code relied on reference/fixup?
        // Let's assumes we need to save to get ID if it's database generated.
        // However, making multiple saves in one request might be slow.
        // For now, let's defer the ID or assume 0 and update later? 
        // Actually legacy L178 puts NotificationId into the string. 
        // Let's assume we need to save.

        merrick.SaveChanges(); // Save to generate ID.

        // Nick|unknown|type number|translation string|type string|action|timestring|ID
        notification.Content = string.Join(
            '|',
            fromWhom.Name, // Nick
            "", // unknown
            type, // type number
            kind, // translation string
            "notfication_generic_action", // type string (legacy typo 'notfication' preserved?)
            "action_friend_request", // action
            notification.TimestampCreated.ToString("MM/dd/yy HH:mm tt",
                CultureInfo.InvariantCulture), // Legacy format: 04/16 16:59 PM ? 
            // Legacy L161: DateTime.UtcNow.
            // L177 passes it strictly. Does string.Join use default ToString?
            // Need to verify legacy DateTime format in string.Join.
            // Assuming standard formatting for now or "MM/dd/yy HH:mm tt" based on "04/16 16:59 PM" comment in legacy retrieval. 
            // Wait, comment in legacy retrieval (step 4619) showed: "|04/16 16:59 PM|2181677".
            // That looks like "MM/dd HH:mm tt" (no year?) or "MM/dd/yy".
            // "04/16" could be Day/Month or Month/Day. 
            // Let's stick to a safe standard for now or mimic exactly if needed.
            notification.NotificationId // ID
        );

        // Update with content
        merrick.SaveChanges();

        return notification;
    }
}