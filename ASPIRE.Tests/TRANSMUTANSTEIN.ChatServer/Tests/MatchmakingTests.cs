using System.Net.Sockets;

using ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;

using KONGOR.MasterServer.Configuration;
using KONGOR.MasterServer.Configuration.Matchmaking;

using TRANSMUTANSTEIN.ChatServer.Domain.Core;

namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests;

public class MatchmakingTests
{
    private const int TestPort = 54123;

    [Test]
    public async Task PopularityUpdate_ReturnsConfiguredModes()
    {
        // 1. Setup: Inject Mock Configuration via Reflection
        MatchmakingConfiguration mockConfig = new()
        {
            Ranked =
                new MatchmakingMapConfiguration
                {
                    Map = "mock_ranked",
                    Modes = ["ap", "sd"],
                    Regions = ["USE"],
                    Match =
                        new MatchConfiguration
                        {
                            MaximumPlayerRatingDifference = 500, IsRanked = true, TeamSize = 5
                        }
                },
            Unranked = new MatchmakingMapConfiguration
            {
                Map = "mock_unranked",
                Modes = ["ar"],
                Regions = ["USW"],
                Match = new MatchConfiguration { MaximumPlayerRatingDifference = 0, IsRanked = false, TeamSize = 5 }
            },
            MidWars = new MatchmakingMapConfiguration
            {
                Map = "midwars",
                Modes = ["bd"],
                Regions = ["EU"],
                Match = new MatchConfiguration { MaximumPlayerRatingDifference = 0, IsRanked = false, TeamSize = 5 }
            },
            RiftWars = new MatchmakingMapConfiguration
            {
                Map = "riftwars_test", // Unique map to verify
                Modes = ["bp"],
                Regions = ["AU"],
                Match = new MatchConfiguration { MaximumPlayerRatingDifference = 0, IsRanked = false, TeamSize = 5 }
            }
        };

        // Set the static property directly
        JSONConfiguration.MatchmakingConfiguration = mockConfig;

        // 2. Start Application (Orchestrated Pattern preferred as per ChatChannelTests)
        await using TRANSMUTANSTEINServiceProvider app =
            await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(TestPort);

        // 3. Connect Client
        // Note: Using ChatTestHelpers pattern from ChatChannelTests
        using TcpClient client =
            await ChatTestHelpers.ConnectAndAuthenticateAsync(app, TestPort, 9999, "MatchmakingTester");
        NetworkStream stream = client.GetStream();

        // 4. Send Popularity Update Request
        ChatBuffer requestBuffer = new();
        requestBuffer.WriteCommand(ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_POPULARITY_UPDATE);

        byte[] packet = requestBuffer.Data.AsSpan(0, (int) requestBuffer.Size).ToArray();
        List<byte> rawPacket = [];
        rawPacket.AddRange(BitConverter.GetBytes((ushort) packet.Length));
        rawPacket.AddRange(packet);
        await stream.WriteAsync(rawPacket.ToArray());

        // 5. Wait for Response (NET_CHAT_CL_TMM_POPULARITY_UPDATE = 0x0D07)
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

        // Loop until we find the popularity packet
        while (!cts.Token.IsCancellationRequested)
        {
            byte[] header = new byte[4];
            await stream.ReadExactlyAsync(header, cts.Token);
            ushort length = BitConverter.ToUInt16(header, 0);
            ushort command = BitConverter.ToUInt16(header, 2);

            int payloadSize = length - 2;
            byte[] payload = new byte[payloadSize];
            int read = 0;
            while (read < payloadSize)
            {
                read += await stream.ReadAsync(payload, read, payloadSize - read, cts.Token);
            }

            if (command == ChatProtocol.Matchmaking.NET_CHAT_CL_TMM_POPULARITY_UPDATE)
            {
                ChatBuffer reader = new(payload);

                byte availability = reader.ReadInt8(); // 1
                string maps = reader.ReadString();
                string types = reader.ReadString();
                string modes = reader.ReadString();
                string regions = reader.ReadString();

                // Assertions (TUnit Syntax)
                await Assert.That(maps).Contains("riftwars_test");
                await Assert.That(maps).Contains("mock_ranked");

                // Verify our RiftWars logic added Type 4
                await Assert.That(types).Contains("4");

                // Verify map config presence
                await Assert.That(modes).Contains("bp"); // From RiftWars config

                return; // Pass
            }
        }

        Assert.Fail("NET_CHAT_CL_TMM_POPULARITY_UPDATE not received");
    }
}