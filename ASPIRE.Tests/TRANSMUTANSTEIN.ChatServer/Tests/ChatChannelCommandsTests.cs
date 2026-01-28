using System.Net.Sockets;

using ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;

using MERRICK.DatabaseContext;
using MERRICK.DatabaseContext.Entities.Core;
using MERRICK.DatabaseContext.Enumerations;

using TRANSMUTANSTEIN.ChatServer.Domain.Core;

namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests;


public sealed class ChatChannelCommandsTests
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

        Console.WriteLine($"[TEST] JoinChannel waiting for response...");
        while (true)
        {
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
                Console.WriteLine($"[TEST] JoinChannel Success. ID: {id}");
                return (id, command);
            }
            
            if (command == ChatProtocol.ChatServerToClient.NET_CHAT_CL_REJECT)
            {
                byte[] reasonByte = new byte[1];
                // Reject payload is usually reason byte? Re-checking protocol. 
                // Ah, above payload read already consumed 'length-2'. 
                // Reject (0x1C01) size?
                // ChatServer sends: WriteInt16(3); WriteInt16(REJECT); WriteByte(Reason). Total 5 bytes? 
                // Length=3. Header=4? No. Length includes command? Usually Length(2) + Cmd(2) + Payload.
                // If Length=3, Payload=1.
                // My code read `payloadSize` bytes.
                // So `payload` contains the reason byte.
                
                byte reason = payload.Length > 0 ? payload[0] : (byte)0;
                Console.WriteLine($"[TEST] JoinChannel Failed. REJECTED (0x1C01). Reason: {(ChatProtocol.ChatRejectReason)reason} ({reason})");
                return (0, command);
            }
            
            Console.WriteLine($"[TEST] JoinChannel Ignored Command: 0x{command:X4}");
        }
    }

    [Test]
    public async Task Roll_BroadcastsResult()
    {
        int testPort = 0;
        await using TRANSMUTANSTEINServiceProvider app =
            await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(testPort);

        using TcpClient client = await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, 555, "Roller");

        // Join Channel
        (int channelId, _) = await JoinChannelAndGetId(client, "RollRoom");

        // Act - Roll
        ChatBuffer rollBuffer = new();
        rollBuffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHAT_ROLL); // 0x0064
        rollBuffer.WriteString("100"); // Params
        rollBuffer.WriteInt32(channelId);

        byte[] packet = rollBuffer.Data.AsSpan(0, (int) rollBuffer.Size).ToArray();
        List<byte> rawBytes = [];
        rawBytes.AddRange(BitConverter.GetBytes((ushort) packet.Length));
        rawBytes.AddRange(packet);
        await client.GetStream().WriteAsync(rawBytes.ToArray());

        // Assert - Receive 0x0064
        // Skip potential noise (e.g. LEFT_CHANNEL 0x0006)
        ushort command = 0;
        int senderId = 0;
        int chanId = 0;
        string result = string.Empty;

        for (int i = 0; i < 5; i++)
        {
            byte[] header = new byte[4];
            await client.GetStream().ReadExactlyAsync(header);
            ushort length = BitConverter.ToUInt16(header, 0);
            command = BitConverter.ToUInt16(header, 2);

            byte[] payload = new byte[length - 2];
            await client.GetStream().ReadExactlyAsync(payload);

            if (command == ChatProtocol.Command.CHAT_CMD_CHAT_ROLL)
            {
                ChatBuffer response = new(payload);
                senderId = response.ReadInt32();
                chanId = response.ReadInt32();
                result = response.ReadString();
                break;
            }
        }

        await Assert.That(command).IsEqualTo(ChatProtocol.Command.CHAT_CMD_CHAT_ROLL);
        await Assert.That(senderId).IsEqualTo(555);
        await Assert.That(chanId).IsEqualTo(channelId);
        await Assert.That(result).Contains("rolled");
    }

    [Test]
    public async Task Promote_BroadcastsPromotion()
    {
        int testPort = 0;
        await using TRANSMUTANSTEINServiceProvider app =
            await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(testPort);

        int idAdmin = Random.Shared.Next(7000, 7999);
        int idUser = Random.Shared.Next(8000, 8999);

        using TcpClient adminClient = await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, idAdmin, "AdminUser");
        using TcpClient userClient = await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, idUser, "NormalUser");

        // Manually Promote AdminUser to Staff in DB since helper default is User/Normal.
        using (IServiceScope scope = app.Services.CreateScope())
        {
            MerrickContext db = scope.ServiceProvider.GetRequiredService<MerrickContext>();
            Account? adminAcc = await db.Accounts.Include(a => a.User).FirstOrDefaultAsync(a => a.ID == idAdmin);
            if (adminAcc != null)
            {
                // Create Staff Role if not exists
                Role? staffRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Staff");
                if (staffRole == null)
                {
                    staffRole = new Role { ID = 2, Name = "Staff" }; // Assuming ID 2 is safe
                    db.Roles.Add(staffRole);
                    await db.SaveChangesAsync();
                }
                adminAcc.User.Role = staffRole;
                db.Update(adminAcc.User);
                await db.SaveChangesAsync();
            }
        }

        // Admin Creates/Joins Channel
        (int channelId, _) = await JoinChannelAndGetId(adminClient, "PromoteRoom");

        // User Joins Channel
        await JoinChannelAndGetId(userClient, "PromoteRoom");

        // Act - Promote
        ChatBuffer buffer = new();
        buffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_PROMOTE); // 0x003A
        buffer.WriteInt32(channelId);
        buffer.WriteInt32(idUser); // Target ID

        byte[] packet = buffer.Data.AsSpan(0, (int) buffer.Size).ToArray();
        List<byte> rawBytes = [];
        rawBytes.AddRange(BitConverter.GetBytes((ushort) packet.Length));
        rawBytes.AddRange(packet);
        await adminClient.GetStream().WriteAsync(rawBytes.ToArray());

        // Assert - Receive 0x003A on user client
        byte[] header = new byte[4];
        await userClient.GetStream().ReadExactlyAsync(header);
        ushort length = BitConverter.ToUInt16(header, 0);
        ushort command = BitConverter.ToUInt16(header, 2);

        // Might receive JOINED_CHANNEL (0x0005) first if race condition, but usually User joins *after* admin so admin gets 0x0005, user gets 0x0004.
        // But here we wait for promotion.

        await Assert.That(command).IsEqualTo(ChatProtocol.Command.CHAT_CMD_CHANNEL_PROMOTE);

        byte[] payload = new byte[length - 2];
        await userClient.GetStream().ReadExactlyAsync(payload);

        ChatBuffer response = new(payload);
        int respChanId = response.ReadInt32();
        int respTargetId = response.ReadInt32();
        byte respRank = response.ReadInt8();

        await Assert.That(respChanId).IsEqualTo(channelId);
        await Assert.That(respTargetId).IsEqualTo(idUser);
        await Assert.That(respRank).IsEqualTo((byte) ChatProtocol.AdminLevel.CHAT_CLIENT_ADMIN_OFFICER);
    }

    [Test]
    public async Task Ban_BroadcastsBanAndKicksUser()
    {
        int testPort = 0;
        await using TRANSMUTANSTEINServiceProvider app =
            await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(testPort);

        int idAdmin = Random.Shared.Next(4000, 4999);
        int idVictim = Random.Shared.Next(5000, 5999);
        int idObserver = Random.Shared.Next(6000, 6999);

        using TcpClient adminClient = await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, idAdmin, "AdminBanner");
        using TcpClient userClient = await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, idVictim, "BannedUser");
        using TcpClient observerClient = await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, idObserver, "Observer");

        // Manually Promote AdminUser to Staff in DB
        using (IServiceScope scope = app.Services.CreateScope())
        {
            MerrickContext db = scope.ServiceProvider.GetRequiredService<MerrickContext>();
            Account? adminAcc = await db.Accounts.Include(a => a.User).FirstOrDefaultAsync(a => a.ID == idAdmin);
            if (adminAcc != null)
            {
                // Create Staff Role if not exists
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

        // Join
        (int channelId, _) = await JoinChannelAndGetId(adminClient, "BanRoom");
        await JoinChannelAndGetId(userClient, "BanRoom");
        await JoinChannelAndGetId(observerClient, "BanRoom");

        // Act - Ban
        ChatBuffer buffer = new();
        buffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_CHANNEL_BAN); // 0x0032
        buffer.WriteInt32(channelId);
        buffer.WriteString("BannedUser"); // Target Name (String)

        byte[] packet = buffer.Data.AsSpan(0, (int) buffer.Size).ToArray();
        List<byte> rawBytes = [];
        rawBytes.AddRange(BitConverter.GetBytes((ushort) packet.Length));
        rawBytes.AddRange(packet);
        await adminClient.GetStream().WriteAsync(rawBytes.ToArray());

        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

        // Assert - User should receive KICK (0x0031)
        // User might receive JOINED_CHANNEL (0x0005) or LEFT_CHANNEL (0x0006) as noise.
        ushort c1 = 0;
        for (int i = 0; i < 5; i++)
        {
            byte[] h1 = new byte[4];
            await userClient.GetStream().ReadExactlyAsync(h1, cts.Token);
            c1 = BitConverter.ToUInt16(h1, 2);
            ushort l1 = BitConverter.ToUInt16(h1, 0);
            byte[] p1 = new byte[l1 - 2];
            int r1 = 0;
            while(r1 < p1.Length)
            {
               r1 += await userClient.GetStream().ReadAsync(p1, r1, p1.Length - r1, cts.Token);
            }
            
            if (c1 == ChatProtocol.Command.CHAT_CMD_CHANNEL_KICK) break;
        }

        await Assert.That(c1).IsEqualTo(ChatProtocol.Command.CHAT_CMD_CHANNEL_KICK); // 0x0031

        // Assert - Observer should receive KICK (0x0031) then BAN (0x0032)
        // We might see LEFT_CHANNEL (0x0006) in between due to Kick implementation.
        

        ushort banCmd = 0;
        
        // Read 1: Expect Kick
        ushort finalKickCmd = 0;
        for (int i=0; i<5; i++)
        {
            Console.WriteLine($"[TEST] Observer waiting for KICK... Attempt {i}");
            byte[] oh1 = new byte[4];
            await observerClient.GetStream().ReadExactlyAsync(oh1, cts.Token);
            finalKickCmd = BitConverter.ToUInt16(oh1, 2);
            ushort len1 = BitConverter.ToUInt16(oh1, 0);
            Console.WriteLine($"[TEST] Observer received 0x{finalKickCmd:X4}");
            
            byte[] p1Ops = new byte[len1 - 2];
            int r2 = 0;
             while(r2 < p1Ops.Length)
            {
               r2 += await observerClient.GetStream().ReadAsync(p1Ops, r2, p1Ops.Length - r2, cts.Token);
            }

            if (finalKickCmd == ChatProtocol.Command.CHAT_CMD_CHANNEL_KICK) break;
        }
        
        await Assert.That(finalKickCmd).IsEqualTo(ChatProtocol.Command.CHAT_CMD_CHANNEL_KICK);

        // Read 2: Expect Ban (skipping Left Channel if present)
        for(int i=0; i<5; i++)
        {
            Console.WriteLine($"[TEST] Observer waiting for BAN... Attempt {i}");
            byte[] h = new byte[4];
            await observerClient.GetStream().ReadExactlyAsync(h, cts.Token);
            banCmd = BitConverter.ToUInt16(h, 2);
            ushort len = BitConverter.ToUInt16(h, 0);
             Console.WriteLine($"[TEST] Observer received 0x{banCmd:X4}");
             
            byte[] p = new byte[len - 2];
            int r3 = 0;
            while(r3 < p.Length)
            {
               r3 += await observerClient.GetStream().ReadAsync(p, r3, p.Length - r3, cts.Token);
            }
            
            if (banCmd == ChatProtocol.Command.CHAT_CMD_CHANNEL_BAN) break;
        }

        await Assert.That(banCmd).IsEqualTo(ChatProtocol.Command.CHAT_CMD_CHANNEL_BAN); 
    }
}
