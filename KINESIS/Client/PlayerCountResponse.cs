namespace KINESIS.Client;

public class PlayerCountResponse : ProtocolResponse
{
    // How many players are currently connected to the ChatServer.
    private readonly int NumberOfPlayersOnline;

    // Additional details to display in the following format: "TEXT1:COUNT1|TEXT2:COUNT2|TEXT3:COUNT3"
    // For example: "NA:12|TR:55|CIS:39|SEA:8|KR:4"
    // We are likely to make changes to this format in future.
    private readonly string Details;

    public PlayerCountResponse(int numberOfPlayersOnline, string details)
    {
        NumberOfPlayersOnline = numberOfPlayersOnline;
        Details = details;
    }

    public override CommandBuffer Encode()
    {
        CommandBuffer buffer = new();
        buffer.WriteInt16(ChatServerResponse.PlayerCount);
        buffer.WriteInt32(NumberOfPlayersOnline);
        buffer.WriteString(Details);
        return buffer;
    }
}
