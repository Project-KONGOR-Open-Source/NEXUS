namespace KONGOR.MasterServer.Controllers.ClientRequester;

public partial class ClientRequesterController
{
    /// <summary>
    ///     Handles the "add_room" client requester command, which saves a chat channel to the account's auto-connect list, the channels of which the game client joins on login.
    ///     The game client only adds the channel to its local copy of the list if the response reports success, and otherwise notifies the player that saving the channel failed.
    /// </summary>
    private async Task<IActionResult> AddAutoConnectChatChannel()
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        string? channelName = Request.Form["chatroom_name"];

        if (string.IsNullOrWhiteSpace(channelName))
            return BadRequest(@"Missing Value For Form Parameter ""chatroom_name""");

        if (channelName.Length > ChatProtocol.CHAT_CHANNEL_MAX_LENGTH)
            return BadRequest($@"Value For Form Parameter ""chatroom_name"" Exceeds The Maximum Channel Name Length Of {ChatProtocol.CHAT_CHANNEL_MAX_LENGTH} Characters");

        (bool isValid, string? accountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);

        if (isValid.Equals(false) || accountName is null)
            return Unauthorized($@"Unrecognised Cookie ""{cookie}""");

        Account account = await MerrickContext.Accounts
            .Include(candidate => candidate.Clan)
            .SingleAsync(candidate => candidate.Name.Equals(accountName));

        List<string> defaultChannels = account.GetDefaultChatChannels();

        // Channel Names Are Matched Case-Insensitively, Consistent With How The Game Client Determines Whether A Channel Is Already Saved
        // The Default Channels Are Joined On Every Login, So They Count As Saved Without Being Persisted, Which Also Keeps Any Numbered General Channel Load-Balanced
        if (account.IsDefaultChatChannel(channelName))
            return Ok(PhpSerialization.Serialize(new Dictionary<string, string> { { "add_room", "OK" } }));

        // Accounts Are Placed In Reserved Channels Based On Their State
        if (ChatChannels.IsReservedChannel(channelName))
        {
            Logger.LogInformation(@"Account ""{AccountName}"" (ID: {AccountID}) Could Not Save Reserved Chat Channel ""{ChatChannelName}"" To Its Auto-Connect List",
                account.Name, account.ID, channelName);

            Dictionary<string, object> failure = new ()
            {
                { "add_room", false },
                { "error", "Reserved Chat Channels Cannot Be Saved To The Auto-Connect List" }
            };

            return Ok(PhpSerialization.Serialize(failure));
        }

        if (account.AutoConnectChatChannels.Contains(channelName, StringComparer.OrdinalIgnoreCase))
            return Ok(PhpSerialization.Serialize(new Dictionary<string, string> { { "add_room", "OK" } }));

        // Saving More Channels Than A Client Can Be In At Once Serves No Purpose, Since The Excess Could Not Be Joined On Login Alongside The Default Channels
        // Staff Accounts Are Exempt, Consistent With Their Exemption From The Chat Server's Channel Limit
        if (account.Type is not AccountType.Staff && defaultChannels.Count + account.AutoConnectChatChannels.Count >= ChatProtocol.MAX_CHANNELS_PER_CLIENT)
        {
            Logger.LogInformation(@"Account ""{AccountName}"" (ID: {AccountID}) Could Not Save Chat Channel ""{ChatChannelName}"" Because Its Auto-Connect List Is Full",
                account.Name, account.ID, channelName);

            Dictionary<string, object> failure = new ()
            {
                { "add_room", false },
                { "error", $"An Account Cannot Auto-Connect To More Than {ChatProtocol.MAX_CHANNELS_PER_CLIENT} Channels" }
            };

            return Ok(PhpSerialization.Serialize(failure));
        }

        account.AutoConnectChatChannels.Add(channelName);

        await MerrickContext.SaveChangesAsync();

        Logger.LogInformation(@"Account ""{AccountName}"" (ID: {AccountID}) Saved Chat Channel ""{ChatChannelName}"" To Its Auto-Connect List",
            account.Name, account.ID, channelName);

        return Ok(PhpSerialization.Serialize(new Dictionary<string, string> { { "add_room", "OK" } }));
    }

    /// <summary>
    ///     Handles the "remove_room" client requester command, which removes a chat channel from the account's auto-connect list.
    ///     Removing a channel which is not saved still succeeds, but the default channels cannot be removed, because the account joins them on every login.
    ///     A clan channel therefore only stops being joined on login once the account leaves the clan.
    /// </summary>
    private async Task<IActionResult> RemoveAutoConnectChatChannel()
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        string? channelName = Request.Form["chatroom_name"];

        if (string.IsNullOrWhiteSpace(channelName))
            return BadRequest(@"Missing Value For Form Parameter ""chatroom_name""");

        (bool isValid, string? accountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);

        if (isValid.Equals(false) || accountName is null)
            return Unauthorized($@"Unrecognised Cookie ""{cookie}""");

        Account account = await MerrickContext.Accounts
            .Include(candidate => candidate.Clan)
            .SingleAsync(candidate => candidate.Name.Equals(accountName));

        // A Failure Keeps The Game Client Showing The Default Channel As Saved, Which Reflects That It Is Still Joined On Every Login
        if (account.IsDefaultChatChannel(channelName))
        {
            Logger.LogInformation(@"Account ""{AccountName}"" (ID: {AccountID}) Could Not Remove Default Chat Channel ""{ChatChannelName}"" From Its Auto-Connect List",
                account.Name, account.ID, channelName);

            Dictionary<string, object> failure = new ()
            {
                { "remove_room", false },
                { "error", "Default Chat Channels Cannot Be Removed From The Auto-Connect List" }
            };

            return Ok(PhpSerialization.Serialize(failure));
        }

        // Channel Names Are Matched Case-Insensitively, Consistent With How The Game Client Determines Whether A Channel Is Saved
        int removedChannelCount = account.AutoConnectChatChannels
            .RemoveAll(savedChannelName => savedChannelName.Equals(channelName, StringComparison.OrdinalIgnoreCase));

        if (removedChannelCount > 0)
        {
            await MerrickContext.SaveChangesAsync();

            Logger.LogInformation(@"Account ""{AccountName}"" (ID: {AccountID}) Removed Chat Channel ""{ChatChannelName}"" From Its Auto-Connect List",
                account.Name, account.ID, channelName);
        }

        return Ok(PhpSerialization.Serialize(new Dictionary<string, string> { { "remove_room", "OK" } }));
    }

    /// <summary>
    ///     Handles the "clear_rooms" client requester command, which removes every chat channel from the account's auto-connect list.
    ///     On success, the game client also leaves every channel it considered saved, whether or not it is currently in that channel.
    ///     The default channels are not persisted to the list, so the account still joins them on its next login.
    /// </summary>
    private async Task<IActionResult> ClearAutoConnectChatChannels()
    {
        string? cookie = Request.Form["cookie"];

        if (cookie is null)
            return BadRequest(@"Missing Value For Form Parameter ""cookie""");

        (bool isValid, string? accountName) = await DistributedCache.ValidateAccountSessionCookie(cookie);

        if (isValid.Equals(false) || accountName is null)
            return Unauthorized($@"Unrecognised Cookie ""{cookie}""");

        Account account = await MerrickContext.Accounts
            .SingleAsync(candidate => candidate.Name.Equals(accountName));

        if (account.AutoConnectChatChannels.Count > 0)
        {
            account.AutoConnectChatChannels.Clear();

            await MerrickContext.SaveChangesAsync();

            Logger.LogInformation(@"Account ""{AccountName}"" (ID: {AccountID}) Cleared Its Auto-Connect List", account.Name, account.ID);
        }

        return Ok(PhpSerialization.Serialize(new Dictionary<string, string> { { "clear_rooms", "OK" } }));
    }
}
