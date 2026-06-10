namespace KONGOR.MasterServer.Filters;

/// <summary>
///     Emits a self-diagnosing diagnostic when a request form cannot be read during model binding.
///     A form size limit (value count, key length, or value length) surfaces as an <see cref="InvalidDataException"/> and the request body size limit as a <see cref="BadHttpRequestException"/>.
///     ASP.NET Core wraps either in a model-state error, and the <c>[ApiController]</c> convention then returns a 400 response without the action ever running, which leaves no exception to observe.
///     This filter inspects the model state before that 400 is produced, recording the failure and the body metadata, so that occurrences are diagnosable from the logs alone.
/// </summary>
public sealed class FormLimitDiagnosticsFilter(ILogger<FormLimitDiagnosticsFilter> logger) : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        ModelError? formReadError = context.ModelState.Values
            .SelectMany(entry => entry.Errors)
            .SingleOrDefault(error => error.Exception is InvalidDataException or BadHttpRequestException || error.Exception?.InnerException is InvalidDataException or BadHttpRequestException);

        if (formReadError is null)
            return;

        HttpRequest request = context.HttpContext.Request;

        string remoteIPAddress = request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "UNKNOWN";
        string remoteEndpoint = $"{remoteIPAddress}:{request.HttpContext.Connection.RemotePort}";
        string formReadMessage = (formReadError.Exception?.InnerException ?? formReadError.Exception)?.Message ?? "UNKNOWN";

        // The Form Could Not Be Read, So Only The Request Metadata Is Available For Diagnosis; The Exception Message States The Actual Cause
        logger.LogError(@"Request Form Could Not Be Read: ""{FormReadError}""" + Environment.NewLine + @"RemoteEndpoint: ""{RemoteEndpoint}"", ContentLength: {ContentLength}, ContentType: ""{ContentType}"", UserAgent: ""{UserAgent}""",
            formReadMessage, remoteEndpoint, request.ContentLength, request.ContentType, request.Headers.UserAgent.ToString());
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
