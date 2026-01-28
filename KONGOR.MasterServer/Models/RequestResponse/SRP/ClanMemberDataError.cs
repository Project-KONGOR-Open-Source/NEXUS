namespace KONGOR.MasterServer.Models.RequestResponse.SRP;

public class ClanMemberDataError
{
    /// <summary>
    ///     This replaces the account's clan member data when the account is not part of a clan.
    /// </summary>
    [PHPProperty("error")]
    public string Error { get; set; } = "No Clan Member Found";
}
