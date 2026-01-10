using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MERRICK.DatabaseContext.Entities;

[Index(nameof(AccountId), IsUnique = false)]
public class Notification
{
    [Key]
    public int NotificationId { get; set; }

    public string Content { get; set; } = string.Empty;

    public int AccountId { get; set; }

    public DateTime TimestampCreated { get; set; }
}
