namespace ZORGATH.WebPortal.API.Services;

public enum ServiceErrorType
{
    None,
    BadRequest,
    NotFound,
    Conflict,
    Unauthorized,
    UnprocessableEntity,
    InternalError
}

public class ServiceResult<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public ServiceErrorType ErrorType { get; init; }
    public IEnumerable<string>? ValidationErrors { get; init; }

    public static ServiceResult<T> Success(T data) => new() { IsSuccess = true, Data = data };
    public static ServiceResult<T> Failure(string message, ServiceErrorType type) => new() { IsSuccess = false, ErrorMessage = message, ErrorType = type };
    public static ServiceResult<T> ValidationFailure(IEnumerable<string> errors) => new() { IsSuccess = false, ValidationErrors = errors, ErrorType = ServiceErrorType.BadRequest, ErrorMessage = "Validation Failed" };
}
