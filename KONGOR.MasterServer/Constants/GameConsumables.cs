//namespace KONGOR.MasterServer.Constants;

///// <summary>
/////     Consumables contain an asterisk in the code, indicating the amount of items of that type.
/////     e.g. "ma.Mastery Boost * 10" or "ma.Super Mastery Boost * 1".
///// </summary>
//public static class GameConsumables
//{
//    /// <summary>
//    ///     Does not automatically save the tracked changes to the database context.
//    ///     A manual save is required, e.g. "await MerrickContext.SaveChangesAsync();".
//    /// </summary>
//    /// <returns>
//    ///     TRUE if the operation completed successfully, or FALSE if the operation did not complete.
//    /// </returns>
//    public static bool AddConsumable(ElementUser user, string code, int amount = 1)
//        => code switch
//        {
//            "ma.Mastery Boost"                                                  =>  MasteryRewards.AddMasteryBoost(user, amount),
//            "ma.Super Mastery Boost"                                            =>  MasteryRewards.AddSuperMasteryBoost(user, amount),
//            not null when Coupons.Select(coupon => coupon.Code).Contains(code)  =>  MasteryRewards.AddMasteryCoupon(user, Coupons.Single(coupon => coupon.Code == code).Hero, amount),
//            _                                                                   =>  false
//        };

//    /// <summary>
//    ///     Does not automatically save the tracked changes to the database context.
//    ///     A manual save is required, e.g. "await MerrickContext.SaveChangesAsync();".
//    /// </summary>
//    /// <returns>
//    ///     TRUE if the operation completed successfully, or FALSE if the operation did not complete.
//    /// </returns>
//    public static bool RemoveConsumable(ElementUser user, string code, int amount = 1)
//        => code switch
//        {
//            "ma.Mastery Boost"                                                  =>  MasteryRewards.RemoveMasteryBoost(user, amount),
//            "ma.Super Mastery Boost"                                            =>  MasteryRewards.RemoveSuperMasteryBoost(user, amount),
//            not null when Coupons.Select(coupon => coupon.Code).Contains(code)  =>  MasteryRewards.RemoveMasteryCoupon(user, Coupons.Single(coupon => coupon.Code == code).Hero, amount),
//            _                                                                   =>  false
//        };

//    public static IEnumerable<string> AllConsumables => Array.Empty<string>()
//        .Concat(MasteryBoostProducts)
//        .Concat(Coupons.Select(coupon => coupon.Code));

//    public static Dictionary<string, IEnumerable<string>> AllConsumablesGroupedByType => new()
//    {
//        {"Mastery Boost Products", MasteryBoostProducts},
//        {"Mastery Coupons", Coupons.Select(coupon => coupon.Code)}
//    };

//    /// <summary>
//    ///     These are custom consumables implemented as part of the Project KONGOR Hero Mastery feature.
//    /// </summary>
//    public static string[] MasteryBoostProducts =
//    {
//        "ma.Mastery Boost",
//        "ma.Super Mastery Boost"
//    };

//    /// <summary>
//    ///     These are coupons meant to be used in the store when purchasing alternative avatars.
//    ///     They will apply a silver/gold discount to the product(s) that they apply to.
//    ///     In the user's collection of upgrades, coupons have the prefix "cp" (Upgrade.Type enumeration value 13).
//    /// </summary>
//    public static Coupon[] Coupons = JsonConvert.DeserializeObject<Coupon[]>(File.ReadAllText(SeedData.Coupons)) ?? Array.Empty<Coupon>();

//    public static List<string> GetOwnedMasteryBoostProducts(IEnumerable<string> ownedUpgradesAndConsumables)
//        => GetOwnedConsumables(ownedUpgradesAndConsumables)
//            .Where(consumable => consumable.StartsWith("ma.")).ToList();

//    public static Dictionary<string, Coupon> GetOwnedCoupons(IEnumerable<string> ownedUpgradesAndConsumables)
//        => GetOwnedConsumables(ownedUpgradesAndConsumables)
//            .Where(consumable => consumable.StartsWith("cp.")).ToDictionary(consumable => consumable, consumable => Coupons.Single(coupon => consumable.StartsWith(coupon.Code)));

//    /// <summary>
//    ///     <para>
//    ///         Replaces all unusable hero-specific discount coupons with discount coupons which can be used for any avatar in the store.
//    ///     </para>
//    ///
//    ///     <br/>
//    /// 
//    ///     <para>
//    ///         Does not automatically save the tracked changes to the database context.
//    ///         A manual save is required, e.g. "await MerrickContext.SaveChangesAsync();".
//    ///     </para>
//    /// </summary>
//    public static void ResolveUnusableCoupons(ElementUser user)
//    {
//        Dictionary<string, Coupon> coupons = GetOwnedCoupons(user.UnlockedUpgradeCodes);

//        if (coupons.Any().Equals(false)) return;

//        foreach (KeyValuePair<string, Coupon> coupon in coupons.Where(coupon => coupon.Value.Hero != string.Empty) /* Skip All Avatar Mastery Coupon */ )
//        {
//            bool unusable = coupon.Value.ApplicableProductsList.Intersect(user.UnlockedUpgradeCodes).Count() == coupon.Value.ApplicableProductsList.Count();

//            if (unusable.Equals(false)) continue;

//            int amount = Convert.ToInt32(coupon.Key.Replace(coupon.Value.Code, string.Empty).Replace(" * ", string.Empty));

//            RemoveConsumable(user, coupon.Value.Code, amount);
//            AddConsumable(user, Coupons.Single(replacement => replacement.Hero == string.Empty).Code, amount);
//        }
//    }

//    private static IEnumerable<string> GetOwnedConsumables(IEnumerable<string> ownedUpgradesAndConsumables)
//    {
//        Regex pattern = new(@"^(?<type>ma|cp){1}\.(?<name>.+) \* (?<amount>\d+)$");

//        return from upgrade in ownedUpgradesAndConsumables let match = pattern.Match(upgrade) where match.Success select upgrade;
//    }
//}
