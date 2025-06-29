namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Matchmaking;

[ChatCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_INVITE)]
public class GroupInvite(MerrickContext merrick, ILogger<GroupInvite> logger) : CommandProcessorsBase, ICommandProcessor
{
    private MerrickContext MerrickContext { get; set; } = merrick;
    private ILogger<GroupInvite> Logger { get; set; } = logger;

    public async Task Process(ChatSession session, ChatBuffer buffer)
    {
        GroupInviteRequestData requestData = new (buffer);

        ChatBuffer invite = new();

        invite.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_GROUP_INVITE);

        invite.WriteString(session.ClientInformation.Account.Name);                                     // Invite Issuer Name
        invite.WriteInt32(session.ClientInformation.Account.ID);                                        // Invite Issuer ID
        invite.WriteInt8(Convert.ToByte(ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED));   // Invite Issuer Status
        invite.WriteInt8(session.ClientInformation.Account.GetChatClientFlags());                       // Invite Issuer Chat Flags
        invite.WriteString(session.ClientInformation.Account.ChatNameColour);                           // Invite Issuer Chat Name Colour
        invite.WriteString(session.ClientInformation.Account.Icon);                                     // Invite Issuer Icon
        // TODO: Get Actual Map Name From Group Information, Rather Than Hardcoding It
        /*
            private static string GetMapName(TMMGameType gameType)
            {
                return gameType switch
                {
                    TMMGameType.MIDWARS => "midwars",
                    TMMGameType.RIFTWARS => "riftwars",
                    TMMGameType.CAMPAIGN_NORMAL => "caldavar",
                    TMMGameType.NORMAL => "caldavar",
                    TMMGameType.CAMPAIGN_CASUAL => "caldavar_old",
                    TMMGameType.CASUAL => "caldavar_old",
                    TMMGameType.REBORN_UNRANKED_NORMAL => "caldavar_reborn",
                    TMMGameType.REBORN_UNRANKED_CASUAL => "caldavar_reborn",
                    _ => "unknown:" + gameType,
                };
            }
         */
        invite.WriteString("caldavar");                                                                 // Map Name
        // TODO: Get Actual Game Type From Group Information, Rather Than Hardcoding It
        invite.WriteInt8(Convert.ToByte(ChatProtocol.TMMGameType.TMM_GAME_TYPE_NORMAL));                // Game Type
        // TODO: Get Actual Game Modes From Group Information, Rather Than Hardcoding It
        invite.WriteString("ap|ar|sd");                                                                 // Game Modes
        // TODO: Get Actual Game Regions From Group Information, Rather Than Hardcoding It
        invite.WriteString("EU|USE|USW");                                                               // Game Regions

        invite.PrependBufferSize();

        // TODO: Send Invite

        // TODO: Broadcast Invite To All Group Members
    }
}

public class GroupInviteRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes = buffer.ReadCommandBytes();
    public string InviteReceiverName = buffer.ReadString();
    //public string InviterName = buffer.ReadString();
    //public int InviterID = buffer.ReadInt32();
    //public ChatProtocol.ChatClientStatus InviterStatus = (ChatProtocol.ChatClientStatus) buffer.ReadInt8();

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
