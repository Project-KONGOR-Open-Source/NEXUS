using System.Net.Sockets;
using ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;

using TRANSMUTANSTEIN.ChatServer.Domain.Core;

namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests;


public sealed class ChatModerationTagTests
{
    private static async Task<(int, int)> JoinChannelAndGetId(TcpClient client, string channelName)
    {
        ChatBuffer joinBuffer = new();
        joinBuffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_JOIN_CHANNEL);
        joinBuffer.WriteString(channelName);

        byte[] packet = joinBuffer.Data.AsSpan(0, (int) joinBuffer.Size).ToArray();
        List<byte> rawBytes = [];
        rawBytes.AddRange(BitConverter.GetBytes((ushort) packet.Length));
        rawBytes.AddRange(packet);
        await client.GetStream().WriteAsync(rawBytes.ToArray());

        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));
        byte[] header = new byte[4];
        await client.GetStream().ReadExactlyAsync(header, cts.Token);
        ushort length = BitConverter.ToUInt16(header, 0);
        ushort command = BitConverter.ToUInt16(header, 2);

        int payloadSize = length - 2;
        byte[] payload = new byte[payloadSize];
        int read = 0;
        while (read < payloadSize)
        {
            read += await client.GetStream().ReadAsync(payload, read, payloadSize - read, cts.Token);
        }

        if (command == ChatProtocol.Command.CHAT_CMD_CHANGED_CHANNEL)
        {
            ChatBuffer b = new(payload);
            b.ReadString(); // Name
            int id = b.ReadInt32(); // ID
            return (id, command);
        }

        return (0, command);
    }

    [Test]
    public async Task Ban_WithClanTag_StripsTagAndBansUser()
    {
        int testPort = 0;
        await using TRANSMUTANSTEINServiceProvider app =
            await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(testPort);

        // 801: Admin, 802: Target
        // We simulate Target having a Clan Tag in their "Display Name" as sent by client, OR we explicitly send the tagged name in the command.
        // The fix is in the CommandProcessor treating "requestData.TargetName" which comes from the packet.

        using TcpClient adminClient =
            await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, 801, "TagBanAdmin");
        using TcpClient targetClient =
            await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, 802, "TargetNoTag");

        // Manually Promote Admin to Staff so they can Ban
        using (IServiceScope scope = app.Services.CreateScope())
        {
            MerrickContext db = scope.ServiceProvider.GetRequiredService<MerrickContext>();
            Account? adminAcc = await db.Accounts.Include(a => a.User).FirstOrDefaultAsync(a => a.ID == 801);
            if (adminAcc != null)
            {
                Role? staffRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Staff");
                if (staffRole == null)
                {
                    staffRole = new Role { ID = 2, Name = "Staff" };
                    db.Roles.Add(staffRole);
                    await db.SaveChangesAsync();
                }

                adminAcc.User.Role = staffRole;
                db.Update(adminAcc.User);
                await db.SaveChangesAsync();
            }
        }

        // Join Channel
        (int channelId, _) = await JoinChannelAndGetId(adminClient, "TagBanRoom");
        await JoinChannelAndGetId(targetClient, "TagBanRoom");

        // Act - Ban with [TAG] prefix
        // The Target's actual name in DB/Session is "TargetNoTag".
        // We verify that banning "[PK]TargetNoTag" works.
        ChatBuffer buffer = new();
        buffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_BAN);
        buffer.WriteInt32(channelId);
        buffer.WriteString("[PK]TargetNoTag"); // <--- The Test Case

        byte[] packet = buffer.Data.AsSpan(0, (int) buffer.Size).ToArray();
        List<byte> rawBytes = [];
        rawBytes.AddRange(BitConverter.GetBytes((ushort) packet.Length));
        rawBytes.AddRange(packet);
        await adminClient.GetStream().WriteAsync(rawBytes.ToArray());

        // Assert - Target should receive KICK (0x0031)
        byte[] h1 = new byte[4];
        await targetClient.GetStream().ReadExactlyAsync(h1);
        ushort c1 = BitConverter.ToUInt16(h1, 2);

        // Consume payload
        ushort l1 = BitConverter.ToUInt16(h1, 0);
        byte[] p1 = new byte[l1 - 2];
        await targetClient.GetStream().ReadExactlyAsync(p1);

        await Assert.That(c1).IsEqualTo(ChatProtocol.Command.CHAT_CMD_CHANNEL_KICK);
    }

    [Test]
    public async Task Silence_WithClanTag_StripsTagAndSilencesUser()
    {
        int testPort = 0;
        await using TRANSMUTANSTEINServiceProvider app =
            await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(testPort);

        using TcpClient adminClient =
            await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, 803, "TagSilenceAdmin");
        using TcpClient targetClient =
            await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, 804, "TargetSilence");

        // Promote Admin
        using (IServiceScope scope = app.Services.CreateScope())
        {
            MerrickContext db = scope.ServiceProvider.GetRequiredService<MerrickContext>();
            Account? adminAcc = await db.Accounts.Include(a => a.User).FirstOrDefaultAsync(a => a.ID == 803);
            if (adminAcc != null)
            {
                Role? staffRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Staff")
                                  ?? new Role { ID = 2, Name = "Staff" };
                if (db.Entry(staffRole).State == EntityState.Detached) db.Roles.Attach(staffRole);

                adminAcc.User.Role = staffRole;
                db.Update(adminAcc.User);
                await db.SaveChangesAsync();
            }
        }

        (int channelId, _) = await JoinChannelAndGetId(adminClient, "TagSilenceRoom");
        await JoinChannelAndGetId(targetClient, "TagSilenceRoom");

        // Act - Silence with [TAG]
        ChatBuffer buffer = new();
        buffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_SILENCE_USER);
        buffer.WriteInt32(channelId);
        buffer.WriteString("[PK]TargetSilence");
        buffer.WriteInt32(60000); // 1 min

        byte[] packet = buffer.Data.AsSpan(0, (int) buffer.Size).ToArray();
        List<byte> rawBytes = [];
        rawBytes.AddRange(BitConverter.GetBytes((ushort) packet.Length));
        rawBytes.AddRange(packet);
        await adminClient.GetStream().WriteAsync(rawBytes.ToArray());

        // Assert - Target should receive SILENCED (0x0038)
        byte[] h1 = new byte[4];
        await targetClient.GetStream().ReadExactlyAsync(h1);
        ushort c1 = BitConverter.ToUInt16(h1, 2);

        ushort l1 = BitConverter.ToUInt16(h1, 0);
        byte[] p1 = new byte[l1 - 2];
        await targetClient.GetStream().ReadExactlyAsync(p1);

        await Assert.That(c1).IsEqualTo(ChatProtocol.Command.CHAT_CMD_CHANNEL_SILENCE_PLACED);
    }

    [Test]
    public async Task Unban_WithClanTag_StripsTagAndUnbansUser()
    {
        // Arrange
        // Use Random Names to avoid collisions with other tests (Parallel Execution)
        string adminName = $"TagUnbanAdmin_{Guid.NewGuid().ToString()[..8]}";
        string observerName = $"Observer_{Guid.NewGuid().ToString()[..8]}";
        string targetName = $"TargetUnban_{Guid.NewGuid().ToString()[..8]}";

        int testPort = 0;
        await using TRANSMUTANSTEINServiceProvider app =
            await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(testPort);

        // 1. Admin connects
        int adminId = Random.Shared.Next(100, 1000);
        using TcpClient adminClient = await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, adminId, adminName, 
            accountType: AccountType.Staff); // Officer can Ban

        // 2. Observer connects (To verify Unban broadcast)
        int observerId = adminId + 1;
        using TcpClient observerClient = await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, observerId, observerName);

        // 3. Target User (The one who was banned/kicked) - Needed?
        // Just mocking the Ban in DB or simulate full flow?
        // Simulate full flow: Admin Bans Target. Target Kicked. Admin Unbans.
        // But `Unban` test title "Unban_WithClanTag_StripsTag..."
        // So we need a Target WITH Clan Tag.
        
        // Create "Target" user with REAL Clan Tag in DB
        // Account Name = "TargetUnban_..." (Clean)
        // Clan Tag = "PK"
        string targetDisplay = $"[PK]{targetName}"; 
        int targetId = observerId + 1;
        using TcpClient targetClient = await ChatTestHelpers.ConnectAndAuthenticateAsync(
            app, app.ClientPort, targetId, targetName, // Use CLEAN name for Account
            clanName: "PhantomKind", clanTag: "PK");   // Assign Clan

        // All join "TagUnbanRoom"
        string roomName = $"TagUnbanRoom_{Guid.NewGuid().ToString()[..8]}";
        int channelId = await ChatTestHelpers.JoinChannelAndGetId(adminClient, roomName);
        await ChatTestHelpers.JoinChannelAndGetId(observerClient, roomName);
        await ChatTestHelpers.JoinChannelAndGetId(targetClient, roomName);

        // 1. Ban "TargetUnban" (using clean name to ensure ban works)
        ChatBuffer banBuffer = new();
        banBuffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_BAN);
        banBuffer.WriteInt32(channelId);
        banBuffer.WriteString(targetName); // Use dynamic Name (Exact Match)

        byte[] banPacket = banBuffer.Data.AsSpan(0, (int) banBuffer.Size).ToArray();
        List<byte> banRaw = [];
        banRaw.AddRange(BitConverter.GetBytes((ushort) banPacket.Length));
        banRaw.AddRange(banPacket);
        await adminClient.GetStream().WriteAsync(banRaw.ToArray());

        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));

        // Read Obs: Ban Notification
        ushort c1 = 0;
        int maxRetries = 10;
        for(int i=0; i<maxRetries; i++)
        {
            byte[] h = new byte[4];
            await observerClient.GetStream().ReadExactlyAsync(h, cts.Token);
            c1 = BitConverter.ToUInt16(h, 2);
            ushort len = BitConverter.ToUInt16(h, 0);
            byte[] p = new byte[len - 2];
            int r = 0;
            while(r < p.Length)
            {
               r += await observerClient.GetStream().ReadAsync(p, r, p.Length - r, cts.Token);
            }
            
            Console.WriteLine($"[TEST] Observer loop 1 received command: 0x{c1:X4}");

            if (c1 == ChatProtocol.Command.CHAT_CMD_CHANNEL_BAN) 
            {
                 Console.WriteLine("[TEST] Observer received BAN (0x0032).");
                 break;
            }
            // Continue if not BAN
        }

        await Assert.That(c1).IsEqualTo(ChatProtocol.Command.CHAT_CMD_CHANNEL_BAN);

        // 2. Act - Unban with [TAG]
        ChatBuffer unbanBuffer = new();
        unbanBuffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_UNBAN);
        unbanBuffer.WriteInt32(channelId);
        unbanBuffer.WriteString(targetDisplay); // Use dynamic Name with Tag ([PK])

        byte[] unbanPacket = unbanBuffer.Data.AsSpan(0, (int) unbanBuffer.Size).ToArray();
        List<byte> unbanRaw = [];
        unbanRaw.AddRange(BitConverter.GetBytes((ushort) unbanPacket.Length));
        unbanRaw.AddRange(unbanPacket);
        await adminClient.GetStream().WriteAsync(unbanRaw.ToArray(), cts.Token);

        // Assert - Observer receives UNBAN (0x0033)
        ushort c2 = 0;
        for(int i=0; i<maxRetries; i++)
        {
            byte[] h = new byte[4];
            await observerClient.GetStream().ReadExactlyAsync(h, cts.Token);
            c2 = BitConverter.ToUInt16(h, 2);
            ushort len = BitConverter.ToUInt16(h, 0);
            byte[] p = new byte[len - 2];
            int r = 0;
            while(r < p.Length)
            {
               r += await observerClient.GetStream().ReadAsync(p, r, p.Length - r, cts.Token);
            }
            
            Console.WriteLine($"[TEST] Observer loop 2 received command: 0x{c2:X4} Length: {len}");
            
            // If 0x0006, log details
            if (c2 == ChatProtocol.Command.CHAT_CMD_LEFT_CHANNEL)
            {
                ChatBuffer leftBuf = new(p);
                int lId = leftBuf.ReadInt32();
                int cId = leftBuf.ReadInt32();
                Console.WriteLine($"[TEST] Loop 2 Ignored LEFT_CHANNEL. Leaver: {lId}, Channel: {cId}");
            }

            if (c2 == ChatProtocol.Command.CHAT_CMD_CHANNEL_UNBAN) 
            {
                Console.WriteLine("[TEST] Observer received UNBAN (0x0033).");
                break;
            }
        }

        await Assert.That(c2).IsEqualTo(ChatProtocol.Command.CHAT_CMD_CHANNEL_UNBAN);
    }
}
