namespace KONGOR.MasterServer.Models.RequestResponse.ServerRequester;

public class ConnectClientResponse
{
    [PHPProperty("cookie")]
    public required string Cookie { get; set; }

    [PHPProperty("account_id")]
    public required int AccountID { get; set; }

    [PHPProperty("nickname")]
    public required string Nickname { get; set; }

    [PHPProperty("super_id")]
    public required int SuperID { get; set; }

    [PHPProperty("account_type")]
    public required int AccountType { get; set; }

    [PHPProperty("level")]
    public required int Level { get; set; }

    [PHPProperty("clan_id")]
    public int? ClanID { get; set; }

    [PHPProperty("tag")]
    public string? ClanTag { get; set; }

    [PHPProperty("infos")]
    public string Infos { get; set; } = "";

    [PHPProperty("game_cookie")]
    public string GameCookie { get; set; } = "16cb3211-5253-45a8-bcb9-10d037ec9303";

    [PHPProperty("my_upgrades")]
    public required List<string> MyUpgrades { get; set; }

    [PHPProperty("selected_upgrades")]
    public required List<string> SelectedUpgrades { get; set; }
}
