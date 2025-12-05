namespace TRANSMUTANSTEIN.ChatServer.Domain.Social;

public class Friend
{
    public required string AccountName { get; init; }

    /// <summary>
    ///     Hidden Constructor Which Enforces <see cref="WithAccountName"/> As The Primary Mechanism For Creating Friends
    /// </summary>
    private Friend() { }

    public static Friend WithAccountName(string accountName)
        => new () { AccountName = accountName };

    /// <summary>
    ///     Sends a friend request to the target account.
    ///     If the target account has already made a friend request to the requester, creates bi-directional friendship immediately.
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
            SendFriendAddFailure(session, ChatProtocol.FriendAddStatus.GENERIC_FAILURE);

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
            SendFriendAddFailure(session, ChatProtocol.FriendAddStatus.BANNED_OR_IGNORED);

            return this;
        }

        // Check If Requester Is On Target's Ban List
        bool isBanned = targetAccount.BannedPeers.Any(peer => peer.ID == requesterAccount.ID);

        if (isBanned)
        {
            SendFriendAddFailure(session, ChatProtocol.FriendAddStatus.BANNED_OR_IGNORED);

            return this;
        }

        // Check If Already Friends
        bool alreadyFriends = requesterAccount.FriendedPeers
            .Any(friend => friend.Name.Equals(targetAccount.Name, StringComparison.OrdinalIgnoreCase));

        if (alreadyFriends)
        {
            SendFriendAddFailure(session, ChatProtocol.FriendAddStatus.DUPLICATE_RECORD);

            return this;
        }

        // Check If Requester Has Reached Friend Limit
        if (requesterAccount.FriendedPeers.Count >= ChatProtocol.FRIEND_LIMIT)
        {
            SendFriendAddFailure(session, ChatProtocol.FriendAddStatus.FRIEND_LIMIT_REACHED);

            return this;
        }

        // Check Whether Pending Friend Request Already Exists
        bool existingPendingRequest = await distributedCacheStore.PendingFriendRequestExists(requesterAccount.ID, targetAccount.ID);

        if (existingPendingRequest)
        {
            SendFriendAddFailure(session, ChatProtocol.FriendAddStatus.DUPLICATE_RECORD);

            return this;
        }

        // Check Whether The Target Account Has Already Sent A Friend Request To The Requester
        int? pendingFriendRequestNotificationID = await distributedCacheStore.GetFriendRequest(targetAccount.ID, requesterAccount.ID);

        if (pendingFriendRequestNotificationID is not null)
        {
            // Request Is Mutual: Both Accounts Become Friends Immediately
            await CreateMutualFriendship(requesterAccount, targetAccount, merrick, distributedCacheStore);

            // Send Approval Response To Requester (Their Request Was Approved)
            SendFriendRequestApproval(session, targetAccount, ChatProtocol.FriendApproveStatus.SUCCESS_REQUESTER);

            ChatSession? targetSession = Context.ChatSessions.Values
                .SingleOrDefault(chatSession => chatSession.Account.Name.Equals(targetAccount.Name, StringComparison.OrdinalIgnoreCase));

            // Send Approval Response To Target, If Online (They Are Approving The Requester)
            if (targetSession is not null)
                SendFriendRequestApproval(targetSession, requesterAccount, ChatProtocol.FriendApproveStatus.SUCCESS_APPROVER);

            return this;
        }

        int notificationID = GenerateNotificationID();

        // Request Is Not Mutual: Store Pending Request In Cache Store
        await distributedCacheStore.SetFriendRequest(requesterAccount.ID, targetAccount.ID, notificationID);

        // Send Success Response To Requester (Friend Request Created Successfully)
        SendFriendAddSuccess(session, targetAccount);

        ChatSession? targetOnlineSession = Context.ChatSessions.Values
            .SingleOrDefault(chatSession => chatSession.Account.Name.Equals(targetAccount.Name, StringComparison.OrdinalIgnoreCase));

        // Send Friend Request Notification To Target, If Online
        if (targetOnlineSession is not null && targetOnlineSession.Metadata.ClientChatModeState is not ChatProtocol.ChatModeType.CHAT_MODE_DND)
            SendFriendRequest(targetOnlineSession, requesterAccount, notificationID);

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
        int? pendingFriendRequestNotificationID = await distributedCacheStore.GetFriendRequest(requesterAccount.ID, approverAccount.ID);

        if (pendingFriendRequestNotificationID is null)
        {
            // No Pending Request Found
            return this;
        }

        // Create Bi-Directional Friendship
        await CreateMutualFriendship(requesterAccount, approverAccount, merrick, distributedCacheStore);

        // Send Approval Request To Approver
        SendFriendRequestApproval(session, requesterAccount, ChatProtocol.FriendApproveStatus.SUCCESS_APPROVER);

        ChatSession? requesterSession = Context.ChatSessions.Values
            .SingleOrDefault(chatSession => chatSession.Account.Name.Equals(requesterAccount.Name, StringComparison.OrdinalIgnoreCase));

        // Send Approval Response To Requester, If Online
        if (requesterSession is not null)
            SendFriendRequestApproval(requesterSession, approverAccount, ChatProtocol.FriendApproveStatus.SUCCESS_REQUESTER);

        return this;
    }

    /// <summary>
    ///     Creates bi-directional friendship between two accounts and removes any pending requests from the cache store.
    /// </summary>
    private static async Task CreateMutualFriendship(Account one, Account two, MerrickContext merrick, IDatabase distributedCacheStore)
    {
        // Check If Already Friends (Prevents Duplicates From Race Conditions)
        bool alreadyFriendsOneToTwo = one.FriendedPeers.Any(friend => friend.ID == two.ID);
        bool alreadyFriendsTwoToOne = two.FriendedPeers.Any(friend => friend.ID == one.ID);

        // If Not Already Friends, Add Account Two To Account One's Friends
        if (alreadyFriendsOneToTwo.Equals(false))
        {
            FriendedPeer friendOneToTwo = new ()
            {
                ID = two.ID,
                Name = two.Name,
                ClanTag = two.Clan?.Tag,
                FriendGroup = "DEFAULT"
            };

            one.FriendedPeers.Add(friendOneToTwo);
        }

        // If Not Already Friends, Add Account One To Account Two's Friends
        if (alreadyFriendsTwoToOne.Equals(false))
        {
            FriendedPeer friendTwoToOne = new ()
            {
                ID = one.ID,
                Name = one.Name,
                ClanTag = one.Clan?.Tag,
                FriendGroup = "DEFAULT"
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
    private static int GenerateNotificationID() => Guid.CreateVersion7().GetDeterministicInt32Hash();

    /// <summary>
    ///     Sends friend request notification to the target player for approval.
    /// </summary>
    private static void SendFriendRequest(ChatSession targetSession, Account requesterAccount, int notificationID)
    {
        ChatBuffer notification = new ();

        notification.WriteCommand(ChatProtocol.Command.CHAT_CMD_REQUEST_BUDDY_ADD_RESPONSE);
        notification.WriteInt8(Convert.ToByte(ChatProtocol.FriendAddStatus.SUCCESS_REQUESTEE));             // Friend Addition Status
        notification.WriteInt32(notificationID);                                                            // Notification ID For Tracking This Request
        notification.WriteString(requesterAccount.NameWithClanTag);                                         // Requester's Display Name With Clan Tag
        notification.WriteInt32(requesterAccount.ID);                                                       // Requester's Account ID
        notification.WriteInt8(Convert.ToByte(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED)); // Requester's Current Connection Status
        notification.WriteInt8(requesterAccount.GetChatClientFlags());                                      // Requester's Account Flags
        notification.WriteInt32(requesterAccount.Clan?.ID ?? 0);                                            // Requester's Clan ID (Zero If Not In A Clan)
        notification.WriteString(requesterAccount.Clan?.Name ?? string.Empty);                              // Requester's Clan Full Name (Empty String If Not In A Clan)
        notification.WriteString(requesterAccount.ChatSymbolNoPrefixCode);                                  // Requester's Chat Symbol
        notification.WriteString(requesterAccount.NameColour);                                              // Requester's Name Colour
        notification.WriteString(requesterAccount.Icon);                                                    // Requester's Account Icon
        notification.WriteInt32(requesterAccount.AscensionLevel);                                           // Requester's Ascension Level

        targetSession.Send(notification);
    }

    /// <summary>
    ///     Sends friend approval response.
    /// </summary>
    private static void SendFriendRequestApproval(ChatSession session, Account friendAccount, ChatProtocol.FriendApproveStatus status)
    {
        ChatBuffer response = new ();

        response.WriteCommand(ChatProtocol.Command.CHAT_CMD_REQUEST_BUDDY_APPROVE_RESPONSE);
        response.WriteInt8(Convert.ToByte(status));          // Friend Approval Status
        response.WriteInt32(friendAccount.ID);               // Friend's Account ID
        response.WriteInt32(session.Account.ID);             // Receiver's Own Account ID
        response.WriteString(friendAccount.NameWithClanTag); // Friend's Display Name With Clan Tag

        session.Send(response);
    }

    /// <summary>
    ///     Sends successful friend add response to the requester with complete friend details.
    ///     This is sent when a friend is immediately added (mutual request scenario).
    /// </summary>
    private static void SendFriendAddSuccess(ChatSession session, Account friendAccount)
    {
        int notificationID = GenerateNotificationID();

        ChatBuffer response = new ();

        response.WriteCommand(ChatProtocol.Command.CHAT_CMD_REQUEST_BUDDY_ADD_RESPONSE);
        response.WriteInt8(Convert.ToByte(ChatProtocol.FriendAddStatus.SUCCESS_REQUESTER));             // Success Indicator For The Client Who Initiated The Friend Request
        response.WriteInt32(notificationID);                                                            // The ID Of The Friend Addition Success Notification
        response.WriteString(friendAccount.NameWithClanTag);                                            // Friend's Display Name With Clan Tag
        response.WriteInt32(friendAccount.ID);                                                          // Friend's Account ID
        response.WriteInt8(Convert.ToByte(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED)); // Friend's Current Connection Status
        response.WriteInt8(friendAccount.GetChatClientFlags());                                         // Friend's Account Flags
        response.WriteInt32(friendAccount.Clan?.ID ?? 0);                                               // Friend's Clan ID (Zero If Not In A Clan)
        response.WriteString(friendAccount.Clan?.Name ?? string.Empty);                                 // Friend's Clan Name (Empty String If Not In A Clan)
        response.WriteString(friendAccount.ChatSymbolNoPrefixCode);                                     // Friend's Chat Symbol
        response.WriteString(friendAccount.NameColour);                                                 // Friend's Name Colour
        response.WriteString(friendAccount.Icon);                                                       // Friend's Account Icon
        response.WriteInt32(friendAccount.AscensionLevel);                                              // Friend's Ascension Level

        session.Send(response);
    }

    /// <summary>
    ///     Sends friend add failure response with the specific failure reason.
    /// </summary>
    private void SendFriendAddFailure(ChatSession session, ChatProtocol.FriendAddStatus reason)
    {
        int notificationID = GenerateNotificationID();

        ChatBuffer response = new ();

        response.WriteCommand(ChatProtocol.Command.CHAT_CMD_REQUEST_BUDDY_ADD_RESPONSE);
        response.WriteInt8(Convert.ToByte(reason)); // Friend Addition Failure Reason
        response.WriteInt32(notificationID);        // The ID Of The Friend Addition Failure Notification
        response.WriteString(AccountName);          // Target Account Name That Was Attempted To Be Added (For Client Error Message Display)

        session.Send(response);
    }
}
