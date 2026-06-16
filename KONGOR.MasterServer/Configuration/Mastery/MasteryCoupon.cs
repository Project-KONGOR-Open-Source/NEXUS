namespace KONGOR.MasterServer.Configuration.Mastery;

/// <summary>
///     A mastery coupon definition, seeded from configuration.
///     Each coupon applies a discount to a single hero's alternative avatars, except the "All Avatar Mastery Coupon", which has an empty <see cref="Hero"/> and applies to any avatar.
/// </summary>
public class MasteryCoupon
{
    public required int ID { get; set; }

    public required string Name { get; set; }

    public required string Hero { get; set; }

    /// <summary>
    ///     The prefixed code under which an owned coupon is stored (for example "cp.Accursed Mastery Coupon").
    /// </summary>
    public string Code => $"cp.{Name}";
}
