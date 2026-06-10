namespace KONGOR.MasterServer.Filters;

/// <summary>
///     Emits a self-diagnosing diagnostic when a request form cannot be read because a configured limit was exceeded.
///     When the form body exceeds a value-count, key-length, or value-length limit, or the request-body-size limit, the form reader throws while the body is read during model binding.
///     The framework wraps that failure in a model-state error, and the <c>[ApiController]</c> convention then returns a 400 response without the action ever running, which leaves no exception to observe.
///     This filter inspects the model state before that 400 is produced, capturing the failure together with the body metadata needed to size it, so the next occurrence is diagnosable from the logs alone.
/// </summary>
public sealed class FormLimitDiagnosticsFilter(ILogger<FormLimitDiagnosticsFilter> logger) : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // The Form Reader Surfaces A Value-Count, Key-Length, Or Value-Length Limit As InvalidDataException, And The Request-Body-Size Limit As BadHttpRequestException (Which Derives From IOException); Both Are Wrapped Before Reaching The Model State, So The Inner Exception Is Inspected As Well
        ModelError? formReadError = context.ModelState.Values
            .SelectMany(entry => entry.Errors)
            .SingleOrDefault(error => error.Exception is InvalidDataException or IOException
                                   || error.Exception?.InnerException is InvalidDataException or IOException);

        if (formReadError is null)
            return;

        HttpRequest request = context.HttpContext.Request;

        string remoteIPAddress = request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN";
        string formReadMessage = (formReadError.Exception?.InnerException ?? formReadError.Exception)?.Message ?? "Unknown";

        // The Form Could Not Be Read, So Only The Query String And Transport-Level Metadata Are Available For Diagnosis
        string requestContext = JsonSerializer.Serialize(new
        {
            Function = request.Query["f"].SingleOrDefault() ?? "NULL",
            Query = request.Query.ToDictionary(entry => entry.Key, entry => entry.Value.ToString()),
            RemoteEndpoint = $"{remoteIPAddress}:{request.HttpContext.Connection.RemotePort}",
            UserAgent = request.Headers.UserAgent.ToString(),
            ContentLength = request.ContentLength,
            ContentType = request.ContentType
        });

        logger.LogError(@"Request Form Could Not Be Read Because A Configured Limit Was Exceeded: ""{FormReadError}""" + Environment.NewLine + @"Request Context: {RequestContext}",
            formReadMessage, requestContext);
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
