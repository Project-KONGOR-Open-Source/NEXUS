namespace TRANSMUTANSTEIN.ChatServer.CommandProcessors.Social;

public class BlockPhraseRequestData(ChatBuffer buffer)
{
    public byte[] CommandBytes { get; init; } = buffer.ReadCommandBytes();

    public string Phrase { get; } = buffer.ReadString();
}
