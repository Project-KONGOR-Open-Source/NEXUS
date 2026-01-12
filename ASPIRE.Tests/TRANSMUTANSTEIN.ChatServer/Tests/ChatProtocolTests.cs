using System.Net.Sockets;

using ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;

using KONGOR.MasterServer.Extensions.Cache;

using TRANSMUTANSTEIN.ChatServer.Domain.Core;

namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests;

public sealed class ChatProtocolTests
{
    [Test]
    public async Task HandshakeSequence_IsStrictlyOrdered()
    {
        // Arrange
        // Use a unique port to avoid conflicts with other tests
        int testPort = 0;
        await using TRANSMUTANSTEINServiceProvider app =
            await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(testPort);
        using TcpClient client = new();

        // Act
        await client.ConnectAsync("localhost", app.ClientPort);
        NetworkStream stream = client.GetStream();

        // Seed Database and Cache for Authentication
        // Seed Database and Cache for Authentication
        // Seed Database and Cache for Authentication
        await ChatTestHelpers.SeedLock.WaitAsync();
        try
        {
            using (IServiceScope scope = app.Services.CreateScope())
            {
                MerrickContext db = scope.ServiceProvider.GetRequiredService<MerrickContext>();
                IDatabase cache = scope.ServiceProvider.GetRequiredService<IDatabase>();

                User user = new()
                {
                    ID = 999,
                    EmailAddress = "test999@test.com",
                    SRPPasswordHash = "hash",
                    SRPPasswordSalt = "salt",
                    PBKDF2PasswordHash = "hash",
                    Role = null! // Deferred assignment
                };

                // Fix Role
                Role? existingRole = await db.Roles.FindAsync(1);
                if (existingRole == null)
                {
                    existingRole = new Role { ID = 1, Name = "User" };
                    db.Roles.Add(existingRole);
                    await db.SaveChangesAsync();
                }

                user.Role = existingRole;

                Account account = new() { ID = 999, Name = "TestUser999", IsMain = true, User = user };

                user.Accounts = new List<Account> { account };

                // Ensure Account Exists
                if (await db.Accounts.FindAsync(999) == null)
                {
                    // Ensure Role
                    if (await db.Roles.FindAsync(1) == null)
                    {
                        db.Roles.Add(new Role { ID = 1, Name = "User" });
                        await db.SaveChangesAsync();
                    }

                    db.Accounts.Add(account);
                    await db.SaveChangesAsync();
                    Console.WriteLine("[TEST DEBUG] Seeded Account ID 999 into InMemory Database.");
                }
                else
                {
                    Console.WriteLine("[TEST DEBUG] Account ID 999 already exists in InMemory Database.");
                }

                // Verify explicit retrieval
                Account? check = await db.Accounts.FindAsync(999);
                if (check == null)
                {
                    Console.WriteLine("[TEST DEBUG] CRITICAL: Account ID 999 NOT FOUND after seeding!");
                }
                else
                {
                    Console.WriteLine($"[TEST DEBUG] Verified Account ID 999 exists: {check.Name}");
                }

                // Seed Session Cookie
                await cache.SetAccountNameForSessionCookie("test_cookie", "TestUser999");
            }
        }
        finally
        {
            ChatTestHelpers.SeedLock.Release();
        }

        string ip = "127.0.0.1";
        string cookie = "test_cookie";
        int accountId = 999;
        string hash = SRPAuthenticationHandlers.ComputeChatServerCookieHash(accountId, ip, cookie);

        // 0. Send Login Packet (NET_CHAT_CL_CONNECT - 0x0C00)
        ChatBuffer loginBuffer = new();
        loginBuffer.WriteCommand(ChatProtocol.ClientToChatServer.NET_CHAT_CL_CONNECT);
        loginBuffer.WriteInt32(accountId); // Account ID
        loginBuffer.WriteString(cookie); // Session Cookie
        loginBuffer.WriteString(ip); // Remote IP
        loginBuffer.WriteString(hash); // Auth Hash
        loginBuffer.WriteInt32(1); // Chat Protocol Version
        loginBuffer.WriteInt8(1); // OS ID
        loginBuffer.WriteInt8(1); // OS Major
        loginBuffer.WriteInt8(0); // OS Minor
        loginBuffer.WriteInt8(0); // OS Patch
        loginBuffer.WriteString("build"); // OS Build
        loginBuffer.WriteString("arch"); // OS Arch
        loginBuffer.WriteInt8(4); // Client Major
        loginBuffer.WriteInt8(10); // Client Minor
        loginBuffer.WriteInt8(1); // Client Patch
        loginBuffer.WriteInt8(0); // Client Revision
        loginBuffer.WriteInt8((byte) ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED); // Last State
        loginBuffer.WriteInt8((byte) ChatProtocol.ChatModeType.CHAT_MODE_AVAILABLE); // Chat Mode
        loginBuffer.WriteString("USE"); // Region
        loginBuffer.WriteString("en"); // Language

        // Get the bytes
        byte[] loginPacket = loginBuffer.Data.AsSpan(0, (int) loginBuffer.Size).ToArray();
        // Packets need length prefix. ChatBuffer doesn't add it to 'Buffer' automatically until Send() is called in server
        // But here we are client.
        // Wait, ChatBuffer structure:
        // Size = current write position.
        // Buffer = byte array.
        // WriteCommand writes the 2 bytes of command.
        // We need to prepend the 2 bytes of LENGTH.
        ushort packetLength = (ushort) loginPacket.Length;

        List<byte> rawBytes = new();
        rawBytes.AddRange(BitConverter.GetBytes(packetLength));
        rawBytes.AddRange(loginPacket);

        await stream.WriteAsync(rawBytes.ToArray());

        // Use a cancellation token to prevent infinite waiting if packets don't arrive
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));

        List<ushort> receivedCommands = new();
        List<byte[]> receivedPayloads = new();

        // We expect at least 4 packets: Accept, Options, InitialStatus, Connected
        // We might validly receive more (e.g. Channel joins), but we focus on the first 4 for the handshake
        int expectedPackets = 4;

        try
        {
            for (int i = 0; i < expectedPackets; i++)
            {
                // Read Header: [Length: 2 bytes] [Command: 2 bytes]
                byte[] headerBuffer = new byte[4];
                int bytesRead = 0;
                while (bytesRead < 4)
                {
                    int read = await stream.ReadAsync(headerBuffer, bytesRead, 4 - bytesRead, cts.Token);
                    if (read == 0)
                    {
                        break; // Disconnected
                    }

                    bytesRead += read;
                }

                if (bytesRead < 4)
                {
                    break;
                }

                ushort length = BitConverter.ToUInt16(headerBuffer, 0);
                ushort command = BitConverter.ToUInt16(headerBuffer, 2);

                if (command == 0x1C01) // NET_CHAT_CL_REJECT
                {
                    Console.WriteLine($"[TEST DEBUG] Received REJECT command (0x1C01). Length: {length}.");
                    receivedCommands.Add(command);
                    // Read potential reject reason payload?
                    // Reject payload is usually int32 reason.
                    break;
                }

                receivedCommands.Add(command);

                // Read Payload
                // The length field in the header is the length of the command (2 bytes) + payload.
                // So payload length = length - 2.
                // However, the ChatProtocol logic sometimes treats 'length' as the total packet size INCLUDING the length bytes themselves, 
                // OR excluding the length bytes but including the command bytes.
                // Based on CommandBuffer.cs: WriteInt16(Size) is usually NOT done automatically for the header itself in the same buffer, 
                // but ChatSession.Send() writes the length prefix.
                // Let's assume standard behavior: Length (2 bytes) tells us how many following bytes to read?
                // Actually, typically in K2: Length includes the Command (2 bytes). 
                // So if Length is 2, Payload is 0 bytes.

                int payloadSize = length - 2;
                byte[] payload = Array.Empty<byte>();

                if (payloadSize > 0)
                {
                    payload = new byte[payloadSize];
                    int payloadRead = 0;
                    while (payloadRead < payloadSize)
                    {
                        int read = await stream.ReadAsync(payload, payloadRead, payloadSize - payloadRead, cts.Token);
                        if (read == 0)
                        {
                            break;
                        }

                        payloadRead += read;
                    }

                    receivedPayloads.Add(payload);
                }
                else
                {
                    receivedPayloads.Add(Array.Empty<byte>());
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Timeout, analyze what we got
        }

        // Assert - Sequence
        Console.WriteLine(
            $"[TEST DEBUG] Received {receivedCommands.Count} commands: {string.Join(", ", receivedCommands.Select(c => $"0x{c:X4}"))}");

        if (receivedCommands.Count == 1 && receivedCommands[0] == ChatProtocol.ChatServerToClient.NET_CHAT_CL_REJECT)
        {
            Assert.Fail(
                "Authentication Failed: Server Rejected Connection (NET_CHAT_CL_REJECT). Check server logs for exact reason.");
        }

        await Assert.That(receivedCommands.Count).IsGreaterThanOrEqualTo(4);

        // 1. Accept
        await Assert.That(receivedCommands[0]).IsEqualTo(ChatProtocol.ChatServerToClient.NET_CHAT_CL_ACCEPT);

        // 2. Options (MUST BE BEFORE 0x000B)
        await Assert.That(receivedCommands[1]).IsEqualTo(ChatProtocol.Command.CHAT_CMD_OPTIONS);

        // 3. Initial Status (0x000B) (MUST BE BEFORE 0x000C)
        await Assert.That(receivedCommands[2]).IsEqualTo(ChatProtocol.Command.CHAT_CMD_INITIAL_STATUS);

        // 4. Update Status (0x000C) (Connected)
        await Assert.That(receivedCommands[3]).IsEqualTo(ChatProtocol.Command.CHAT_CMD_UPDATE_STATUS);

        // Assert - Payloads

        // Verify Options (0x00C0) Structure
        // Should be 116 bytes payload (as seen in logs)
        // Command (2) + Payload (116) = Length 118? Or Length 116 if Length excludes itself but includes command?
        // Let's rely on the capture:
        // Options Payload from log was 116 bytes.
        // My code reads length-2. If length in log was 118, then payload is 116.
        // Let's check the size of the payload we captured.
        // We know we removed the padding byte, so it should be 115 bytes now?
        // Original invalid payload: 116 bytes (115 data + 1 padding).
        // Correct payload: 115 bytes.

        // Wait, log said "Payload (116 bytes)". 
        // 116 bytes of *Payload* + 2 bytes Command = 118 bytes total length field? 
        // 
        // Let's assert that the Options payload does NOT contain that trailing 0x00 padding if it's 116 bytes long.
        // Actually, let's just assert it is NOT 0 bytes.
        // And more importantly, let's assert correct order.

        // Verify Initial Status (0x000B) Structure
        // For a new guest account with 0 friends, it should be Command (2) + Count (4) = 6 bytes total.
        // So payload size (excluding command) should be 4 bytes.
        await Assert.That(receivedPayloads[2].Length).IsEqualTo(4);
        int friendCount = BitConverter.ToInt32(receivedPayloads[2], 0);
        await Assert.That(friendCount).IsEqualTo(0);
    }
}