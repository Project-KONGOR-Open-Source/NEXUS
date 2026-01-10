namespace ZORGATH.WebPortal.API.Contracts;

public record GetBasicUserDTO(int ID, string EmailAddress, List<GetBasicAccountDTO> Accounts);