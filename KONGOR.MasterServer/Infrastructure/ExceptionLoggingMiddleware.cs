namespace KONGOR.MasterServer.Infrastructure;

public class ExceptionLoggingMiddleware(RequestDelegate next, ILogger<ExceptionLoggingMiddleware> logger)
{
    private RequestDelegate Next { get; } = next;
    private ILogger Logger { get; } = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await Next(context);
        }
        catch (Exception exception)
        {
            // Log the exception message explicitly so it appears in the Structured Logs "Message" column
            // structure: [Error] {ExceptionMessage}
            Logger.LogError(exception, "{ExceptionMessage}", exception.Message);
            throw;
        }
    }
}