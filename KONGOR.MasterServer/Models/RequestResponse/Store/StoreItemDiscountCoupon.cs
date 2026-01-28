using System.Globalization;

using KONGOR.MasterServer.Attributes.Serialisation;

namespace KONGOR.MasterServer.Models.RequestResponse.Store;

public class StoreItemDiscountCoupon
{
    // TODO: Implement This As Part Of The Hero Mastery Feature

    [PHPProperty("product_id")]
    public required int Id { get; set; }

    [PhpIgnore]
    public required string Name { get; set; }

    [PhpIgnore]
    public string Code => $"cp.{Name}";

    [PhpIgnore]
    public required string Hero { get; set; }

    [PHPProperty("coupon_id")]
    // The integer value of the "discount" property part of the form data which gets sent when making a purchase in the in-game store.
    // The "discount" property will be set to this value after choosing to use a discount coupon.
    public int DiscountId => Id;

    [PHPProperty("coupon_products")]
    public required string ApplicableProducts { get; set; }

    [PhpIgnore]
    public required IEnumerable<string> ApplicableProductsList { get; set; }

    [PHPProperty("discount")]
    public double DiscountGold => 0.75;

    [PHPProperty("mmp_discount")]
    public double DiscountSilver => 0.75;

    [PHPProperty("end_time")]
    public string DiscountExpirationDate { get; set; } = DateTimeOffset.UtcNow.AddYears(1000).ToString("dd MMMM yyyy", CultureInfo.InvariantCulture);
}
