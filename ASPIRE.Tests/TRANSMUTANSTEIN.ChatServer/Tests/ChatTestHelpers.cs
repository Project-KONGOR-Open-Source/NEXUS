using System.Net;
using System.Net.Sockets;

using ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;

using KONGOR.MasterServer.Extensions.Cache;

using TRANSMUTANSTEIN.ChatServer.Domain.Core;

namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests;

public static class ChatTestHelpers
{
    public static readonly SemaphoreSlim SeedLock = new(1, 1);

    public static int GetAvailablePort()
    {
        using TcpListener listener = new(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint) listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public static async Task<TcpClient> ConnectAndAuthenticateAsync(
        TRANSMUTANSTEINServiceProvider app,
        int port,
        int accountId,
        string accountName = "TestUser",
        string region = "USE",
        string language = "en",
        string? clanName = null,
        string? clanTag = null,
        AccountType accountType = AccountType.Normal)
    {
        // Seed Database and Cache
        // Serialize seeding to avoid race conditions in InMemory DB
        await SeedLock.WaitAsync();
        try
        {
            using IServiceScope scope = app.Services.CreateScope();
            MerrickContext db = scope.ServiceProvider.GetRequiredService<MerrickContext>();
            IDatabase cache = scope.ServiceProvider.GetRequiredService<IDatabase>();

            db.ChangeTracker.Clear();

            // 1. Ensure Role
            Role? role = await db.Roles.FindAsync(1);
            if (role == null)
            {
                role = new Role { ID = 1, Name = "User" };
                db.Roles.Add(role);
                await db.SaveChangesAsync();
            }

            // 2. Ensure User/Account
            Account? existingAccount = await db.Accounts.FindAsync(accountId);
            if (existingAccount == null)
            {
                role = await db.Roles.FindAsync(1);

                User user = new()
                {
                    ID = accountId,
                    EmailAddress = $"test{accountId}@test.com",
                    SRPPasswordHash = "hash",
                    SRPPasswordSalt = "salt",
                    PBKDF2PasswordHash = "hash",
                    Role = role!
                };

                Account account = new()
                {
                    ID = accountId,
                    Name = accountName,
                    IsMain = true,
                    User = user,
                    Cookie = $"cookie_{accountId}",
                    Type = accountType
                };

                user.Accounts = [account];

                db.Users.Add(user);
                await db.SaveChangesAsync();

                existingAccount = account;
            }
            else
            {
                // Update name/type if different (handling reruns)
                bool changed = false;
                if (existingAccount.Name != accountName)
                {
                    existingAccount.Name = accountName;
                    changed = true;
                }
                if (existingAccount.Type != accountType)
                {
                    existingAccount.Type = accountType;
                    changed = true;
                }
                
                if (changed)
                {
                    db.Accounts.Update(existingAccount);
                    await db.SaveChangesAsync();
                }
            }

            // 3. Ensure Clan
            if (clanName is not null && clanTag is not null)
            {
                Clan? clan = await db.Clans.FirstOrDefaultAsync(c => c.Name == clanName);
                if (clan == null)
                {
                    clan = new Clan { Name = clanName, Tag = clanTag };
                    db.Clans.Add(clan);
                    await db.SaveChangesAsync();
                }

                if (existingAccount.Clan?.ID != clan.ID)
                {
                    // Reload account with Clan navigation to ensure we don't cause tracking issues
                    // Or just set it if we trust current state.
                    // Since we did FindAsync above, it might be tracked.
                    // But we might have just created it.
                    existingAccount.Clan = clan;
                    existingAccount.ClanTier = ClanTier.Leader; // Assume leader if creating/joining for test simplicity
                    db.Accounts.Update(existingAccount);
                    await db.SaveChangesAsync();

                    Console.WriteLine($"[ChatTestHelpers] Updated Account {existingAccount.Name} with Clan {clan.Name}");

                    // Verify
                    db.ChangeTracker.Clear();
                    Account? verify = await db.Accounts.Include(a => a.Clan).FirstOrDefaultAsync(a => a.ID == accountId);
                    Console.WriteLine($"[ChatTestHelpers] Verified Account {verify?.Name} Clan: {verify?.Clan?.Name ?? "NULL"}");
                }
            }

            await cache.SetAccountNameForSessionCookie($"cookie_{accountId}", accountName);
        }
        finally
        {
            SeedLock.Release();
        }

        const int maxRetries = 3;
        Exception? lastException = null;

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            TcpClient? client = null;
            try
            {
                client = new TcpClient();
                // Use 127.0.0.1 to avoid IPv6/DNS issues when server is bound to 0.0.0.0
                Console.WriteLine($"[ChatTestHelpers] Connecting to 127.0.0.1:{port}...");
                await client.ConnectAsync("127.0.0.1", port);
                NetworkStream stream = client.GetStream();

                string ip = "127.0.0.1";
                string cookie = $"cookie_{accountId}";
                string hash = SRPAuthenticationHandlers.ComputeChatServerCookieHash(accountId, ip, cookie);

                // Send Login Packet (0x0C00)
                ChatBuffer loginBuffer = new();
                loginBuffer.WriteCommand(ChatProtocol.ClientToChatServer.NET_CHAT_CL_CONNECT);
                loginBuffer.WriteInt32(accountId);
                loginBuffer.WriteString(cookie);
                loginBuffer.WriteString(ip);
                loginBuffer.WriteString(hash);
                // ... (rest of handshake payload remains same, simplified context for brevity but keeping full logic)
                loginBuffer.WriteInt32(1); // Protocol Version
                loginBuffer.WriteInt8(1); // OS ID
                loginBuffer.WriteInt8(1); // OS Major
                loginBuffer.WriteInt8(0); // OS Minor
                loginBuffer.WriteInt8(0); // OS Patch
                loginBuffer.WriteString("build");
                loginBuffer.WriteString("arch");
                loginBuffer.WriteInt8(4); // Major
                loginBuffer.WriteInt8(10); // Minor
                loginBuffer.WriteInt8(1); // Patch
                loginBuffer.WriteInt8(0); // Revision
                loginBuffer.WriteInt8((byte) ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED);
                loginBuffer.WriteInt8((byte) ChatProtocol.ChatModeType.CHAT_MODE_AVAILABLE);
                loginBuffer.WriteString(region);
                loginBuffer.WriteString(language);

                // Prepend Length
                byte[] loginPacket = loginBuffer.Data.AsSpan(0, (int) loginBuffer.Size).ToArray();
                ushort packetLength = (ushort) loginPacket.Length;
                List<byte> rawBytes = [];
                rawBytes.AddRange(BitConverter.GetBytes(packetLength));
                rawBytes.AddRange(loginPacket);

                await stream.WriteAsync(rawBytes.ToArray());

                // Read through the handshake to ensure we are connected
                using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));
                bool handshakeComplete = false;

                while (!handshakeComplete)
                {
                    // Header
                    byte[] header = new byte[4];
                    int bytesRead = 0;
                    while (bytesRead < 4)
                    {
                        int read = await stream.ReadAsync(header, bytesRead, 4 - bytesRead, cts.Token);
                        if (read == 0)
                        {
                            throw new Exception("Disconnected during handshake");
                        }

                        bytesRead += read;
                    }

                    ushort length = BitConverter.ToUInt16(header, 0);
                    ushort command = BitConverter.ToUInt16(header, 2);
                    
                    Console.WriteLine($"[ChatTestHelpers] Packet: 0x{command:X4}, Len: {length}");

                    if (command == ChatProtocol.ChatServerToClient.NET_CHAT_CL_REJECT)
                    {
                        throw new Exception("Connection Rejected (0x1C01)");
                    }

                    if (command == ChatProtocol.Command.CHAT_CMD_UPDATE_STATUS)
                    {
                        handshakeComplete = true; // Still consume payload
                    }
                    
                    // Consume Payload
                    int payloadSize = length - 2;
                    if (payloadSize > 0)
                    {
                        byte[] payload = new byte[payloadSize];
                        int payloadRead = 0;
                        while (payloadRead < payloadSize)
                        {
                            int read = await stream.ReadAsync(payload, payloadRead, payloadSize - payloadRead,
                                cts.Token);
                            if (read == 0)
                            {
                                throw new Exception("Disconnected reading payload");
                            }

                            payloadRead += read;
                        }
                    }
                }

                // If success, return the connected client
                return client;
            }
            catch (Exception ex)
            {
                lastException = ex;
                client?.Dispose(); // CLEANUP FAILED CLIENT

                // Log retry (optional, console for now)
                Console.WriteLine(
                    $"[ChatTestHelpers] Connection Attempt {attempt + 1} Failed: {ex.Message}. Retrying...");

                if (attempt < maxRetries - 1)
                {
                    await Task.Delay(500); // Wait before retry
                }
            }
        }

        throw lastException ?? new Exception("Failed to connect to Chat Server after retries.");
    }

    public static async Task ExpectCommandAsync(NetworkStream stream, ushort expectedCommand, int timeoutSeconds = 5)
    {
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(timeoutSeconds));

        // Read until we find the command or timeout
        // Note: In a real scenario we might need to buffer and parse fully if commands are interleaved,
        // but for strict test sequences, we expect it to be the next one.
        // However, we must skip potential heartbeats/status updates if necessary, or just fail if strict.
        // For this test, we assume strict ordering or simple noise filtering.

        while (true)
        {
            byte[] h = new byte[4];
            int read = 0;
            while (read < 4)
            {
                read += await stream.ReadAsync(h, read, 4 - read, cts.Token);
            }

            ushort len = BitConverter.ToUInt16(h, 0);
            ushort cmd = BitConverter.ToUInt16(h, 2);

            if (cmd == expectedCommand)
            {
                // Consume payload
                int payloadSize = len - 2;
                if (payloadSize > 0)
                {
                    byte[] payload = new byte[payloadSize];
                    int pread = 0;
                    while (pread < payloadSize)
                    {
                        pread += await stream.ReadAsync(payload, pread, payloadSize - pread, cts.Token);
                    }
                }
                return;
            }

            // If strictly expecting it to be NEXT, we should fail if it's not.
            // But if we want to skip noise (e.g. 0x0006 LEFT_CHANNEL), we can use logic here.
            // For now, let's act strict but allow 0x0006/0x0005/0x0004 skip if needed, or just strict equality.
            // Based on previous test fixes, filtering noise is good.
            if (cmd is 0x0004 or 0x0005 or 0x0006 or 0x000D) // Changed/Joined/Left/BuddyReq
            {
                // Consume payload and continue
                int payloadSize = len - 2;
                if (payloadSize > 0)
                {
                    byte[] payload = new byte[payloadSize];
                    int pread = 0;
                    while (pread < payloadSize)
                    {
                        pread += await stream.ReadAsync(payload, pread, payloadSize - pread, cts.Token);
                    }
                }
                continue;
            }

            throw new Exception($"Expected command 0x{expectedCommand:X4} but got 0x{cmd:X4}");
        }
    }

    public static async Task<int> JoinChannelAndGetId(TcpClient client, string channelName)
    {
        ChatBuffer join = new();
        join.WriteCommand(ChatProtocol.Command.CHAT_CMD_JOIN_CHANNEL);
        join.WriteString(channelName);
        
        byte[] packetBytes = join.Data.AsSpan(0, (int) join.Size).ToArray();
        List<byte> r = [];
        r.AddRange(BitConverter.GetBytes((ushort) packetBytes.Length));
        r.AddRange(packetBytes);
        
        await client.GetStream().WriteAsync(r.ToArray());

        // Read 0x0004 response to get ID
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));
        while (true)
        {
            byte[] h = new byte[4];
            int readTotal = 0;
            while (readTotal < 4)
            {
                int rread = await client.GetStream().ReadAsync(h, readTotal, 4 - readTotal, cts.Token);
                if (rread == 0) throw new Exception("Disconnected while waiting for Join response");
                readTotal += rread;
            }

            ushort l = BitConverter.ToUInt16(h, 0);
            ushort c = BitConverter.ToUInt16(h, 2);
            
            byte[] py = new byte[l - 2];
            int readPy = 0;
            while (readPy < py.Length)
            {
                readPy += await client.GetStream().ReadAsync(py, readPy, py.Length - readPy, cts.Token);
            }

            if (c == ChatProtocol.Command.CHAT_CMD_CHANGED_CHANNEL) // 0x0004
            {
                ChatBuffer b = new(py);
                b.ReadString(); // Name
                return b.ReadInt32(); // ID
            }
            
            // Ignore noise (e.g. 0x0006 LEFT, 0x0005 JOINED)
            // But if we get LEFT for SELF, we are kicked?
            // Assuming noise for now.
        }
    }

    public static async Task SendPacketAsync(NetworkStream stream, ChatBuffer buffer)
    {
        byte[] packet = buffer.Data.AsSpan(0, (int) buffer.Size).ToArray();
        List<byte> rawPacket = [];
        rawPacket.AddRange(BitConverter.GetBytes((ushort) packet.Length));
        rawPacket.AddRange(packet);
        await stream.WriteAsync(rawPacket.ToArray());
    }
}