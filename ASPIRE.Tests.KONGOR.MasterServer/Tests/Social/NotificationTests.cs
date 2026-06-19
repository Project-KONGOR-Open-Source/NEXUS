namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Social;

/// <summary>
///     Tests for the notification lifecycle: removal through the delete-notification endpoint, and the re-delivery of missed or ignored notifications on login.
/// </summary>
public sealed class NotificationTests(KONGORIntegrationWebApplicationFactory webApplicationFactory)
{
    [Before(HookType.Test)]
    public Task Before_Each_Test()
        => webApplicationFactory.WithSQLServerContainer().WithDistributedCacheContainer().InitialiseAsync();

    [Test]
    public async Task Deleting_A_Friend_Request_Notification_Removes_The_Pending_Request_And_Returns_Success()
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        (Account requesterAccount, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials("delete_notification_requester@kongor.com", "Requester", "SecurePassword123!");
        SRPAuthenticationData targetAuthentication = await srpAuthenticationService.AuthenticateWithSRP("delete_notification_target@kongor.com", "Target", "SecurePassword123!");

        string cookie = targetAuthentication.Cookie ?? throw new InvalidOperationException("Cookie Is NULL");

        const int targetNotificationID = 810002;

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        IDatabase distributedCacheStore = scope.ServiceProvider.GetRequiredService<IDatabase>();

        await distributedCacheStore.SetFriendRequest(requesterAccount.ID, targetAuthentication.Account.ID, targetNotificationID);

        HttpClient httpClient = webApplicationFactory.CreateClient();

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "S2 Games/Heroes Of Newerth/4.10.1.0/wac/x86_64");

        Dictionary<string, string> formData = new ()
        {
            { "account_id", targetAuthentication.Account.ID.ToString() },
            { "cookie", cookie },
            { "notify_id", targetNotificationID.ToString() },
            { "internal_id", "0" }
        };

        HttpResponseMessage response = await httpClient.PostAsync("client_requester.php?f=delete_notification", new FormUrlEncodedContent(formData));

        string responseBody = await response.Content.ReadAsStringAsync();

        IDictionary<object, object>? responseData = PhpSerialization.Deserialize(responseBody) as IDictionary<object, object>;

        using (Assert.Multiple())
        {
            await Assert.That(response.IsSuccessStatusCode).IsTrue();
            await Assert.That(responseData?["status"] as string).IsEqualTo("OK");
            await Assert.That(responseData?["notify_id"] as string).IsEqualTo(targetNotificationID.ToString());
            await Assert.That(responseData?["internal_id"] as string).IsEqualTo("0");
            await Assert.That(await distributedCacheStore.PendingFriendRequestExists(requesterAccount.ID, targetAuthentication.Account.ID)).IsFalse();
            await Assert.That((await distributedCacheStore.GetPendingFriendRequestsForAccount(targetAuthentication.Account.ID)).Count).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Deleting_A_Notification_With_No_Matching_Backing_Entry_Still_Returns_Success()
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        SRPAuthenticationData targetAuthentication = await srpAuthenticationService.AuthenticateWithSRP("idempotent_delete_target@kongor.com", "Recipient", "SecurePassword123!");

        string cookie = targetAuthentication.Cookie ?? throw new InvalidOperationException("Cookie Is NULL");

        HttpClient httpClient = webApplicationFactory.CreateClient();

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "S2 Games/Heroes Of Newerth/4.10.1.0/wac/x86_64");

        Dictionary<string, string> formData = new ()
        {
            { "account_id", targetAuthentication.Account.ID.ToString() },
            { "cookie", cookie },
            { "notify_id", "999999" },
            { "internal_id", "0" }
        };

        HttpResponseMessage firstResponse = await httpClient.PostAsync("client_requester.php?f=delete_notification", new FormUrlEncodedContent(formData));
        HttpResponseMessage secondResponse = await httpClient.PostAsync("client_requester.php?f=delete_notification", new FormUrlEncodedContent(formData));

        IDictionary<object, object>? firstResponseData = PhpSerialization.Deserialize(await firstResponse.Content.ReadAsStringAsync()) as IDictionary<object, object>;
        IDictionary<object, object>? secondResponseData = PhpSerialization.Deserialize(await secondResponse.Content.ReadAsStringAsync()) as IDictionary<object, object>;

        using (Assert.Multiple())
        {
            await Assert.That(firstResponse.IsSuccessStatusCode).IsTrue();
            await Assert.That(secondResponse.IsSuccessStatusCode).IsTrue();
            await Assert.That(firstResponseData?["status"] as string).IsEqualTo("OK");
            await Assert.That(secondResponseData?["status"] as string).IsEqualTo("OK");
        }
    }

    [Test]
    public async Task Pending_Friend_Requests_Are_Enumerated_Only_For_The_Target_Account()
    {
        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        IDatabase distributedCacheStore = scope.ServiceProvider.GetRequiredService<IDatabase>();

        const int requesterID = 820001;
        const int targetID = 820002;
        const int targetNotificationID = 820012;

        await distributedCacheStore.SetFriendRequest(requesterID, targetID, targetNotificationID);

        List<(int RequesterAccountID, int NotificationID, DateTimeOffset CreatedAt)> incomingForTarget = await distributedCacheStore.GetPendingFriendRequestsForAccount(targetID);
        List<(int RequesterAccountID, int NotificationID, DateTimeOffset CreatedAt)> incomingForRequester = await distributedCacheStore.GetPendingFriendRequestsForAccount(requesterID);

        using (Assert.Multiple())
        {
            await Assert.That(incomingForTarget.Count).IsEqualTo(1);
            await Assert.That(incomingForTarget.Single().RequesterAccountID).IsEqualTo(requesterID);
            await Assert.That(incomingForTarget.Single().NotificationID).IsEqualTo(targetNotificationID);
            await Assert.That(incomingForRequester.Count).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Logging_In_Re_Delivers_A_Pending_Friend_Request_As_An_Approvable_Notification()
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        (Account requesterAccount, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials("login_redelivery_requester@kongor.com", "Requester", "SecurePassword123!");
        (Account targetAccount, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials("login_redelivery_target@kongor.com", "Target", "SecurePassword123!");

        const int targetNotificationID = 920012;

        using (IServiceScope cacheScope = webApplicationFactory.Services.CreateScope())
        {
            IDatabase distributedCacheStore = cacheScope.ServiceProvider.GetRequiredService<IDatabase>();

            await distributedCacheStore.SetFriendRequest(requesterAccount.ID, targetAccount.ID, targetNotificationID);
        }

        HttpClient httpClient = webApplicationFactory.CreateClient();

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "S2 Games/Heroes Of Newerth/4.10.1.0/wac/x86_64");

        SrpParameters parameters = SrpParameters.Create<System.Security.Cryptography.SHA256>
            (SRPAuthenticationSessionDataStageOne.SafePrimeNumber, SRPAuthenticationSessionDataStageOne.MultiplicativeGroupGenerator);

        parameters.Generator = parameters.Pad(parameters.Generator);

        SrpClient client = new (parameters);
        SrpEphemeral clientEphemeral = client.GenerateEphemeral();

        Dictionary<string, string> preAuthenticationFormData = new ()
        {
            { "login", targetAccount.Name },
            { "A", clientEphemeral.Public },
            { "SysInfo", "MAC123|system|info|data|testhash" }
        };

        HttpResponseMessage preAuthenticationResponse = await httpClient.PostAsync("client_requester.php?f=pre_auth", new FormUrlEncodedContent(preAuthenticationFormData));

        IDictionary<object, object>? preAuthenticationData = PhpSerialization.Deserialize(await preAuthenticationResponse.Content.ReadAsStringAsync()) as IDictionary<object, object>
            ?? throw new NullReferenceException("Pre-Authentication Response Deserialised To NULL");

        string sessionSalt = preAuthenticationData["salt"] as string ?? throw new NullReferenceException("Session Salt Is NULL");
        string passwordSalt = preAuthenticationData["salt2"] as string ?? throw new NullReferenceException("Password Salt Is NULL");
        string serverPublicEphemeral = preAuthenticationData["B"] as string ?? throw new NullReferenceException("Server Public Ephemeral Is NULL");

        string passwordHash = SRPPasswordHasher.ComputeSRPPasswordHash("SecurePassword123!", passwordSalt);
        string privateClientKey = client.DerivePrivateKey(sessionSalt, targetAccount.Name, passwordHash);

        SrpSession clientSession = client.DeriveSession(clientEphemeral.Secret, serverPublicEphemeral, sessionSalt, targetAccount.Name, privateClientKey);

        Dictionary<string, string> authenticationFormData = new ()
        {
            { "login", targetAccount.Name },
            { "proof", clientSession.Proof },
            { "OSType", "windows" },
            { "MajorVersion", "4" },
            { "MinorVersion", "10" },
            { "MicroVersion", "1" },
            { "SysInfo", "testhash|testhash|testhash|testhash|testhash" }
        };

        HttpResponseMessage authenticationResponse = await httpClient.PostAsync("client_requester.php?f=srpAuth", new FormUrlEncodedContent(authenticationFormData));

        string authenticationResponseBody = await authenticationResponse.Content.ReadAsStringAsync();

        using (Assert.Multiple())
        {
            await Assert.That(authenticationResponse.IsSuccessStatusCode).IsTrue();

            // The Notification Carries The Requester's Name And The Type 23 Marker (NOTIFY_TYPE_BUDDY_REQUESTED_ADDED) In Its Pipe-Separated Data
            await Assert.That(authenticationResponseBody).Contains($"{requesterAccount.Name}||23||||");

            // The Timestamp Follows The Notification's Type And Placeholder Fields, In The 24-Hour "dd/MM  HH:mm" Format
            await Assert.That(Regex.IsMatch(authenticationResponseBody, @"\|\|23\|\|\|\|\d{2}/\d{2}  \d{2}:\d{2}")).IsTrue();

            // The Target's Notification ID Is Sent Separately So The Client Can Remove It Once Actioned
            await Assert.That(authenticationResponseBody).Contains(targetNotificationID.ToString());
        }
    }

    [Test]
    public async Task Removing_All_Notifications_Clears_Every_Pending_Friend_Request_And_Returns_Success()
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        (Account requesterAccountOne, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials("remove_all_requester_one@kongor.com", "RAReqOne", "SecurePassword123!");
        (Account requesterAccountTwo, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials("remove_all_requester_two@kongor.com", "RAReqTwo", "SecurePassword123!");
        SRPAuthenticationData targetAuthentication = await srpAuthenticationService.AuthenticateWithSRP("remove_all_target@kongor.com", "RAClearTgt", "SecurePassword123!");

        string cookie = targetAuthentication.Cookie ?? throw new InvalidOperationException("Cookie Is NULL");

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        IDatabase distributedCacheStore = scope.ServiceProvider.GetRequiredService<IDatabase>();

        await distributedCacheStore.SetFriendRequest(requesterAccountOne.ID, targetAuthentication.Account.ID, 810020);
        await distributedCacheStore.SetFriendRequest(requesterAccountTwo.ID, targetAuthentication.Account.ID, 810021);

        HttpClient httpClient = webApplicationFactory.CreateClient();

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "S2 Games/Heroes Of Newerth/4.10.1.0/wac/x86_64");

        Dictionary<string, string> formData = new ()
        {
            { "account_id", targetAuthentication.Account.ID.ToString() },
            { "cookie", cookie }
        };

        HttpResponseMessage response = await httpClient.PostAsync("client_requester.php?f=remove_all_notifications", new FormUrlEncodedContent(formData));

        IDictionary<object, object>? responseData = PhpSerialization.Deserialize(await response.Content.ReadAsStringAsync()) as IDictionary<object, object>;

        using (Assert.Multiple())
        {
            await Assert.That(response.IsSuccessStatusCode).IsTrue();
            await Assert.That(responseData?["status"] as string).IsEqualTo("OK");
            await Assert.That((await distributedCacheStore.GetPendingFriendRequestsForAccount(targetAuthentication.Account.ID)).Count).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Removing_All_Notifications_With_No_Pending_Notifications_Still_Returns_Success()
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        SRPAuthenticationData targetAuthentication = await srpAuthenticationService.AuthenticateWithSRP("remove_all_empty_target@kongor.com", "RAEmptyTgt", "SecurePassword123!");

        string cookie = targetAuthentication.Cookie ?? throw new InvalidOperationException("Cookie Is NULL");

        HttpClient httpClient = webApplicationFactory.CreateClient();

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "S2 Games/Heroes Of Newerth/4.10.1.0/wac/x86_64");

        Dictionary<string, string> formData = new ()
        {
            { "account_id", targetAuthentication.Account.ID.ToString() },
            { "cookie", cookie }
        };

        HttpResponseMessage response = await httpClient.PostAsync("client_requester.php?f=remove_all_notifications", new FormUrlEncodedContent(formData));

        IDictionary<object, object>? responseData = PhpSerialization.Deserialize(await response.Content.ReadAsStringAsync()) as IDictionary<object, object>;

        using (Assert.Multiple())
        {
            await Assert.That(response.IsSuccessStatusCode).IsTrue();
            await Assert.That(responseData?["status"] as string).IsEqualTo("OK");
        }
    }

    [Test]
    public async Task Removing_All_Notifications_Only_Clears_The_Requesting_Account()
    {
        SRPAuthenticationService srpAuthenticationService = new (webApplicationFactory);

        (Account requesterAccount, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials("remove_all_isolation_requester@kongor.com", "RAIsoReq", "SecurePassword123!");
        SRPAuthenticationData targetAuthenticationOne = await srpAuthenticationService.AuthenticateWithSRP("remove_all_isolation_target_one@kongor.com", "RAIsoTgtA", "SecurePassword123!");
        (Account targetAccountTwo, string _) = await srpAuthenticationService.CreateAccountWithSRPCredentials("remove_all_isolation_target_two@kongor.com", "RAIsoTgtB", "SecurePassword123!");

        string cookie = targetAuthenticationOne.Cookie ?? throw new InvalidOperationException("Cookie Is NULL");

        using IServiceScope scope = webApplicationFactory.Services.CreateScope();

        IDatabase distributedCacheStore = scope.ServiceProvider.GetRequiredService<IDatabase>();

        await distributedCacheStore.SetFriendRequest(requesterAccount.ID, targetAuthenticationOne.Account.ID, 830020);
        await distributedCacheStore.SetFriendRequest(requesterAccount.ID, targetAccountTwo.ID, 830021);

        HttpClient httpClient = webApplicationFactory.CreateClient();

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "S2 Games/Heroes Of Newerth/4.10.1.0/wac/x86_64");

        Dictionary<string, string> formData = new ()
        {
            { "account_id", targetAuthenticationOne.Account.ID.ToString() },
            { "cookie", cookie }
        };

        HttpResponseMessage response = await httpClient.PostAsync("client_requester.php?f=remove_all_notifications", new FormUrlEncodedContent(formData));

        IDictionary<object, object>? responseData = PhpSerialization.Deserialize(await response.Content.ReadAsStringAsync()) as IDictionary<object, object>;

        using (Assert.Multiple())
        {
            await Assert.That(response.IsSuccessStatusCode).IsTrue();
            await Assert.That(responseData?["status"] as string).IsEqualTo("OK");
            await Assert.That((await distributedCacheStore.GetPendingFriendRequestsForAccount(targetAuthenticationOne.Account.ID)).Count).IsEqualTo(0);
            await Assert.That((await distributedCacheStore.GetPendingFriendRequestsForAccount(targetAccountTwo.ID)).Count).IsEqualTo(1);
        }
    }
}
