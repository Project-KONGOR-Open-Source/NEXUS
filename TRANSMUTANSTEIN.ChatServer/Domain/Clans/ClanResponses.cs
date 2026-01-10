
using TRANSMUTANSTEIN.ChatServer.Attributes;
using TRANSMUTANSTEIN.ChatServer.Domain.Core;

namespace TRANSMUTANSTEIN.ChatServer.Domain.Clans;

public class ClanCreateFailResponse : ChatBuffer
{
    public ClanCreateFailResponse(ushort command, string content)
    {
        WriteCommand(command);
        WriteString(content);
    }
    
    public ClanCreateFailResponse(ushort command)
    {
        WriteCommand(command);
    }
}

public class ClanAddMemberResponse : ChatBuffer
{
    public ClanAddMemberResponse(string inviterName, string clanName)
    {
        WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_ADD_MEMBER);
        WriteString(inviterName);
        WriteString(clanName);
    }
}

public class ClanCreateAcceptedResponse : ChatBuffer
{
    public ClanCreateAcceptedResponse(int clanId)
    {
        WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_ACCEPT);
        WriteInt32(clanId);
    }
    
    public ClanCreateAcceptedResponse(string acceptorName)
    {
        WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_ACCEPT);
        WriteString(acceptorName);
    }
}

public class ClanCreateCompleteResponse : ChatBuffer
{
    public ClanCreateCompleteResponse(int clanId)
    {
        WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_COMPLETE);
        WriteInt32(clanId);
    }
     public ClanCreateCompleteResponse()
    {
        WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_COMPLETE);
    }
}

public class ClanNewMemberResponse : ChatBuffer
{
    public ClanNewMemberResponse(int accountId)
    {
        WriteCommand(ChatProtocol.Command.CHAT_CMD_NEW_CLAN_MEMBER);
        WriteInt32(accountId);
    }

    public ClanNewMemberResponse(int accountId, int clanId, string clanName, string clanTag)
    {
        WriteCommand(ChatProtocol.Command.CHAT_CMD_NEW_CLAN_MEMBER);
        WriteInt32(accountId);
        WriteInt32(clanId);
        WriteString(clanName);
        WriteString(clanTag);
    }
}

public class ClanCreateRejectedResponse : ChatBuffer
{
    public ClanCreateRejectedResponse(string rejectorName)
    {
        WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_REJECT);
        WriteString(rejectorName);
    }
}

public class ClanAddRejectedResponse : ChatBuffer
{
    public ClanAddRejectedResponse(string rejectorName)
    {
        WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_ADD_REJECTED);
        WriteString(rejectorName);
    }
}

public class ClanRankChangeResponse : ChatBuffer
{
    public ClanRankChangeResponse(int targetAccountId, MERRICK.DatabaseContext.Enumerations.ClanTier newRank, int promoterAccountId)
    {
        WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_RANK_CHANGE);
        WriteInt32(targetAccountId);
        WriteInt8((byte)newRank);
        WriteInt32(promoterAccountId);
    }

}

public class ClanWhisperResponse : ChatBuffer
{
    public ClanWhisperResponse(int senderAccountId, string message)
    {
        WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_WHISPER);
        WriteInt32(senderAccountId);
        WriteString(message);
    }
}

public class ClanWhisperFailedResponse : ChatBuffer
{
    public ClanWhisperFailedResponse()
    {
        WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_WHISPER_FAILED);
    }
}

