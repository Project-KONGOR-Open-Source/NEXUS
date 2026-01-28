using KONGOR.MasterServer.Services.Requester;
// For PhpSerialization

namespace KONGOR.MasterServer.Handlers.ClientRequester;

public partial class RewardsHandler(ILogger<RewardsHandler> logger) : IClientRequestHandler
{
    private ILogger Logger { get; } = logger;

    [LoggerMessage(Level = LogLevel.Information, Message = "[Rewards] Claiming season rewards (STUB).")]
    private partial void LogClaimingRewards();

    public Task<IActionResult> HandleRequestAsync(HttpContext context)
    {
        LogClaimingRewards();

        Dictionary<string, object> response = new() { ["vested_threshold"] = "5", ["0"] = true };

        return Task.FromResult<IActionResult>(new OkObjectResult(PhpSerialization.Serialize(response)));
    }
}