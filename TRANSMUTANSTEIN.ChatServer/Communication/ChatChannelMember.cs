namespace TRANSMUTANSTEIN.ChatServer.Communication;

public class ChatChannelMember(ChatSession session)
{
    public Account Account = session.ClientInformation.Account;

    public ChatSession Session = session;

    public string AccountIcon = session.ClientInformation.Account.SelectedStoreItems
        .SingleOrDefault(item => item.StartsWith("ai."))?.Replace("ai.", string.Empty) ?? "Default Icon";

    public string ChatSymbol = session.ClientInformation.Account.SelectedStoreItems
        .SingleOrDefault(item => item.StartsWith("cs."))?.Replace("cs.", string.Empty) ?? string.Empty;

    public string NameColour = session.ClientInformation.Account.SelectedStoreItems
        .SingleOrDefault(item => item.StartsWith("cc."))?.Replace("cc.", string.Empty) ?? "white";

    public ChatProtocol.AdminLevel AdministratorLevel = session.ClientInformation.Account.Type is AccountType.Staff
        ? ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_STAFF
        : ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_NONE;

    public ChatProtocol.ChatClientStatus ConnectionStatus = ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED;
}
