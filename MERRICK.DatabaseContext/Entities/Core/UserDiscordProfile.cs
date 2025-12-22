using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MERRICK.DatabaseContext.Entities.Core;

[Index(nameof(DiscordID), IsUnique = true)]
public class UserDiscordProfile
{
    [Key]
    public int ID { get; set; }

    public int UserID { get; set; }
    
    [ForeignKey(nameof(UserID))]
    public User User { get; set; } = null!;

    [MaxLength(30)]
    public required string DiscordID { get; set; }

    [MaxLength(32)]
    public required string Username { get; set; }

    [MaxLength(100)]
    public string? Avatar { get; set; }

    [MaxLength(100)]
    public string? Banner { get; set; }

    public bool EmailVerified { get; set; }

    public bool MfaEnabled { get; set; }
}
