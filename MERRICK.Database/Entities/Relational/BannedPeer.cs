﻿namespace MERRICK.Database.Entities.Relational;

[Index(nameof(Name), IsUnique = true)]
public class BannedPeer
{
    public required int Identifier { get; set; }

    // Currently Unable To Easily Name The "ID" Property "ID" Due To A Conflict With A JSON Property Name: https://github.com/dotnet/efcore/issues/29380
    // Having A JSON-Mapped Property Called "ID" In An Entity Which Maps To JSON Causes An Entity Framework Exception At Runtime
    // The Following Is A Messy Workaround: https://github.com/dotnet/efcore/issues/29380#issuecomment-1979288993

    [MaxLength(15)]
    public required string Name { get; set; }

    [MaxLength(30)]
    public required string BanReason { get; set; }
}
