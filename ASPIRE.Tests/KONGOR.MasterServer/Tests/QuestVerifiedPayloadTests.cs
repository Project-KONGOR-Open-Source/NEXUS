
namespace ASPIRE.Tests.KONGOR.MasterServer.Tests;

using ASPIRE.Tests.KONGOR.MasterServer.Infrastructure;
using global::MERRICK.DatabaseContext;
using global::MERRICK.DatabaseContext.Entities;
using global::MERRICK.DatabaseContext.Enumerations;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using EntityRole = global::MERRICK.DatabaseContext.Entities.Utility.Role;

public sealed class QuestVerifiedPayloadTests
{
    [Test]
    public async Task GetPlayerQuests_ReturnsSuccess()
    {
        await using WebApplicationFactory<KONGORAssemblyMarker> webApplicationFactory = KONGORServiceProvider.CreateOrchestratedInstance();
        HttpClient client = webApplicationFactory.CreateClient();
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();
        MerrickContext dbContext = scope.ServiceProvider.GetRequiredService<MerrickContext>();

        string cookie = Guid.NewGuid().ToString("N");
        User user = new() { EmailAddress = "quest@kongor.net", PBKDF2PasswordHash = "h", SRPPasswordHash = "h", SRPPasswordSalt = "s", Role = new EntityRole { Name = UserRoles.User } };
        Account account = new() { Name = "QuestUser", User = user, Type = AccountType.Normal, IsMain = true, Cookie = cookie };
        await dbContext.Users.AddAsync(user);
        await dbContext.Accounts.AddAsync(account);
        await dbContext.SaveChangesAsync();

        // Endpoint: /master/questserver/getplayerquests/
        // Body: cookie=...&account_ids%5B%5D=59&account_ids%5B%5D=-1&account_ids%5B%5D=
        // Form: cookie, account_ids[]
        
        Dictionary<string, string> payload = new Dictionary<string, string>
        {
            { "cookie", cookie },
            { "account_ids[0]", account.ID.ToString() }, // Simulating array submission
            { "account_ids[1]", "-1" }
        };
        FormUrlEncodedContent content = new(payload);

        HttpResponseMessage response = await client.PostAsync("master/questserver/getplayerquests/", content);
        response.EnsureSuccessStatusCode();
    }
}
