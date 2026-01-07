using System.Text;

namespace KONGOR.MasterServer.Infrastructure;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    private RequestDelegate Next { get; } = next;
    private ILogger Logger { get; } = logger;

    public async Task Invoke(HttpContext context)
    {
        context.Request.EnableBuffering();

        string body = string.Empty;

        if (context.Request.ContentLength > 0 && (context.Request.ContentType?.Contains("form") == true || context.Request.ContentType?.Contains("json") == true))
        {
            using StreamReader reader = new(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        string queryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty;
        
        // Log the request
        Logger.LogInformation("Incoming Request: {Method} {Path}{QueryString} Body: {Body}", 
            context.Request.Method, 
            context.Request.Path, 
            queryString, 
            string.IsNullOrWhiteSpace(body) ? "[Empty/Binary]" : body);

        await Next(context);
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
