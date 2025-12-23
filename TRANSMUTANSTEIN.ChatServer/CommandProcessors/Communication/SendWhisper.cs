using global::TRANSMUTANSTEIN.ChatServer.Domain.Core;
using global::TRANSMUTANSTEIN.ChatServer.Domain.Communication;
using global::TRANSMUTANSTEIN.ChatServer.Utilities;
using KINESIS.Client;

namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Communication;

[ChatCommand(ChatProtocol.Command.CHAT_CMD_WHISPER)]
public class SendWhisper : ISynchronousCommandProcessor<ClientChatSession>
{
    public void Process(ClientChatSession session, ChatBuffer buffer)
    {
        Log.Error("DEBUG: SendWhisper.Process O0x0008 ENTERED. Buffer Size: {Size}", buffer.Size);

        // Decode request using KINESIS model
        // Skip 2 bytes for the command header (0x0008)
        WhisperRequest request = WhisperRequest.Decode(buffer.Data, (int)buffer.Offset + 2, out _);

        Log.Error("DEBUG: Decoded WhisperRequest. Nickname='{Nickname}', Message='{Message}'", request.Nickname, request.Message);

        // Note: request.Nickname corresponds to TargetName in the protocol
        Whisper
            .Create(request.Message)
            .Send(session, request.Nickname);
    }
}
