﻿namespace KONGOR.MasterServer.RequestResponseModels.Store;

public class StoreItemDiscountCoupon
{
    // TODO: Implement This As Part Of The Hero Mastery Feature

    //[PhpProperty("product_id")]
    //public int Id { get; set; }

    //[PhpIgnore]
    //public string Name { get; set; }

    //[PhpIgnore]
    //public string Code => $"cp.{Name}";

    //[PhpIgnore]
    //public string Hero { get; set; }

    //[PhpProperty("coupon_id")]
    //// The integer value of the "discount" property part of the form data which gets sent when making a purchase in the in-game store.
    //// The "discount" property will be set to this value after choosing to use a discount coupon.
    //public int DiscountId => Id;

    //[PhpProperty("coupon_products")]
    //public string ApplicableProducts => GetApplicableProducts();

    //[PhpIgnore]
    //public IEnumerable<string> ApplicableProductsList => GetApplicableProductsList();

    //[PhpProperty("discount")]
    //public double DiscountGold => 0.75;

    //[PhpProperty("mmp_discount")]
    //public double DiscountSilver => 0.75;

    //[PhpProperty("end_time")]
    //public string DiscountExpirationDate => DateTime.UtcNow.AddYears(1000).ToString("dd MMMM yyyy", CultureInfo.InvariantCulture);

    //private string GetApplicableProducts()
    //{
    //    IEnumerable<string> avatars = DataSeedHelpers.AllUpgrades
    //        .Where(upgrade => upgrade.UpgradeType is Upgrade.Type.AlternativeAvatar && upgrade.Code.StartsWith(Hero))
    //        .Select(upgrade => upgrade.PrefixedCode);

    //    return string.Join(',', avatars);
    //}

    //private IEnumerable<string> GetApplicableProductsList()
    //{
    //    IEnumerable<string> avatars = DataSeedHelpers.AllUpgrades
    //        .Where(upgrade => upgrade.UpgradeType is Upgrade.Type.AlternativeAvatar && upgrade.Code.StartsWith(Hero))
    //        .Select(upgrade => upgrade.PrefixedCode);

    //    return avatars;
    //}
}
