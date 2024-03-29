﻿namespace MERRICK.Database.Entities.Core;

[Index(nameof(Name), nameof(Tag), IsUnique = true)]
public class Clan
{
    [Key]
    public Guid ID { get; set; }

    [MaxLength(20)]
    public required string Name { get; set; }

    [MaxLength(4)]
    public required string Tag { get; set; }

    public List<Account> Members { get; set; } = [];

    public DateTime TimestampCreated { get; set; } = DateTime.UtcNow;
}
