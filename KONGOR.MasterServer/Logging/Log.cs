using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

namespace KONGOR.MasterServer.Logging;

public static partial class Log
{
    // Authentication Handlers
    [GeneratedRegex(@"(?>S2 Games)\/(?>Heroes [oO]f Newerth)\/(?<version>\d{1,2}\.\d{1,2}\.\d{1,2}\.\d{1,2})\/(?<platform>[wlm]a[cs])\/(?<architecture>x86_64|x86-biarch|universal-64)")]
    public static partial Regex UserAgentRegex();

    [LoggerMessage(Level = LogLevel.Error, Message = "[BUG] Unable To Retrieve Cached SRP Authentication Session Data For Account Name \"{AccountName}\"")]
    public static partial void LogMissingSRPDataBug(this ILogger logger, string accountName);

    [LoggerMessage(Level = LogLevel.Error, Message = "[BUG] Unable To Retrieve Cached System Information For Account Name \"{AccountName}\"")]
    public static partial void LogMissingSysInfoBug(this ILogger logger, string accountName);

    [LoggerMessage(Level = LogLevel.Error, Message = "[BUG] Remote IP Address For Account Name \"{AccountName}\" Is NULL")]
    public static partial void LogRemoteIPNullBug(this ILogger logger, string accountName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Account \"{AccountName}\" Has Made A Request To Log In Using Unexpected User Agent \"{Agent}\"")]
    public static partial void LogUnexpectedUserAgent(this ILogger logger, string accountName, string agent);

    [LoggerMessage(Level = LogLevel.Error, Message = "Account \"{AccountName}\" Has Made A Request To Log In Using Unexpected System Information \"{SysInfo}\"")]
    public static partial void LogUnexpectedSysInfo(this ILogger logger, string accountName, string sysInfo);

    [LoggerMessage(Level = LogLevel.Error, Message = "Account \"{AccountName}\" Has Made A Request To Log In Using Unexpected System Information Hashes \"{Hashes}\"")]
    public static partial void LogUnexpectedSysInfoHashes(this ILogger logger, string accountName, string hashes);

    [LoggerMessage(Level = LogLevel.Information, Message = "Persisted Cookie {Cookie} for Account {AccountName} to Database")]
    public static partial void LogPersistedCookie(this ILogger logger, string cookie, string accountName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Account \"{AccountName}\" Is Attempting To Use HTTP Client Authentication")]
    public static partial void LogAttemptingHttpClientAuth(this ILogger logger, string accountName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Missing Cookie In aids2cookie Request")]
    public static partial void LogMissingCookie(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "Cookie \"{Cookie}\" Validated In Controller But Account Name Not Found In Cache (Context Missing, Redis Check Failed)")]
    public static partial void LogCookieValidatedButNotFound(this ILogger logger, string cookie);

    [LoggerMessage(Level = LogLevel.Error, Message = "Account Name \"{AccountName}\" From Cookie Not Found In Database")]
    public static partial void LogAccountNameNotFoundInDB(this ILogger logger, string accountName);

    // ClientRequesterController
    [LoggerMessage(Level = LogLevel.Information, Message = "[ClientRequester] Processing '{FunctionName}'. Raw Cookie: '{CookieRaw}' (RequiresValidation: {RequiresValidation})")]
    public static partial void LogProcessingRequest(this ILogger logger, string functionName, string cookieRaw, bool requiresValidation);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[ClientRequester] Redis Miss for '{FunctionName}' with cookie '{CookieRaw}'.")]
    public static partial void LogRedisMiss(this ILogger logger, string functionName, string cookieRaw);

    [LoggerMessage(Level = LogLevel.Information, Message = "[ClientRequester] Attempting DB Fallback for cookie '{Cookie}'...")]
    public static partial void LogAttemptingDBFallback(this ILogger logger, string cookie);

    [LoggerMessage(Level = LogLevel.Information, Message = "[ClientRequester] DB Hit! Restored Session From Database For Cookie '{Cookie}' (User: {AccountName})")]
    public static partial void LogDBHit(this ILogger logger, string cookie, string accountName);

    [LoggerMessage(Level = LogLevel.Error, Message = "[ClientRequester] DB Miss! Cookie '{Cookie}' not found in Accounts table.")]
    public static partial void LogDBMiss(this ILogger logger, string cookie);

    [LoggerMessage(Level = LogLevel.Warning, Message = "IP Address \"{IPAddress}\" Has Made A Client Request With Forged Cookie \"{CookieRaw}\"")]
    public static partial void LogForgedCookie(this ILogger logger, string ipAddress, string cookieRaw);

    // IconsController
    [LoggerMessage(Level = LogLevel.Debug, Message = "Icon Not Found: {Path}")]
    public static partial void LogIconNotFound(this ILogger logger, string path);

    // PatcherController
    [LoggerMessage(Level = LogLevel.Error, Message = "Latest Patch Details Not Found For Distribution \"{OperatingSystem}\"")]
    public static partial void LogLatestPatchNotFound(this ILogger logger, string operatingSystem);

    // ReplayController
    [LoggerMessage(Level = LogLevel.Information, Message = "Saved Replay (Form): {Path}")]
    public static partial void LogSavedReplayForm(this ILogger logger, string path);

    [LoggerMessage(Level = LogLevel.Information, Message = "Saved Replay (Raw): {Path}")]
    public static partial void LogSavedReplayRaw(this ILogger logger, string path);

    [LoggerMessage(Level = LogLevel.Error, Message = "Replay Upload Failed")]
    public static partial void LogUploadFailed(this ILogger logger, Exception ex);

    // QuestController
    [LoggerMessage(Level = LogLevel.Information, Message = "[QuestController] GetCurrentQuests called. Cookie present: {CookiePresent}")]
    public static partial void LogGetCurrentQuests(this ILogger logger, bool cookiePresent);

    [LoggerMessage(Level = LogLevel.Information, Message = "[QuestController] Cookie validation result: IsValid={IsValid}, Account={AccountName}")]
    public static partial void LogCookieValidationResult(this ILogger logger, bool isValid, string? accountName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[QuestController] Unauthorized access attempt. Invalid cookie: {Cookie}. Proceeding with dummy response to prevent client logout.")]
    public static partial void LogUnauthorizedAccess(this ILogger logger, string cookie);

    [LoggerMessage(Level = LogLevel.Information, Message = "[QuestController] Returning success response: {SerializedResponse}")]
    public static partial void LogSuccessResponse(this ILogger logger, string serializedResponse);

    [LoggerMessage(Level = LogLevel.Error, Message = "[QuestController] Unhandled exception in GetCurrentQuests")]
    public static partial void LogGetCurrentQuestsError(this ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[QuestController] Unauthorized access attempt (PlayerQuests). Invalid cookie: {Cookie}. Proceeding with dummy response.")]
    public static partial void LogUnauthorizedPlayerQuests(this ILogger logger, string cookie);

    [LoggerMessage(Level = LogLevel.Error, Message = "[QuestController] Unhandled exception in GetPlayerQuests")]
    public static partial void LogGetPlayerQuestsError(this ILogger logger, Exception ex);

    // ServerRequesterController - Authentication
    [LoggerMessage(Level = LogLevel.Error, Message = "[BUG] Remote IP Address For Server Manager With Host Account Name \"{HostAccountName}\" Is NULL")]
    public static partial void LogServerManagerIPNullBug(this ILogger logger, string hostAccountName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Server Manager ID \"{MatchServerManagerID}\" Was Registered At \"{MatchServerManagerIPAddress}\" With Cookie \"{MatchServerManagerCookie}\"")]
    public static partial void LogServerManagerRegistered(this ILogger logger, int matchServerManagerID, string matchServerManagerIPAddress, string matchServerManagerCookie);

    [LoggerMessage(Level = LogLevel.Information, Message = "Server ID \"{MatchServerID}\" Was Registered At \"{MatchServerIPAddress}\":\"{MatchServerPort}\" With Cookie \"{MatchServerCookie}\"")]
    public static partial void LogServerRegistered(this ILogger logger, int matchServerID, string matchServerIPAddress, int matchServerPort, string matchServerCookie);

    [LoggerMessage(Level = LogLevel.Error, Message = "[BUG] No Account Could Be Found For Account Name \"{AccountName}\" With Session Cookie \"{Cookie}\"")]
    public static partial void LogAccountNotFoundForCookieBug(this ILogger logger, string accountName, string cookie);

    [LoggerMessage(Level = LogLevel.Debug, Message = "{Message}")]
    public static partial void LogSetOnlineDebug(this ILogger logger, string message);

    [LoggerMessage(Level = LogLevel.Error, Message = "[BUG] Received Match Initialisation Heartbeat For Session \"{Session}\", But No MatchStartData Found In Cache. Server Status: {ServerStatus}")]
    public static partial void LogMatchInitHeartbeatBug(this ILogger logger, string session, ServerStatus serverStatus);

    [LoggerMessage(Level = LogLevel.Error, Message = "[BUG] Received Match Initialisation Heartbeat For Match ID \"{MatchID}\", But No MatchStartData Found In Cache")]
    public static partial void LogMatchInitHeartbeatMatchIdBug(this ILogger logger, string matchID);

    [LoggerMessage(Level = LogLevel.Information, Message = "Captured Match Mode And Options For Match ID {MatchID}: MatchMode={MatchMode}, ArrangedMatchType={ArrangedMatchType}")]
    public static partial void LogCapturedMatchMode(this ILogger logger, string matchID, string matchMode, int arrangedMatchType);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Account \"{AccountName}\" Is Attempting To Use HTTP Server Authentication")]
    public static partial void LogHttpServerAuthAttempt(this ILogger logger, string accountName);

    // ServerRequesterController - Match
    [LoggerMessage(Level = LogLevel.Information, Message = "Match ID {MatchID} Has Started - Host Name: {HostName}, Server ID: {ServerID}, Map: {Map}")]
    public static partial void LogMatchStarted(this ILogger logger, int matchID, string hostName, int serverID, string map);

    // ServerRequesterController - Replays
    [LoggerMessage(Level = LogLevel.Error, Message = "Missing Value For Form Parameter \"session\" In HandleSetReplaySize")]
    public static partial void LogMissingSessionInSetReplaySize(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "No Match Server Found For Session Cookie \"{Session}\" In HandleSetReplaySize")]
    public static partial void LogMatchServerNotFoundForSession(this ILogger logger, string session);

    [LoggerMessage(Level = LogLevel.Error, Message = "Missing Value For Form Parameter \"match_id\"")]
    public static partial void LogMissingMatchId(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "Missing Value For Form Parameter \"file_size\"")]
    public static partial void LogMissingFileSize(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Received Replay Size For Match ID {MatchID}: {FileSize} Bytes")]
    public static partial void LogReceivedReplaySize(this ILogger logger, int matchID, int fileSize);

    [LoggerMessage(Level = LogLevel.Error, Message = "Invalid Match ID Or File Size Format: ID={MatchID}, Size={FileSize}")]
    public static partial void LogInvalidMatchIdOrFileSize(this ILogger logger, string matchID, string fileSize);

    // DistributedCacheExtensions
    [LoggerMessage(Level = LogLevel.Information, Message = "[SessionFallback] DB Hit! Restored Session From Database For Cookie '{Cookie}' (User: {AccountName})")]
    public static partial void LogSessionFallbackRestored(this ILogger logger, string cookie, string accountName);
}
