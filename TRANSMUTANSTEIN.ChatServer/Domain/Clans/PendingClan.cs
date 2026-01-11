namespace TRANSMUTANSTEIN.ChatServer.Domain.Clans;

public class PendingClan
{
    public required string ClanName { get; set; }
    public required string ClanTag { get; set; }
    public int CreatorAccountId { get; set; }
    public required List<int> MembersAccountId { get; set; }
    public required List<bool> Accepted { get; set; }
    public DateTime CreationTime { get; set; }

    public bool IsValid => CreationTime.AddMinutes(2) >= DateTime.UtcNow;
}

public class PendingClanInvite
{
    public required string ClanName { get; set; }
    public required string ClanTag { get; set; }
    public int ClanId { get; set; }
    public int InitiatorAccountId { get; set; }
    public int InvitedAccountId { get; set; }
    public DateTime CreationTime { get; set; }
    public bool IsValid => CreationTime.AddMinutes(2) >= DateTime.UtcNow;
}