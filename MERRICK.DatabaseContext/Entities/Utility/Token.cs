namespace MERRICK.DatabaseContext.Entities.Utility;

public class Token
{
    [Key]
    public int ID { get; set; }

    public required TokenPurpose Purpose { get; set; }

    public required string EmailAddress { get; set; }

    public DateTimeOffset TimestampCreated { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? TimestampConsumed { get; set; }

    public required Guid Value { get; set; }

    public required string Data { get; set; }
}

public enum TokenPurpose
{
    EmailAddressVerification,
    EmailAddressUpdate
}
