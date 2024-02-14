namespace MERRICK.Database.Entities.Relational;

[Index(nameof(Name), IsUnique = true)]
public class IgnoredPeer
{
    // Currently Unable To Name This Property "ID": https://github.com/dotnet/efcore/issues/29380
    public required Guid GUID { get; set; }

    [MaxLength(15)]
    public required string Name { get; set; }
}
