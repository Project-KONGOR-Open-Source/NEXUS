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
        using TcpListener listener = new(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        int port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public static async Task<TcpClient> ConnectAndAuthenticateAsync(
        TRANSMUTANSTEINServiceProvider app,
        int port,
        int accountId,
        string accountName = "TestUser",
        string region = "USE",
        string language = "en")
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
                    Cookie = $"cookie_{accountId}"
                };

                user.Accounts = [account];

                db.Users.Add(user);
                await db.SaveChangesAsync();
            }
            else
            {
                // Update name if different (handling reruns with different names)
                if (existingAccount.Name != accountName)
                {
                    existingAccount.Name = accountName;
                    db.Accounts.Update(existingAccount);
                    await db.SaveChangesAsync();
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
                await client.ConnectAsync("localhost", port);
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

                    if (command == ChatProtocol.ChatServerToClient.NET_CHAT_CL_REJECT)
                    {
                        throw new Exception("Connection Rejected");
                    }

                    if (command == ChatProtocol.Command.CHAT_CMD_UPDATE_STATUS)
                    {
                        handshakeComplete = true;
                    }

                    // Consume Payload
                    int payloadSize = length - 2;
                    if (payloadSize > 0)
                    {
                        byte[] payload = new byte[payloadSize];
                        int payloadRead = 0;
                        while (payloadRead < payloadSize)
                        {
                            int read = await stream.ReadAsync(payload, payloadRead, payloadSize - payloadRead, cts.Token);
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
                Console.WriteLine($"[ChatTestHelpers] Connection Attempt {attempt + 1} Failed: {ex.Message}. Retrying...");
                
                if (attempt < maxRetries - 1)
                {
                    await Task.Delay(500); // Wait before retry
                }
            }
        }

        throw lastException ?? new Exception("Failed to connect to Chat Server after retries.");
    }
}