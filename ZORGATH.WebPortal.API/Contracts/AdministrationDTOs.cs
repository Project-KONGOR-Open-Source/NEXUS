namespace ZORGATH.WebPortal.API.Contracts;

public record HostAccountAuthorisationTokenDTO(Guid Token, DateTimeOffset ExpiresAt);
