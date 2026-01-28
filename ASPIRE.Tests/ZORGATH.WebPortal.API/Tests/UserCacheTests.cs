using System.Net.Http.Headers;
using System.Net.Http.Json;

using ASPIRE.Common.DTOs;

namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Tests;

public sealed class UserCacheTests
{
    [Test]
    public async Task GetUser_ReturnsCachedResponse_WhenCalledMultipleTimes()
    {
        // 1. Setup
        await using WebApplicationFactory<ZORGATHAssemblyMarker> factory = ZORGATHServiceProvider.CreateOrchestratedInstance();
        JWTAuthenticationService authService = new(factory);

        // Create a user
        JWTAuthenticationData authData = await authService.CreateAuthenticatedUser("cacheuser@test.com", "CacheUser", "Password123!");

        using HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authData.AuthenticationToken);

        // 2. First Request (Prime Cache)
        HttpResponseMessage response1 = await client.GetAsync($"/User/{authData.UserID}");
        response1.EnsureSuccessStatusCode();
        GetBasicUserDTO? result1 = await response1.Content.ReadFromJsonAsync<GetBasicUserDTO>();

        // 3. Modify Data in DB Directly
        using (IServiceScope scope = factory.Services.CreateScope())
        {
            MerrickContext context = scope.ServiceProvider.GetRequiredService<MerrickContext>();
            Account account = await context.Accounts.SingleAsync(a => a.Name == "CacheUser");
            account.Name = "ModifiedUser";
            await context.SaveChangesAsync();
        }

        // 4. Second Request (Should be Cached)
        HttpResponseMessage response2 = await client.GetAsync($"/User/{authData.UserID}");
        response2.EnsureSuccessStatusCode();
        GetBasicUserDTO? result2 = await response2.Content.ReadFromJsonAsync<GetBasicUserDTO>();

        // 5. Assert
        await Assert.That(result1).IsNotNull();
        await Assert.That(result2).IsNotNull();

        // The first result should have the original name
        await Assert.That(result1!.Accounts.First().Name).IsEqualTo("CacheUser");

        // The second result should ALSO have the original name because it was served from cache
        // If caching was disabled, this would be "ModifiedUser"
        await Assert.That(result2!.Accounts.First().Name).IsEqualTo("CacheUser");
    }

    [Test]
    public async Task GetUser_VariesByAuthorization_WhenDifferentUsersRequestSameResource()
    {
        // 1. Setup
        await using WebApplicationFactory<ZORGATHAssemblyMarker> factory = ZORGATHServiceProvider.CreateOrchestratedInstance();
        JWTAuthenticationService authService = new(factory);

        // User A
        JWTAuthenticationData authDataA = await authService.CreateAuthenticatedUser("userA@test.com", "UserA", "Password123!");
        using HttpClient clientA = factory.CreateClient();
        clientA.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authDataA.AuthenticationToken);

        // User B
        JWTAuthenticationData authDataB = await authService.CreateAuthenticatedUser("userB@test.com", "UserB", "Password123!");
        using HttpClient clientB = factory.CreateClient();
        clientB.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authDataB.AuthenticationToken);

        // 2. User A requests User A (Prime Cache for A)
        HttpResponseMessage responseA1 = await clientA.GetAsync($"/User/{authDataA.UserID}");
        responseA1.EnsureSuccessStatusCode();
        GetBasicUserDTO? resultA1 = await responseA1.Content.ReadFromJsonAsync<GetBasicUserDTO>();

        // 3. Modify User A in DB
        using (IServiceScope scope = factory.Services.CreateScope())
        {
            MerrickContext context = scope.ServiceProvider.GetRequiredService<MerrickContext>();
            Account accountA = await context.Accounts.SingleAsync(a => a.Name == "UserA");
            accountA.Name = "ModifiedUserA";
            await context.SaveChangesAsync();
        }

        // 4. User B requests User A
        // User B has a different token, so it should generate a DIFFERENT cache key (if VaryByHeader is working).
        // It should therefore execute the handler and see the NEW data.
        HttpResponseMessage responseB = await clientB.GetAsync($"/User/{authDataA.UserID}");
        responseB.EnsureSuccessStatusCode();
        GetBasicUserDTO? resultB = await responseB.Content.ReadFromJsonAsync<GetBasicUserDTO>();

        // 5. Assert
        await Assert.That(resultA1).IsNotNull();
        await Assert.That(resultB).IsNotNull();

        // User A saw original data
        await Assert.That(resultA1!.Accounts.First().Name).IsEqualTo("UserA");

        // User B should see modified data because they got a fresh response
        // If VaryByHeader was missing, User B might have received User A's cached response ("UserA")
        await Assert.That(resultB!.Accounts.First().Name).IsEqualTo("ModifiedUserA");
    }
}
