using System.Net.Sockets;
using ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Infrastructure;
using TRANSMUTANSTEIN.ChatServer.Domain.Clans;
using TRANSMUTANSTEIN.ChatServer.Domain.Core;

namespace ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer.Tests;

public sealed class ChatClanCreateTests
{
    [Test]
    public async Task ClanCreate_QueriesMembers_AndFailsOnOffline()
    {
        // 1. Arrange
        int testPort = 0;
        await using TRANSMUTANSTEINServiceProvider app =
            await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(testPort);

        int creatorId = 1000;
        List<string> memberNames = ["Mem1", "Mem2", "Mem3", "Mem4"];
        List<int> memberIds = [1001, 1002, 1003, 1004];

        // Seed DB
        await ChatTestHelpers.SeedLock.WaitAsync();
        try
        {
            using IServiceScope scope = app.Services.CreateScope();
            MerrickContext db = scope.ServiceProvider.GetRequiredService<MerrickContext>();

            // Ensure Role
            Role role = await db.Roles.FindAsync(1) ?? new Role { ID = 1, Name = "User" };
            if (role.ID == 0) db.Roles.Add(role);

            // Seed Creator
            if (await db.Accounts.FindAsync(creatorId) == null)
            {
                User user = new()
                {
                    ID = creatorId,
                    EmailAddress = $"creator{creatorId}@test.com",
                    SRPPasswordHash = "hash",
                    SRPPasswordSalt = "salt",
                    PBKDF2PasswordHash = "hash",
                    Role = role
                };
                Account acct = new() { ID = creatorId, Name = "Creator", IsMain = true, User = user };
                user.Accounts = new List<Account> { acct };
                db.Users.Add(user);
            }

            // Seed Members
            for (int i = 0; i < 4; i++)
            {
                int mid = memberIds[i];
                string mname = memberNames[i];
                if (await db.Accounts.FindAsync(mid) == null)
                {
                    User user = new()
                    {
                        ID = mid,
                        EmailAddress = $"mem{mid}@test.com",
                        SRPPasswordHash = "hash",
                        SRPPasswordSalt = "salt",
                        PBKDF2PasswordHash = "hash",
                        Role = role
                    };
                    Account acct = new() { ID = mid, Name = mname, IsMain = true, User = user };
                    user.Accounts = new List<Account> { acct };
                    db.Users.Add(user);
                }
            }
            await db.SaveChangesAsync();
        }
        finally
        {
            ChatTestHelpers.SeedLock.Release();
        }

        // 2. Act
        using TcpClient client = await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, creatorId, "Creator");
        NetworkStream stream = client.GetStream();

        ChatBuffer createCmd = new();
        createCmd.WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_REQUEST); // 0x0051
        createCmd.WriteString("NewClan");
        createCmd.WriteString("NCLN");
        foreach (string m in memberNames)
        {
            createCmd.WriteString(m);
        }

        await ChatTestHelpers.SendPacketAsync(stream, createCmd);

        // 3. Assert
        // Expect CHAT_CMD_CLAN_CREATE_FAIL_INVITE (0x0056) because members are offline
        // This confirms MembersSanityCheck passed (it found them in DB).

        ChatBuffer response = await ChatTestHelpers.ExpectCommandAsync(stream, ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_INVITE);
        // The payload for FAIL_INVITE is just the name of the user who failed (offline)
        // Usually the first one found offline.
        string failedMember = response.ReadString();

        await Assert.That(memberNames).Contains(failedMember);
    }

    [Test]
    public async Task ClanCreate_RejectsInvalidNameAndTag()
    {
        // 1. Arrange
        int testPort = 0;
        await using TRANSMUTANSTEINServiceProvider app =
            await TRANSMUTANSTEINServiceProvider.CreateOrchestratedInstanceAsync(testPort);

        int creatorId = 2000;
        string creatorName = "Creator2";

        // Seed DB
        await ChatTestHelpers.SeedLock.WaitAsync();
        try
        {
            using IServiceScope scope = app.Services.CreateScope();
            MerrickContext db = scope.ServiceProvider.GetRequiredService<MerrickContext>();

            // Ensure Role
            Role role = await db.Roles.FindAsync(1) ?? new Role { ID = 1, Name = "User" };
            if (role.ID == 0) db.Roles.Add(role);

            // Seed Creator
            if (await db.Accounts.FindAsync(creatorId) == null)
            {
                User user = new()
                {
                    ID = creatorId,
                    EmailAddress = $"creator{creatorId}@test.com",
                    SRPPasswordHash = "hash",
                    SRPPasswordSalt = "salt",
                    PBKDF2PasswordHash = "hash",
                    Role = role
                };
                Account acct = new() { ID = creatorId, Name = creatorName, IsMain = true, User = user };
                user.Accounts = new List<Account> { acct };
                db.Users.Add(user);
                await db.SaveChangesAsync();
            }
        }
        finally
        {
            ChatTestHelpers.SeedLock.Release();
        }

        // 2. Act & Assert
        using TcpClient client = await ChatTestHelpers.ConnectAndAuthenticateAsync(app, app.ClientPort, creatorId, creatorName);
        NetworkStream stream = client.GetStream();

        // Test Invalid Name
        ChatBuffer invalidNameCmd = new();
        invalidNameCmd.WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_REQUEST);
        invalidNameCmd.WriteString("Invalid@Name"); // Invalid char
        invalidNameCmd.WriteString("TAG");
        invalidNameCmd.WriteString("M1");
        invalidNameCmd.WriteString("M2");
        invalidNameCmd.WriteString("M3");
        invalidNameCmd.WriteString("M4");

        await ChatTestHelpers.SendPacketAsync(stream, invalidNameCmd);
        await ChatTestHelpers.ExpectCommandAsync(stream, ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_NAME);

        // Test Invalid Tag
        ChatBuffer invalidTagCmd = new();
        invalidTagCmd.WriteCommand(ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_REQUEST);
        invalidTagCmd.WriteString("Valid Name");
        invalidTagCmd.WriteString("T@G"); // Invalid char
        invalidTagCmd.WriteString("M1");
        invalidTagCmd.WriteString("M2");
        invalidTagCmd.WriteString("M3");
        invalidTagCmd.WriteString("M4");

        await ChatTestHelpers.SendPacketAsync(stream, invalidTagCmd);
        await ChatTestHelpers.ExpectCommandAsync(stream, ChatProtocol.Command.CHAT_CMD_CLAN_CREATE_FAIL_TAG);
    }
}
