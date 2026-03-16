namespace KONGOR.MasterServer.Controllers.ClientRequesterController;

public partial class ClientRequesterController
{
    private static StoreItemsConfiguration StoreItems => JSONConfiguration.StoreItemsConfiguration;

    /// <summary>
    ///     Persists the player's selected upgrade choices (e.g. account icon, name colour, chat symbol, announcer, taunt, etc.).
    ///     Called by the client whenever the player equips or unequips an upgrade via the <c>SelectUpgrade</c> or <c>ClearUpgrade</c> commands.
    ///     The client sends the full set of currently selected upgrade codes (un-prefixed) and the server replaces the stored selections.
    ///     Alternative avatars are excluded because they are selected per-hero via the match server's <c>GAME_CMD_SELECT_AVATAR</c> command.
    /// </summary>
    private async Task<IActionResult> SetSelectedUpgrades()
    {
        string cookie = Request.Form["cookie"].ToString();

        string? accountName = await DistributedCache.GetAccountNameForSessionCookie(cookie);

        if (accountName is null)
            return Unauthorized($@"Unrecognised Cookie ""{cookie}""");

        Account account = await MerrickContext.Accounts
            .Include(queriedAccount => queriedAccount.User)
            .SingleAsync(queriedAccount => queriedAccount.Name.Equals(accountName));

        // The Client Sends The Un-Prefixed Codes (The Result Of Upgrade_GetName Which Strips The Prefix Before The First Dot)
        // Each Code Needs To Be Matched Against Known Store Items To Recover The Prefixed Code And Verify Ownership
        // Only One Item Per Upgrade Type (Prefix) Is Allowed, Matching The Original PHP Behaviour

        Dictionary<string, string> selectedByPrefix = [];

        foreach (string? upgradeCode in Request.Form["selected_upgrades[]"])
        {
            if (string.IsNullOrWhiteSpace(upgradeCode))
                continue;

            // Custom Account Icons Are Not In The Store Item List And Are Handled Separately
            if (upgradeCode.StartsWith("custom_icon:"))
            {
                string prefixedCode = "ai." + upgradeCode;

                if (account.User.OwnedStoreItems.Contains(prefixedCode))
                    selectedByPrefix["ai."] = prefixedCode;

                continue;
            }

            StoreItem? storeItem = StoreItems.GetByCode(upgradeCode);

            if (storeItem is null)
                continue;

            // The Player Must Own The Item To Select It
            if (account.User.OwnedStoreItems.Contains(storeItem.PrefixedCode) is false)
                continue;

            // Alternative Avatars Are Not Stored In Selected Upgrades (They Are Handled By The Match Server)
            if (storeItem.StoreItemType is StoreItemType.AlternativeAvatar)
                continue;

            // Only One Item Per Upgrade Type Is Allowed
            if (selectedByPrefix.ContainsKey(storeItem.Prefix))
            {
                Logger.LogWarning(@"Account ""{AccountName}"" Sent Multiple Selected Upgrades For Type ""{Prefix}"": ""{Existing}"" And ""{Duplicate}""",
                    accountName, storeItem.Prefix, selectedByPrefix[storeItem.Prefix], storeItem.PrefixedCode);

                continue;
            }

            selectedByPrefix[storeItem.Prefix] = storeItem.PrefixedCode;
        }

        account.SelectedStoreItems = [.. selectedByPrefix.Values];

        await MerrickContext.SaveChangesAsync();

        // The Client Ignores The Response Body
        return Ok(string.Empty);
    }
}
