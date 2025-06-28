namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_INVITE)]
public class GroupInvite(MerrickContext merrick, ILogger<GroupInvite> logger) : CommandProcessorsBase, ICommandProcessor
{
    private MerrickContext MerrickContext { get; set; } = merrick;
    private ILogger<GroupInvite> Logger { get; set; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        GroupInviteRequestData requestData = new (buffer);
    }
}

public class GroupInviteRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();
    public string InviterName = buffer.ReadString();
    public int InviterID = buffer.ReadInt32();
    public ChatProtocol.ChatClientStatus InviterStatus = (ChatProtocol.ChatClientStatus) buffer.ReadInt8();

    // TODO: These Are Wrong, This Is The Broadcast Model

    /*
        public byte GetAccountFlags(Account account)
        {
            byte Flags = 0x0;

            if (account.AccountType == AccountType.Staff)
            {
                Flags |= (byte)ChatServerProtocol.ChatClientFlags.IsStaff;
            }
            if (account.ClanTier == Clan.Tier.Officer)
            {
                Flags |= (byte)ChatServerProtocol.ChatClientFlags.IsOfficer;
            }
            if (account.ClanTier == Clan.Tier.Leader)
            {
                Flags |= (byte)ChatServerProtocol.ChatClientFlags.IsClanLeader;
            }

            return Flags;
        }
     */
}
