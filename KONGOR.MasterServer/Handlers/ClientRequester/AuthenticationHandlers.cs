using System.Text.RegularExpressions;
using KONGOR.MasterServer.Handlers.SRP;
using KONGOR.MasterServer.Infrastructure;
using KONGOR.MasterServer.Models.RequestResponse;
using KONGOR.MasterServer.Services.Requester;
using MERRICK.DatabaseContext;
using MERRICK.DatabaseContext.Entities.Core;
using global::MERRICK.DatabaseContext.Enumerations;
using MERRICK.DatabaseContext.Extensions;
using KONGOR.MasterServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace KONGOR.MasterServer.Handlers.ClientRequester;

public class PreAuthHandler(
    MerrickContext databaseContext,
    IDatabase distributedCache) : IClientRequestHandler
{
    public async Task<IActionResult> HandleRequestAsync(HttpContext context)
    {
        HttpRequest Request = context.Request;
        
        string? accountName = Request.Form["login"];

        if (accountName is null)
        {
            return new NotFoundObjectResult(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingLoginIdentifier)));
        }

        string? clientPublicEphemeral = Request.Form["A"];

        if (clientPublicEphemeral is null)
        {
            return new BadRequestObjectResult(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingClientPublicEphemeral)));
        }

        string? systemInformation = Request.Form["SysInfo"];

        if (systemInformation is null)
        {
            return new BadRequestObjectResult(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingSystemInformation)));
        }

        Account? account = await databaseContext.Accounts
            .Include(account => account.User)
            .Include(account => account.Clan)
            .FirstOrDefaultAsync(account => account.Name.ToLower() == accountName.ToLower());

        if (account is null)
        {
            return new NotFoundObjectResult(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.AccountNotFound)));
        }

        if (account.Type is AccountType.Disabled)
        {
            return new UnauthorizedObjectResult(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.AccountIsDisabled,
                    account.GetNameWithClanTag())));
        }

        User user = account.User;

        string authenticationSessionSalt = SRPAuthenticationSessionDataStageOne.GenerateSRPSessionSalt();

        string verifier =
            SRPAuthenticationSessionDataStageOne.ComputeVerifier(authenticationSessionSalt, accountName,
                user.SRPPasswordHash);

        (string serverPrivateEphemeral, string serverPublicEphemeral) =
            SRPAuthenticationSessionDataStageOne.ComputeServerEphemeral(verifier);

        SRPAuthenticationSessionDataStageOne data = new()
        {
            LoginIdentifier = accountName,
            SessionSalt = authenticationSessionSalt,
            PasswordSalt = user.SRPPasswordSalt,
            PasswordHash = user.SRPPasswordHash,
            ClientPublicEphemeral = clientPublicEphemeral,
            Verifier = verifier,
            ServerPrivateEphemeral = serverPrivateEphemeral,
            ServerPublicEphemeral = serverPublicEphemeral
        };

        await distributedCache.SetSRPAuthenticationSessionData(accountName, data);
        await distributedCache.SetSRPAuthenticationSystemInformation(accountName, systemInformation);

        return new OkObjectResult(PhpSerialization.Serialize(new SRPAuthenticationResponseStageOne(data)));
    }
}

public partial class SRPAuthHandler(
    MerrickContext databaseContext,
    IDatabase distributedCache,
    ILogger<SRPAuthHandler> logger) : IClientRequestHandler
{
    [GeneratedRegex(
        @"(?>S2 Games)\/(?>Heroes [oO]f Newerth)\/(?<version>\d{1,2}\.\d{1,2}\.\d{1,2}\.\d{1,2})\/(?<platform>[wlm]a[cs])\/(?<architecture>x86_64|x86-biarch|universal-64)")]
    private static partial Regex UserAgentRegex();

    public async Task<IActionResult> HandleRequestAsync(HttpContext context)
    {
        HttpRequest Request = context.Request;
        
        string? accountName = Request.Form["login"];

        if (accountName is null)
        {
            return new NotFoundObjectResult(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingLoginIdentifier)));
        }

        string? clientProof = Request.Form["proof"];

        if (clientProof is null)
        {
            return new BadRequestObjectResult(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingSRPClientProof)));
        }
        
        // ... (Skipping verbose validation for brevity, assuming existing logic) ...
        // Re-implementing full logic to be safe
        
        string? operatingSystemType = Request.Form["OSType"];
        if (operatingSystemType is null) return new BadRequestObjectResult(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingOperatingSystemType)));

        string? majorVersion = Request.Form["MajorVersion"];
        if (majorVersion is null) return new BadRequestObjectResult(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingMajorVersion)));

        string? minorVersion = Request.Form["MinorVersion"];
        if (minorVersion is null) return new BadRequestObjectResult(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingMinorVersion)));

        string? microVersion = Request.Form["MicroVersion"];
        if (microVersion is null) return new BadRequestObjectResult(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingMicroVersion)));

        string? systemInformationHashes = Request.Form["SysInfo"];
        if (systemInformationHashes is null) return new BadRequestObjectResult(PhpSerialization.Serialize(new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingSystemInformation)));

        SRPAuthenticationSessionDataStageOne? stageOneData =
            await distributedCache.GetSRPAuthenticationSessionData(accountName);

        if (stageOneData is null)
        {
            logger.LogError(
                $@"[BUG] Unable To Retrieve Cached SRP Authentication Session Data For Account Name ""{accountName}""");

            return new UnprocessableEntityObjectResult(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingCachedSRPData)));
        }

        await distributedCache.RemoveSRPAuthenticationSessionData(accountName);

        string? systemInformation = await distributedCache.GetSRPAuthenticationSystemInformation(accountName);

        if (systemInformation is null)
        {
            logger.LogError($@"[BUG] Unable To Retrieve Cached System Information For Account Name ""{accountName}""");

            return new UnprocessableEntityObjectResult(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingSystemInformation)));
        }

        await distributedCache.RemoveSRPAuthenticationSystemInformation(accountName);

        SRPAuthenticationSessionDataStageTwo stageTwoData = new(stageOneData, clientProof);

        string? serverProof = stageTwoData.ServerProof;

        if (serverProof is null)
        {
            return new UnauthorizedObjectResult(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.IncorrectPassword)));
        }

        Account? account = await databaseContext.Accounts
            .Include(account => account.User).ThenInclude(user => user.Accounts)
            .Include(account => account.Clan).ThenInclude(clan => clan!.Members)
            .Include(account => account.BannedPeers)
            .Include(account => account.FriendedPeers)
            .Include(account => account.IgnoredPeers)
            .FirstOrDefaultAsync(account => account.Name.ToLower() == accountName.ToLower());

        if (account is null)
        {
            return new NotFoundObjectResult(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.AccountNotFound)));
        }

        if (account.Type is AccountType.ServerHost)
        {
            return new UnauthorizedObjectResult(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.IsServerHostingAccount)));
        }

        if (context.Connection.RemoteIpAddress is null)
        {
            logger.LogError($@"[BUG] Remote IP Address For Account Name ""{accountName}"" Is NULL");

            return new UnprocessableEntityObjectResult(PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.MissingIPAddress)));
        }

        string remoteIPAddress = context.Connection.RemoteIpAddress.MapToIPv4().ToString();

        if (account.Type is not AccountType.Staff)
        {
            string agent = Request.Headers.UserAgent.FirstOrDefault() ?? string.Empty;

            if (UserAgentRegex().IsMatch(agent).Equals(false))
            {
                logger.LogError(
                    $@"Account ""{account.GetNameWithClanTag()}"" Has Made A Request To Log In Using Unexpected User Agent ""{agent}""");

                return new BadRequestObjectResult(PhpSerialization.Serialize(
                    new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.UnexpectedUserAgent)));
            }

            if (account.IPAddressCollection.Contains(remoteIPAddress).Equals(false))
            {
                account.IPAddressCollection.Add(remoteIPAddress);
            }
            
            // ... (System Info Logic) ...
            string[] systemInformationDataPoints =
                [.. systemInformation.Split('|', StringSplitOptions.RemoveEmptyEntries)];

            if (systemInformationDataPoints.Length is not 5)
            {
                logger.LogError(
                    $@"Account ""{account.GetNameWithClanTag()}"" Has Made A Request To Log In Using Unexpected System Information ""{systemInformation}""");

                return new BadRequestObjectResult(PhpSerialization.Serialize(
                    new SRPAuthenticationFailureResponse(
                        SRPAuthenticationFailureReason.IncorrectSystemInformationFormat)));
            }

            if (account.MACAddressCollection.Contains(systemInformationDataPoints.First()).Equals(false))
            {
                account.MACAddressCollection.Add(systemInformationDataPoints.First());
            }

            if (account.SystemInformationCollection.Contains(string.Join('|', systemInformationDataPoints.Skip(1)))
                .Equals(false))
            {
                account.SystemInformationCollection.Add(string.Join('|', systemInformationDataPoints.Skip(1)));
            }
            
            string? systemInformationHash = systemInformationHashes.Split('|', StringSplitOptions.RemoveEmptyEntries)
                            .Distinct().FirstOrDefault();
            
            if (systemInformationHash is null)
            {
                logger.LogError(
                    $@"Account ""{account.GetNameWithClanTag()}"" Has Made A Request To Log In Using Unexpected System Information Hashes ""{systemInformationHashes}""");

                return new BadRequestObjectResult(PhpSerialization.Serialize(
                    new SRPAuthenticationFailureResponse(
                        SRPAuthenticationFailureReason.IncorrectSystemInformationFormat)));
            }

            if (account.SystemInformationHashCollection.Contains(systemInformationHash).Equals(false))
            {
                account.SystemInformationHashCollection.Add(systemInformationHash);
            }

            await databaseContext.SaveChangesAsync();
        }

        // Host configs 
        string chatServerHost = Environment.GetEnvironmentVariable("CHAT_SERVER_HOST")
                                ?? throw new NullReferenceException("Chat Server Host Is NULL");

        int chatServerClientConnectionsPort = int.Parse(Environment.GetEnvironmentVariable("CHAT_SERVER_PORT_CLIENT")
                                                        ?? throw new NullReferenceException(
                                                            "Chat Server Client Connections Port Is NULL"));

        SRPAuthenticationHandlers.StageTwoResponseParameters parameters = new()
        {
            Account = account,
            ClanRoster = account.Clan?.Members ?? [],
            ServerProof = serverProof,
            ClientIPAddress = remoteIPAddress,
            ChatServer = (chatServerHost, chatServerClientConnectionsPort)
        };

        SRPAuthenticationResponseStageTwo response =
            SRPAuthenticationHandlers.GenerateStageTwoResponse(parameters, out string cookie);

        account.TimestampLastActive = DateTimeOffset.UtcNow;
        account.Cookie = cookie;

        await databaseContext.SaveChangesAsync();
        logger.LogInformation("Persisted Cookie {Cookie} for Account {AccountName} to Database", cookie, accountName);

        await distributedCache.SetAccountNameForSessionCookie(cookie, accountName);

        return new OkObjectResult(PhpSerialization.Serialize(response));
    }
}

public class AuthHandler(ILogger<AuthHandler> logger) : IClientRequestHandler
{
    public Task<IActionResult> HandleRequestAsync(HttpContext context)
    {
        HttpRequest Request = context.Request;
        string? accountName = Request.Form["login"];

        if (accountName is not null)
        {
            logger.LogWarning(@"Account ""{AccountName}"" Is Attempting To Use HTTP Client Authentication",
                accountName);
        }

        string response =
            PhpSerialization.Serialize(
                new SRPAuthenticationFailureResponse(SRPAuthenticationFailureReason.SRPAuthenticationDisabled));

        return Task.FromResult<IActionResult>(new BadRequestObjectResult(response));
    }
}

public class Aids2CookieHandler(MerrickContext databaseContext, IDatabase distributedCache, ILogger<Aids2CookieHandler> logger) : IClientRequestHandler
{
    public async Task<IActionResult> HandleRequestAsync(HttpContext context)
    {
        HttpRequest Request = context.Request;
        string? cookie = Request.Form["cookie"];

        if (cookie is not null)
        {
            cookie = cookie.Replace("-", string.Empty);
        }

        if (cookie is null)
        {
            logger.LogError("Missing Cookie In aids2cookie Request");
            return new BadRequestObjectResult(PhpSerialization.Serialize(new { error = "Missing Cookie" }));
        }

        string? accountName = context.Items["SessionAccountName"] as string
                              ?? await distributedCache.GetAccountNameForSessionCookie(cookie);

        if (accountName is null)
        {
            var accountData = await databaseContext.Accounts
                .Where(account => account.Cookie == cookie)
                .Select(account => new { account.Name, account.ID })
                .FirstOrDefaultAsync();

            if (accountData is not null)
            {
                await distributedCache.SetAccountNameForSessionCookie(cookie, accountData.Name);
                return new OkObjectResult(PhpSerialization.Serialize(accountData.ID));
            }

            logger.LogError(
                $@"Cookie ""{cookie}"" Validated In Controller But Account Name Not Found In Cache (Context Missing, Redis Check Failed)");

            return new UnauthorizedObjectResult(PhpSerialization.Serialize(new { error = "Invalid Session" }));
        }

        int? accountId = await databaseContext.Accounts
            .Where(account => account.Name.ToLower() == accountName.ToLower())
            .Select(account => (int?) account.ID)
            .FirstOrDefaultAsync();

        if (accountId is null)
        {
            logger.LogError($@"Account Name ""{accountName}"" From Cookie Not Found In Database");
            return new NotFoundObjectResult(PhpSerialization.Serialize(new { error = "Account Not Found" }));
        }

        return new OkObjectResult(PhpSerialization.Serialize(accountId.Value));
    }
}

public class LogoutHandler(IDatabase distributedCache) : IClientRequestHandler
{
    public async Task<IActionResult> HandleRequestAsync(HttpContext context)
    {
        string? cookie = context.Request.Form["cookie"];
        if (cookie is not null)
        {
            await distributedCache.RemoveAccountNameForSessionCookie(cookie);
        }

        return new OkObjectResult(PhpSerialization.Serialize(true));
    }
}
