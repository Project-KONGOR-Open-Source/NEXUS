namespace MERRICK.DatabaseContext.Constants;

/// <summary>
///     <para>
///         Used for setting and getting claims for JwtSecurityToken.Claims or for User.Claims (inside a controller action).
///         Compared to JwtSecurityToken.Claims, some of these claims will be different when getting them from User.Claims.
///     </para>
///     <para>
///         "email" in JwtSecurityToken.Claims is "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress" in User.Claims.
///         <br/>
///         "sub" in JwtSecurityToken.Claims is "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" in User.Claims.
///     </para>
/// </summary>
public static class Claims
{
    public const string AccountID = "account_id";
    public const string AccountIsMain = "account_is_main";
    public const string Audience = "aud";
    public const string AuthenticatedAtTime = "auth_time";
    public const string ClanName = "clan_name";
    public const string ClanTag = "clan_tag";
    public const string Email = "email";
    public const string EmailAddress = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
    public const string ExpiresAtTime = "exp";
    public const string IssuedAtTime = "iat";
    public const string Issuer = "iss";
    public const string JWTIdentifier = "jti";
    public const string NameIdentifier = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
    public const string Nonce = "nonce";
    public const string Subject = "sub";
    public const string UserID = "user_id";
    public const string UserRole = "user_role";
}
