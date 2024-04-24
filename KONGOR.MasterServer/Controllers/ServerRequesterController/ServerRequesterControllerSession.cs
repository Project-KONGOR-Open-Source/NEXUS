using ASPIRE.ServiceDefaults.Extensions.Cryptography; // TODO: Remove Non-Aspire References To This Project From All Projects

namespace KONGOR.MasterServer.Controllers.ServerRequesterController;

public partial class ServerRequesterController
{
    private async Task<IActionResult> HandleNewSession()
    {
        string serverIdentifier = Request.Form["login"].ToString();

        if (serverIdentifier.Split(':').Length is not 2)
            return BadRequest(@"Missing Or Incorrect Value For Form Parameter ""login""");

        string accountName = serverIdentifier.Split(':').First();
        string serverInstance = serverIdentifier.Split(':').Last();

        string? accountPasswordHash = Request.Form["pass"];

        // TODO: This Is Not Needed, Since We Can Check Auth Anyway (The Host Can Use Anything As The Password And It Will Still Work)
        if (accountPasswordHash is null) 
            return BadRequest(@"Missing Value For Form Parameter ""pass""");

        string? serverPort = Request.Form["port"];

        if (serverPort is null)
            return BadRequest(@"Missing Value For Form Parameter ""port""");

        string? serverName = Request.Form["name"];

        if (serverName is null)
            return BadRequest(@"Missing Value For Form Parameter ""name""");

        string? serverDescription = Request.Form["desc"];

        if (serverDescription is null)
            serverDescription = string.Empty;
            // TODO: Make COMPEL Send The Server Description
            // return BadRequest(@"Missing Value For Form Parameter ""desc""");

            string? serverLocation = Request.Form["location"];

        if (serverLocation is null)
            return BadRequest(@"Missing Value For Form Parameter ""location""");

        string? serverIPAddress = Request.Form["ip"];

        if (serverIPAddress is null)
            return BadRequest(@"Missing Value For Form Parameter ""ip""");

        Account? account = await MerrickContext.Accounts
            .Include(account => account.User)
            .SingleOrDefaultAsync(account => account.Name.Equals(accountName));

        if (account is null)
            return NotFound($@"Account ""{accountName}"" Was Not Found");

        if (account.Type is not AccountType.ServerHost)
            return Unauthorized($@"Account ""{accountName}"" Is Not A Server Host");

        string srpPasswordHash = SRPAuthenticationHandlers.ComputeSRPPasswordHash(accountPasswordHash, account.User.SRPPasswordSalt);
        
        if (srpPasswordHash.Equals(account.User.SRPPasswordHash) is false)
            return Unauthorized("Incorrect Password");

        // TODO: Verify Whether The Server Version Matches The Client Version (Or Disallow Servers To Be Started If They Are Not On The Latest Version)

        Server server = new()
        {
            Host = account,
            ID = serverIdentifier.GetDeterministicHashCode(),
            Name = serverName,
            Instance = int.Parse(serverInstance),
            IPAddress = serverIPAddress,
            Port = int.Parse(serverPort),
            Location = serverLocation,
            Description = serverDescription
        };

        // TODO: Create Extension Methods For Distributed Cache

        byte[] serializedServer = JsonSerializer.SerializeToUtf8Bytes(server);
        await DistributedCache.SetAsync($@"SERVER:[""{serverIdentifier}""]", serializedServer);

        // TODO: Implement Verifier In Description (If The Server Is A COMPEL Server, It Will Have A Verifier In The Description)

        // ChatServerConfiguration? chatServerConfig = Configuration.GetSection("ChatServerConfiguration").Get<ChatServerConfiguration>();
        // if (KongorContext.RuntimeEnvironment is "Development") chatServerConfig.Address = AddressHelpers.ResolveChatServerAddress(Request.HttpContext.Connection.RemoteIpAddress);
        // TODO: Resolve Chat Server Address/Port
        string chatAddress = "127.0.0.1";
        int chatPort = 55551;

        Dictionary<string, object> response = new()
        {
            ["session"] = server.Cookie,
            ["server_id"] = server.ID,
            ["chat_address"] = chatAddress,
            ["chat_port"] = chatPort,
            ["leaverthreshold"] = 0.05
        };

        Logger.LogInformation($@"New Server ""{server.IPAddress}:{server.Port}"" Was Created With Cookie ""{server.Cookie}""");

        return Ok(PhpSerialization.Serialize(response));
    }
}
