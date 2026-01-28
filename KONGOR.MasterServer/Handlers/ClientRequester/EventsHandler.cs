using KONGOR.MasterServer.Services.Requester;
// For PhpSerialization

namespace KONGOR.MasterServer.Handlers.ClientRequester;

public partial class EventsHandler(ILogger<EventsHandler> logger) : IClientRequestHandler
{
    private ILogger Logger { get; } = logger;

    [LoggerMessage(Level = LogLevel.Information, Message = "[Events] Request for {FunctionName} (STUB)")]
    private partial void LogEventsRequest(string functionName);

    public Task<IActionResult> HandleRequestAsync(HttpContext context)
    {
        HttpRequest Request = context.Request;
        string? functionName = Request.Query["f"].FirstOrDefault() ?? Request.Form["f"].FirstOrDefault();

        LogEventsRequest(functionName ?? "NULL");

        Dictionary<string, object> response = new()
        {
            // Empty events info or messages
        };

        return Task.FromResult<IActionResult>(new OkObjectResult(PhpSerialization.Serialize(response)));
    }
}
