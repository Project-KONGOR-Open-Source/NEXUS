
namespace ASPIRE.Common.DTOs;

public record GetBasicUserDTO(int ID, string EmailAddress, List<GetBasicAccountDTO> Accounts);
