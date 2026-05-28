namespace DAWNBRINGER.WebPortal.UI.Contracts;

public record HostAccountAuthorisationTokenResponse(Guid Token, DateTimeOffset ExpiresAt);
