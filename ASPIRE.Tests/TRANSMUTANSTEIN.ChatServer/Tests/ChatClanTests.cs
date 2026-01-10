using System.Net.Sockets;
using ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;
using KONGOR.MasterServer.Extensions.Cache;
using MERRICK.DatabaseContext.Entities.Relational;
using MERRICK.DatabaseContext.Entities.Core;
using TRANSMUTANSTEIN.ChatServer.Domain.Clans;
using TRANSMUTANSTEIN.ChatServer.Domain.Core;
using TRANSMUTANSTEIN.ChatServer.Services;

namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests;

public sealed class ChatClanTests
{
    [Test]
    public async Task ClanAdd_Accepted_Success()
    {
        // 1. Arrange
        int testPort = 53500;
        await using TRANSMUTANSTEINServiceProvider app =
            await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(testPort);

        int creatorId = 601;
        int joinerId = 602;
        int clanId = 100;
        string inviteKey = "pending_invite_key";

        // Seed DB and Redis
        await ChatTestHelpers._seedLock.WaitAsync();
        try
        {
            using IServiceScope scope = app.Services.CreateScope();
            MerrickContext db = scope.ServiceProvider.GetRequiredService<MerrickContext>();
            IPendingClanService pendingService = scope.ServiceProvider.GetRequiredService<IPendingClanService>();

            // Ensure Roles & Users
            // Joiner (User 602)
            if (await db.Accounts.FindAsync(joinerId) == null)
            {
                Role role = await db.Roles.FindAsync(1) ?? new Role { ID = 1, Name = "User" };
                if (role.ID == 0) db.Roles.Add(role);

                User userJoiner = new()
                {
                    ID = joinerId,
                    EmailAddress = $"joiner{joinerId}@test.com",
                    SRPPasswordHash = "hash",
                    SRPPasswordSalt = "salt",
                    PBKDF2PasswordHash = "hash",
                    Role = role
                };
                Account acctJoiner = new() { ID = joinerId, Name = "Joiner", IsMain = true, User = userJoiner };
                userJoiner.Accounts = new List<Account> { acctJoiner };
                db.Users.Add(userJoiner);
                await db.SaveChangesAsync();
            }

            // Creator (User 601) & existing Clan
            if (await db.Accounts.FindAsync(creatorId) == null)
            {
                Role? role = await db.Roles.FindAsync(1); // Should exist now

                User userCreator = new()
                {
                    ID = creatorId,
                    EmailAddress = $"creator{creatorId}@test.com",
                    SRPPasswordHash = "hash",
                    SRPPasswordSalt = "salt",
                    PBKDF2PasswordHash = "hash",
                    Role = role!
                };
                Account acctCreator = new() { ID = creatorId, Name = "Creator", IsMain = true, User = userCreator };
                userCreator.Accounts = new List<Account> { acctCreator };
                db.Users.Add(userCreator);
                
                // Create Clan
                Clan clan = new()
                {
                    ID = clanId,
                    Name = "TestClan",
                    Tag = "TEST",
                    TimestampCreated = DateTime.UtcNow
                };
                db.Clans.Add(clan);
                
                acctCreator.Clan = clan;
                acctCreator.ClanTier = ClanTier.Leader;

                await db.SaveChangesAsync();
            }
            
            pendingService.InsertPendingClanInvite(inviteKey, new PendingClanInvite
            {
                ClanId = clanId,
                InitiatorAccountId = creatorId,
                ClanName = "TestClan",
                ClanTag = "TEST",
                InvitedAccountId = joinerId,
                CreationTime = DateTime.UtcNow
            });
            
            // Map User to Invite Key - using private/internal logic helper NOT available? 
            // Wait, AddPendingClanInviteKeyForUser was assumed. Check Interface.
            // Interface has: string? GetPendingClanInviteKeyForUser(Account account);
            // It iterates _pendingClanInvites. 
            // The Invite object has InvitedAccountId. So GetPendingClanInviteKeyForUser should work automatically if Invite is inserted!
            // pendingService.InsertPendingClanInvite(inviteKey, ...); - Done above.
            
        }
        finally
        {
            ChatTestHelpers._seedLock.Release();
        }

        // 2. Act - Joiner Connects and sends CLAN_ADD_ACCEPTED
        using TcpClient joinerClient = await ChatTestHelpers.ConnectAndAuthenticateAsync(app, testPort, joinerId, "Joiner");
        NetworkStream stream = joinerClient.GetStream();

        ChatBuffer acceptedBuffer = new();
        acceptedBuffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_ADD_ACCEPTED); // 0x004F
        
        byte[] packet = acceptedBuffer.Data.AsSpan(0, (int)acceptedBuffer.Size).ToArray();
        List<byte> rawBytes = [];
        rawBytes.AddRange(BitConverter.GetBytes((ushort)packet.Length));
        rawBytes.AddRange(packet);
        await stream.WriteAsync(rawBytes.ToArray());

        // 3. Assert - Expect CHAT_CMD_NEW_CLAN_MEMBER (0x004E) logic
        
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));
        bool foundNewMember = false;

        while (!foundNewMember) 
        {
            byte[] header = new byte[4];
            int read = await stream.ReadAsync(header, 0, 4, cts.Token);
            if (read == 0) break; // Disconnected

            ushort length = BitConverter.ToUInt16(header, 0);
            ushort command = BitConverter.ToUInt16(header, 2);

            if (command == ChatProtocol.Command.CHAT_CMD_NEW_CLAN_MEMBER) // 0x004E
            {
                foundNewMember = true;
                
                int payloadSize = length - 2;
                byte[] payload = new byte[payloadSize];
                int pr = 0;
                while (pr < payloadSize) pr += await stream.ReadAsync(payload, pr, payloadSize - pr, cts.Token);
                
                ChatBuffer b = new(payload);
                int rxId = b.ReadInt32();
                int rxClanId = b.ReadInt32();
                string rxName = b.ReadString();
                string rxTag = b.ReadString();

                await Assert.That(rxId).IsEqualTo(joinerId);
                await Assert.That(rxClanId).IsEqualTo(clanId);
                await Assert.That(rxName).IsEqualTo("TestClan");
                await Assert.That(rxTag).IsEqualTo("TEST");
            }
            else
            {
                int skip = length - 2;
                if (skip > 0)
                {
                    byte[] junk = new byte[skip];
                    int jr = 0;
                    while (jr < skip) jr += await stream.ReadAsync(junk, jr, skip - jr, cts.Token);
                }
            }
        }
        
        await Assert.That(foundNewMember).IsEqualTo(true);
        
        // Verify DB
        using (IServiceScope scope = app.Services.CreateScope())
        {
             MerrickContext db = scope.ServiceProvider.GetRequiredService<MerrickContext>();
             Account? joiner = await db.Accounts
                .Include(a => a.Clan)
                .FirstOrDefaultAsync(a => a.ID == joinerId);
                
             await Assert.That(joiner).IsNotNull();
             await Assert.That(joiner!.Clan).IsNotNull();
             await Assert.That(joiner.Clan!.ID).IsEqualTo(clanId);
             await Assert.That(joiner.ClanTier).IsEqualTo(ClanTier.Member);
        }
    }

    [Test]
    public async Task ClanRemove_Notify_Passive_NoCrash()
    {
        // 1. Arrange
        int testPort = 53600;
        await using TRANSMUTANSTEINServiceProvider app =
            await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(testPort);
        
        int userId = 605;
        // User in Clan
         await ChatTestHelpers._seedLock.WaitAsync();
        try
        {
            using IServiceScope scope = app.Services.CreateScope();
            MerrickContext db = scope.ServiceProvider.GetRequiredService<MerrickContext>();
            Role role = await db.Roles.FindAsync(1) ?? new Role { ID = 1, Name = "User" };
            if (role.ID == 0) db.Roles.Add(role);
            
             User user = new() 
             { 
                 ID = userId, 
                 EmailAddress = $"user{userId}@test.com", 
                 SRPPasswordHash = "hash",
                 SRPPasswordSalt = "salt",
                 PBKDF2PasswordHash = "hash",
                 Role = role 
             };
             Account acct = new() { ID = userId, Name = "RemoveTestUser", IsMain = true, User = user };
             user.Accounts = new List<Account> { acct };
             db.Users.Add(user);
             await db.SaveChangesAsync();
        }
        finally { ChatTestHelpers._seedLock.Release(); }

        using TcpClient client = await ChatTestHelpers.ConnectAndAuthenticateAsync(app, testPort, userId, "RemoveTestUser");
        NetworkStream stream = client.GetStream();

        // 2. Act - Send CHAT_CMD_CLAN_REMOVE_NOTIFY (0x0017)
        ChatBuffer cmd = new();
        cmd.WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_REMOVE_NOTIFY); // 0x0017
        cmd.WriteInt32(userId); // Random payload (ID)
        
        byte[] packet = cmd.Data.AsSpan(0, (int)cmd.Size).ToArray();
        List<byte> rawBytes = [];
        rawBytes.AddRange(BitConverter.GetBytes((ushort)packet.Length));
        rawBytes.AddRange(packet);
        await stream.WriteAsync(rawBytes.ToArray());

        // 3. Assert - Client stays connected (No server crash/disconnect)
        await Task.Delay(500);
        
        await Assert.That(client.Connected).IsEqualTo(true);
    }
}
