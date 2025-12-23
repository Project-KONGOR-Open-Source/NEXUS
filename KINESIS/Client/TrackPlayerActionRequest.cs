using System.Text;

namespace KINESIS.Client;

public enum PlayerAction
{
    // Custom player actions start at 30.
    OBTAIN_DETAILED_ONLINE_STATUS = 30,
};

public class TrackPlayerActionRequest : ProtocolRequest
{
    private readonly PlayerAction Action;

    public TrackPlayerActionRequest(PlayerAction action)
    {
        Action = action;
    }

    public static TrackPlayerActionRequest Decode(byte[] data, int offset, out int updatedOffset)
    {
        TrackPlayerActionRequest message = new TrackPlayerActionRequest(
            action: (PlayerAction)ReadByte(data, offset, out offset)
        );

        updatedOffset = offset;
        return message;
    }


    private string ObtainDetailedOnlineStats()
    {
        StringBuilder status = new();

        // Iterate over all game servers to figure out if they are available to host games,
        // of if there is a game in progress.
        int gamesInProgress = 0;
        Dictionary<string, int> serversByRegion = new();
        Dictionary<string, int> gamesInProgressByRegion = new();

        /*
        foreach (ConnectedServer connectedServer in KongorContext.ConnectedServers)
        {
            if (connectedServer.GamePhase >= 2) // TODO: elaborate
            {
                ++gamesInProgress;
                int count = 0;
                gamesInProgressByRegion.TryGetValue(connectedServer.Region, out count);
                gamesInProgressByRegion[connectedServer.Region] = count + 1;
            }
            else if (connectedServer.Status != ChatServerProtocol.ServerStatus.Sleeping)
            {
                if (connectedServer.Region == null) continue;

                int count = 0;
                serversByRegion.TryGetValue(connectedServer.Region, out count);
                serversByRegion[connectedServer.Region] = count + 1;
            }
        }
        */

        // Games In Progress
        status.Append("games_in_progress::ALL:");
        status.Append(gamesInProgress.ToString());
        status.Append('|');

        foreach (KeyValuePair<string, int> entry in gamesInProgressByRegion)
        {
            status.Append("games_in_progress::");
            status.Append(entry.Key);
            status.Append(':');
            status.Append(entry.Value.ToString());
            status.Append('|');
        }

        // Game Servers Available
        foreach (KeyValuePair<string, int> entry in serversByRegion)
        {
            status.Append("servers_available::");
            status.Append(entry.Key);
            status.Append(':');
            status.Append(entry.Value.ToString());
            status.Append('|');
        }

        //status.Append(GameFinder.MostRecentTMMStats);
        return status.ToString();
    }
}

