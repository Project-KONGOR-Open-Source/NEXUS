using ASPIRE.Common.DTOs;

namespace ZORGATH.WebPortal.API.Services;

public interface IAuthenticationService
{
    Task<ServiceResult<GetAuthenticationTokenDTO>> AuthenticateUserAsync(LogInUserDTO payload);
}
