using ASPIRE.ServiceDefaults.Extensions.Cryptography; // TODO: Remove Non-Aspire References To This Project From All Projects

namespace KONGOR.MasterServer.Controllers.ServerRequesterController;

public partial class ServerRequesterController
{
    private async Task<IActionResult> HandleReplayAuthentication()
    {
        string? accountName = Request.Form["login"];

        if (accountName is null)
            return BadRequest(@"Missing Value For Form Parameter ""login""");

        accountName = accountName.TrimEnd(':'); // The Semicolon Is Used To Separate The Account Name From The Server Instance, So We Need To Remove It Because It Is Not Needed For The Server Manager

        string? accountPasswordHash = Request.Form["pass"];

        if (accountPasswordHash is null)
            return BadRequest(@"Missing Value For Form Parameter ""pass""");

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

        MatchServerManager manager = new()
        {
            HostID = account.ID,
            ID = accountName.GetDeterministicInt32Hash(),
            IPAddress = Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "0.0.0.0"
        };

        // TODO: Create Extension Methods For Distributed Cache

        byte[] serializedManager = JsonSerializer.SerializeToUtf8Bytes(manager);
        // await DistributedCache.SetAsync($@"SERVER-MANAGER:[""{accountName}""]", serializedManager); // TODO: Fix Distributed Cache

        // ChatServerConfiguration? chatServerConfig = Configuration.GetSection("ChatServerConfiguration").Get<ChatServerConfiguration>();
        // if (KongorContext.RuntimeEnvironment is "Development") chatServerConfig.Address = AddressHelpers.ResolveChatServerAddress(Request.HttpContext.Connection.RemoteIpAddress);
        // TODO: Resolve Chat Server Address/Port
        string chatAddress = "127.0.0.1";
        int chatPort = 55551;

        Dictionary<string, object> response = new()
        {
            ["server_id"] = manager.ID,
            ["official"] = 1, // If Not Official, It Is Considered To Be Un-Authorized
            ["session"] = manager.Cookie,
            ["chat_address"] = chatAddress,
            ["chat_port"] = chatPort,
        };

        // TODO: Investigate How These Are Used (+ Resolve CDN Host)
        response["cdn_upload_host"] = "kongor.online";
        response["cdn_upload_target"] = "upload";

        Logger.LogInformation($@"Server Manager ID ""{manager.ID}"" Was Registered At ""{manager.IPAddress}"" With Cookie ""{manager.Cookie}""");

        return Ok(PhpSerialization.Serialize(response));
    }

    private async Task<IActionResult> HandleNewSession()
    {
        string serverIdentifier = Request.Form["login"].ToString();

        if (serverIdentifier.Split(':').Length is not 2)
            return BadRequest(@"Missing Or Incorrect Value For Form Parameter ""login""");

        string accountName = serverIdentifier.Split(':').First();
        string serverInstance = serverIdentifier.Split(':').Last();

        string? accountPasswordHash = Request.Form["pass"];

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

        MatchServer server = new()
        {
            HostID = account.ID,
            ID = serverIdentifier.GetDeterministicInt32Hash(),
            Name = serverName,
            Instance = int.Parse(serverInstance),
            IPAddress = serverIPAddress,
            Port = int.Parse(serverPort),
            Location = serverLocation,
            Description = serverDescription
        };

        // TODO: Create Extension Methods For Distributed Cache

        byte[] serializedServer = JsonSerializer.SerializeToUtf8Bytes(server);
        // await DistributedCache.SetAsync($@"SERVER:[""{serverIdentifier}""]", serializedServer); // TODO: Fix Distributed Cache

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

        Logger.LogInformation($@"Server ID ""{server.ID}"" Was Registered At ""{server.IPAddress}:{server.Port}"" With Cookie ""{server.Cookie}""");

        return Ok(PhpSerialization.Serialize(response));
    }

    private async Task<IActionResult> HandleSetOnline()
    {
        // It Is Unclear What This Does; Requests To This Endpoint Are Made Even While The Game Server Is Idle
        // TODO: Maybe Use This To Link The Server To The Server Manager? (Or Maybe Just Do That On Server New Session)

        return Ok();
    }
}
