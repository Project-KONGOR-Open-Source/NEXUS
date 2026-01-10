using System.Net.Sockets;
using ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;
using KONGOR.MasterServer.Extensions.Cache;
using KONGOR.MasterServer.Handlers.SRP;
using MERRICK.DatabaseContext;
using MERRICK.DatabaseContext.Entities.Core;
using MERRICK.DatabaseContext.Entities.Relational;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using TRANSMUTANSTEIN.ChatServer.Domain.Core;
using TUnit.Assertions;
using TUnit.Core;

namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests;

public sealed class ChatSocialTests
{
    [Test]
    public async Task AddFriend_RequestSent()
    {
        // Arrange
        int testPort = 52500;
        await using TRANSMUTANSTEINServiceProvider app = await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(clientPort: testPort);
        
        // Ensure Target User Exists in DB (we do this by "connecting" them or manually seeding)
        // ChatTestHelpers.ConnectAndAuthenticateAsync seeds the user if they don't exist.
        // We'll just spin up the target first to ensure they exist.
        using TcpClient target = await ChatTestHelpers.ConnectAndAuthenticateAsync(app, testPort, 402, "TargetUser");
        
        using TcpClient requester = await ChatTestHelpers.ConnectAndAuthenticateAsync(app, testPort, 401, "RequesterUser");
        NetworkStream stream = requester.GetStream();
        
        // Act - Send Add Friend Request
        ChatBuffer addBuffer = new ChatBuffer();
        addBuffer.WriteCommand(ChatProtocol.Command.CHAT_CMD_REQUEST_BUDDY_ADD); // 0x000D
        addBuffer.WriteString("TargetUser");
        
        byte[] packet = addBuffer.Data.AsSpan(0, (int)addBuffer.Size).ToArray();
        List<byte> rawBytes = [];
        rawBytes.AddRange(BitConverter.GetBytes((ushort)packet.Length));
        rawBytes.AddRange(packet);
        await stream.WriteAsync(rawBytes.ToArray());
        
        // Assert - Expect CHAT_CMD_REQUEST_BUDDY_ADD_RESPONSE (0x00B2) with Success Status
        
        using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        
        byte[] header = new byte[4];
        await stream.ReadExactlyAsync(header, cts.Token);
        ushort length = BitConverter.ToUInt16(header, 0);
        ushort command = BitConverter.ToUInt16(header, 2);
        
        await Assert.That(command).IsEqualTo(ChatProtocol.Command.CHAT_CMD_REQUEST_BUDDY_ADD_RESPONSE); // 0x00B2
        
        // Read Payload
        int payloadSize = length - 2;
        byte[] payload = new byte[payloadSize];
        int read = 0;
        while (read < payloadSize)
        {
            read += await stream.ReadAsync(payload, read, payloadSize - read, cts.Token);
        }
        
        ChatBuffer responseBuffer = new ChatBuffer(payload);
        byte status = responseBuffer.ReadInt8();
        int notifId = responseBuffer.ReadInt32();
        string friendName = responseBuffer.ReadString();
        
        await Assert.That(status).IsEqualTo((byte)ChatProtocol.FriendAddStatus.SUCCESS_REQUESTER); // 1
        await Assert.That(friendName).IsEqualTo("TargetUser");
        await Assert.That(notifId).IsNotEqualTo(0);
    }
  
    [Test]
    public async Task BuddyList_InitialStatus()
    {
         // Arrange
        int testPort = 52600;
        await using TRANSMUTANSTEINServiceProvider app = await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(clientPort: testPort);
        
        // Create Users
        int accountIdMain = 501;
        int accountIdFriend = 502;
        
        // Seed Friendship manually
        await ChatTestHelpers._seedLock.WaitAsync();
        try
        {
            using (IServiceScope scope = app.Services.CreateScope())
            {
                MerrickContext db = scope.ServiceProvider.GetRequiredService<MerrickContext>();
                IDatabase cache = scope.ServiceProvider.GetRequiredService<IDatabase>();
                
                db.ChangeTracker.Clear();

                // Ensure Role
                Role? role = await db.Roles.FindAsync(1);
                if (role == null)
                {
                    role = new Role { ID = 1, Name = "User" };
                    db.Roles.Add(role);
                    await db.SaveChangesAsync();
                }
                role = await db.Roles.FindAsync(1);


                
                // Ensure Friend User (502)
                Account? existingFriend = await db.Accounts.FindAsync(accountIdFriend);
                if (existingFriend == null)
                {
                     // Re-fetch role
                     role = await db.Roles.FindAsync(1);
                     
                     User userFriend = new User
                     {
                        ID = accountIdFriend,
                        EmailAddress = $"friend{accountIdFriend}@test.com",
                        SRPPasswordHash = "hash",
                        SRPPasswordSalt = "salt",
                        PBKDF2PasswordHash = "hash",
                        Role = role!
                     };
                     Account friendAcct = new Account { ID = accountIdFriend, Name = "FriendGuy", IsMain = true, User = userFriend };
                     userFriend.Accounts = new List<Account> { friendAcct };
                     
                     db.Users.Add(userFriend);
                     await db.SaveChangesAsync();
                }

                // Ensure Main User (501) with Friendship
                if (await db.Accounts.FindAsync(accountIdMain) == null)
                {
                    role = await db.Roles.FindAsync(1);
                    
                    User userMain = new User
                    {
                        ID = accountIdMain,
                        EmailAddress = $"main{accountIdMain}@test.com",
                        SRPPasswordHash = "hash",
                        SRPPasswordSalt = "salt",
                        PBKDF2PasswordHash = "hash",
                        Role = role!
                    };
                    
                    Account mainAcct = new Account { ID = accountIdMain, Name = "MainGuy", IsMain = true, User = userMain };
                    userMain.Accounts = new List<Account> { mainAcct };
                    
                    // Link Friendship
                    // We need to reference the Friend Account Name/ID.
                    // Since we ensured it exists above, we can just use the values.
                    FriendedPeer peerLink = new FriendedPeer
                    {
                        ID = accountIdFriend,
                        Name = "FriendGuy",
                        ClanTag = null,
                        FriendGroup = "DEFAULT"
                    };
                    mainAcct.FriendedPeers = new List<FriendedPeer> { peerLink };
                    
                    db.Users.Add(userMain);
                    await db.SaveChangesAsync();
                }
                
                // Set Session Cookie for MainGuy (so helper can login)
                await cache.SetAccountNameForSessionCookie($"cookie_{accountIdMain}", "MainGuy");
                await cache.SetAccountNameForSessionCookie($"cookie_{accountIdFriend}", "FriendGuy");
            }
        }
        finally
        {
            ChatTestHelpers._seedLock.Release();
        }
        
        // Act
        // 1. Connect Friend (502) so they are "Online"
        using TcpClient friendClient = await ChatTestHelpers.ConnectAndAuthenticateAsync(app, testPort, accountIdFriend, "FriendGuy");
        
        // 2. Connect MainGuy (501) manually to inspect handshake
        TcpClient client = new TcpClient();
        await client.ConnectAsync("localhost", testPort);
        NetworkStream stream = client.GetStream();
        
        string ip = "127.0.0.1";
        string cookie = $"cookie_{accountIdMain}";
        string hash = SRPAuthenticationHandlers.ComputeChatServerCookieHash(accountIdMain, ip, cookie);

        // Send Login Packet (0x0C00)
        ChatBuffer loginBuffer = new ChatBuffer();
        loginBuffer.WriteCommand(ChatProtocol.ClientToChatServer.NET_CHAT_CL_CONNECT);
        loginBuffer.WriteInt32(accountIdMain);
        loginBuffer.WriteString(cookie);
        loginBuffer.WriteString(ip);
        loginBuffer.WriteString(hash);
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
        loginBuffer.WriteInt8((byte)ChatProtocol.ChatClientStatus.CHAT_CLIENT_STATUS_CONNECTED);
        loginBuffer.WriteInt8((byte)ChatProtocol.ChatModeType.CHAT_MODE_AVAILABLE);
        loginBuffer.WriteString("USE");
        loginBuffer.WriteString("en");
        
        byte[] loginPacket = loginBuffer.Data.AsSpan(0, (int)loginBuffer.Size).ToArray();
         ushort packetLength = (ushort)(loginPacket.Length); 
        List<byte> rawBytes = [];
        rawBytes.AddRange(BitConverter.GetBytes(packetLength));
        rawBytes.AddRange(loginPacket);
        await stream.WriteAsync(rawBytes.ToArray());
        
        // Read until we find INITIAL_STATUS (0x000B)
         using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
         bool foundBuddyList = false;
         
         while (!foundBuddyList)
         {
             byte[] header = new byte[4];
             await stream.ReadExactlyAsync(header, 0, 4, cts.Token);
             
             ushort length = BitConverter.ToUInt16(header, 0);
             ushort command = BitConverter.ToUInt16(header, 2);
             
             if (command == ChatProtocol.Command.CHAT_CMD_INITIAL_STATUS) // 0x000B
             {
                 foundBuddyList = true;
                 
                 // Parse Payload
                 // Payload = Length - 2
                 int pSize = length - 2;
                 byte[] p = new byte[pSize];
                 int pr = 0;
                 while(pr < pSize) pr += await stream.ReadAsync(p, pr, pSize - pr, cts.Token);
                 
                 ChatBuffer b = new ChatBuffer(p);
                 
                 // Structure: [Count: Int32] [List...]
                 int buddyCount = b.ReadInt32();
                 
                 await Assert.That(buddyCount).IsGreaterThan(0);
                 
                 // Verify first buddy is "FriendGuy" (AccountID 502)
                 // Loop: ID(4), Status(1), Flags(1), NameColor(String), Icon(String), [If InGame: Server(String), MatchID(4)], Level(4)
                 
                 int fId = b.ReadInt32();
                 // Status (1 byte)
                 b.ReadInt8();
                 // Flags (1 byte)
                 b.ReadInt8();
                 // NameColor (String)
                 b.ReadString(); // Name Color
                 // Icon (String)
                 b.ReadString(); // Icon
                 
                 // Check if we need to read extended info?
                 // Current server implementation:
                 // WriteInt32(ID)
                 // WriteInt8(Status)
                 // WriteInt8(Flags)
                 // WriteString(NameColor)
                 // WriteString(Icon)
                 // If InGame...
                 // WriteInt32(Level)
                 
                 await Assert.That(fId).IsEqualTo(502);
             }
             else
             {
                 // Consume unknown packet payload
                 int skip = length - 2;
                 if (skip > 0)
                 {
                     byte[] junk = new byte[skip];
                     int jr = 0;
                     while(jr < skip) jr += await stream.ReadAsync(junk, jr, skip - jr, cts.Token);
                 }
             }
         }
         
         await Assert.That(foundBuddyList).IsTrue();
    }
}
