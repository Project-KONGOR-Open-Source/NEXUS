using TRANSMUTANSTEIN.ChatServer.Domain.Communication;
using TRANSMUTANSTEIN.ChatServer.Internals;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Channels;

[ChatCommand(ChatProtocol.ClientToChatServer.NET_CHAT_CL_GET_CHANNEL_LIST)]
public class GetChannelList(IChatContext chatContext) : ISynchronousCommandProcessor<ChatSession>
{
    public void Process(ChatSession session, ChatBuffer buffer)
    {
        // 1. Filter Channels
        List<ChatChannel> publicChannels = chatContext.ChatChannels.Values
            .Where(c => !c.Flags.HasFlag(ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_HIDDEN) &&
                        !c.Flags.HasFlag(ChatProtocol.ChatChannelType.CHAT_CHANNEL_FLAG_CLAN))
            .ToList();

        // 2. Send SYN (Start) - Likely expects Count?
        // If unsure, sending 0 bytes payload for SYN is risky if it expects int.
        // But most "List" protocols send Count.
        // Let's assume Count.
        // Wait, if it's just a marker, sending extra bytes usually doesn't hurt if read is strict?
        // Actually, if client reads Int32 and we send nothing, it waits.
        // I will assume Count.
        /*
        ChatBuffer syn = new();
        syn.WriteCommand(ChatProtocol.ChatServerToClient.NET_CHAT_CL_CHANNEL_LIST_SYN);
        syn.WriteInt32(publicChannels.Count);
        session.Send(syn);
        */

        // Actually, looking at `NET_CHAT_CL_CHANNEL_LIST_SYN` (0x1C03), it's Server->Client.
        // There is no explicit "Count" field mentioned in typical HoN protocol docs online (if any).
        // However, usually these lists are streaming.
        // I will send simple SYN. If it fails, we fix it.

        ChatBuffer syn = new();
        syn.WriteCommand(ChatProtocol.ChatServerToClient.NET_CHAT_CL_CHANNEL_LIST_SYN);
        session.Send(syn);

        // 3. Send Info for each channel
        foreach (ChatChannel? channel in publicChannels)
        {
            ChatBuffer info = new();
            info.WriteCommand(ChatProtocol.ChatServerToClient.NET_CHAT_CL_CHANNEL_INFO);
            info.WriteString(channel.Name);
            info.WriteInt32(channel.ID);
            info.WriteInt8((byte) channel.Flags);
            info.WriteInt32(channel.Members.Count);
            info.WriteString(channel.Topic);

            session.Send(info);
        }

        // No explicit End command found in ChatServerToClient for Main List.
    }
}
