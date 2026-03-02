namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Connection;

/// <summary>
///     Handles upgrade refresh requests from game clients.
///     When a player changes their selected upgrades (e.g. account icon, chat symbol, name colour), the game client sends this command to notify the chat server that its cached upgrade data is stale.
///     The handler reloads the account's selected store items from the database and re-broadcasts the connection status to online peers so they see the updated visuals.
/// </summary>
[ChatCommand(ChatProtocol.ClientToChatServer.NET_CHAT_CL_REFRESH_UPGRADES)]
public class RefreshUpgrades(MerrickContext merrick) : IAsynchronousCommandProcessor<ClientChatSession>
{
    public async Task Process(ClientChatSession session, ChatBuffer buffer)
    {
        RefreshUpgradesRequestData requestData = new (buffer);

        Account? account = await merrick.Accounts
            .AsNoTracking()
            .SingleOrDefaultAsync(account => account.ID == session.Account.ID);

        if (account is null)
        {
            Log.Error(@"[BUG] Account With ID ""{AccountID}"" Could Not Be Found During Upgrade Refresh", session.Account.ID);

            return;
        }

        string previousIcon = session.Account.IconNoPrefixCode;
        string previousChatSymbol = session.Account.ChatSymbolNoPrefixCode;
        string previousNameColour = session.Account.NameColourNoPrefixCode;

        session.Account.SelectedStoreItems = account.SelectedStoreItems;

        bool visualsChanged = previousIcon != session.Account.IconNoPrefixCode
                     || previousChatSymbol != session.Account.ChatSymbolNoPrefixCode
                     || previousNameColour != session.Account.NameColourNoPrefixCode;

        if (visualsChanged)
            session.BroadcastVisualRefresh();

        Log.Debug(@"Processed Upgrade Refresh For Account ""{AccountName}"" (Visuals Changed: {VisualsChanged})", session.Account.Name, visualsChanged);
    }
}

file class RefreshUpgradesRequestData
{
    public byte[] CommandBytes { get; init; }

    public RefreshUpgradesRequestData(ChatBuffer buffer)
    {
        CommandBytes = buffer.ReadCommandBytes();
    }
}
