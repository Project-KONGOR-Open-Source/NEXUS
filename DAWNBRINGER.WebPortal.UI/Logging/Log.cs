using Microsoft.Extensions.Logging;

namespace DAWNBRINGER.WebPortal.UI.Logging;

public static partial class Log
{
    [LoggerMessage(Level = LogLevel.Error, Message = "API Registration Failed: {StatusCode} {Error}")]
    public static partial void LogApiRegistrationFailed(this ILogger logger, int statusCode, string error);

    [LoggerMessage(Level = LogLevel.Error, Message = "Exception during registration")]
    public static partial void LogRegistrationException(this ILogger logger, Exception ex);
}
