using ASPIRE.Common.DTOs;

using MERRICK.DatabaseContext.Entities.Core;

namespace ZORGATH.WebPortal.API.Services;

public interface IUserService
{
    Task<ServiceResult<User>> RegisterUserAsync(RegisterUserAndMainAccountDTO payload);
}
