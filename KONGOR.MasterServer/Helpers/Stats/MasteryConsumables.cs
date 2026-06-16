namespace KONGOR.MasterServer.Helpers.Stats;

/// <summary>
///     Manages the mastery consumables that live in a user's owned store items as counted entries (for example "ma.Mastery Boost * 3" or "cp.Accursed Mastery Coupon * 1"), and issues the per-hero rewards granted when a hero crosses a mastery level.
/// </summary>
/// <remarks>
///     None of these methods save the tracked changes. The caller is responsible for calling <see cref="MerrickContext.SaveChangesAsync"/>.
/// </remarks>
public static class MasteryConsumables
{
    public const string MasteryBoostCode = "ma.Mastery Boost";
    public const string SuperMasteryBoostCode = "ma.Super Mastery Boost";

    private const string MasteryBoostPrefix = MasteryBoostCode + " * ";
    private const string SuperMasteryBoostPrefix = SuperMasteryBoostCode + " * ";

    /// <summary>
    ///     The number of regular mastery boosts the user owns.
    /// </summary>
    public static int MasteryBoostsOwned(User user) => GetCount(user, MasteryBoostPrefix);

    /// <summary>
    ///     The number of super mastery boosts the user owns.
    /// </summary>
    public static int SuperMasteryBoostsOwned(User user) => GetCount(user, SuperMasteryBoostPrefix);

    public static bool AddMasteryBoost(User user, int amount = 1) => AddCounted(user, MasteryBoostPrefix, amount);

    public static bool RemoveMasteryBoost(User user, int amount = 1) => RemoveCounted(user, MasteryBoostPrefix, amount);

    public static bool AddSuperMasteryBoost(User user, int amount = 1) => AddCounted(user, SuperMasteryBoostPrefix, amount);

    public static bool RemoveSuperMasteryBoost(User user, int amount = 1) => RemoveCounted(user, SuperMasteryBoostPrefix, amount);

    /// <summary>
    ///     Adds a mastery coupon for the hero with the given identifier (an empty identifier represents the all-avatar coupon).
    /// </summary>
    public static bool AddMasteryCoupon(User user, string heroIdentifier, int amount = 1)
    {
        MasteryCoupon? coupon = JSONConfiguration.MasteryCoupons.SingleOrDefault(coupon => coupon.Hero.Equals(heroIdentifier));

        if (coupon is null)
            return false;

        bool added = AddCounted(user, $"{coupon.Code} * ", amount);

        ResolveUnusableCoupons(user);

        return added;
    }

    /// <summary>
    ///     Removes a mastery coupon for the hero with the given identifier (an empty identifier represents the all-avatar coupon).
    /// </summary>
    public static bool RemoveMasteryCoupon(User user, string heroIdentifier, int amount = 1)
    {
        MasteryCoupon? coupon = JSONConfiguration.MasteryCoupons.SingleOrDefault(coupon => coupon.Hero.Equals(heroIdentifier));

        if (coupon is null)
            return false;

        return RemoveCounted(user, $"{coupon.Code} * ", amount);
    }

    /// <summary>
    ///     Issues the reward granted when a hero reaches the given mastery level.
    ///     Level 1 grants the hero's mastery account icon, levels 2 and 11 grant the hero's mastery coupon, levels 4, 8 and 12 grant silver coins (1000, 2000 and 2000 respectively), and levels 6, 9 and 14 grant plinko tickets (100, 200 and 200 respectively).
    ///     Levels 3, 5, 7, 10, 13 and 15 correspond to cosmetic rewards (silver border, silver badge, silver teleport, gold upgrade, emote and epic upgrade) which are handled automatically by the client, and so are intentionally not issued here.
    /// </summary>
    /// <returns>
    ///     <see langword="true"/> if a reward was issued, otherwise <see langword="false"/>.
    /// </returns>
    public static bool IssueHeroMasteryLevelReward(User user, int level, string heroIdentifier, ILogger logger)
    {
        switch (level)
        {
            case 1:
            {
                string? heroName = Heroes.GetNameByIdentifier(heroIdentifier);

                if (heroName is null)
                {
                    logger.LogError(@"[BUG] Unable To Find Hero Name For Hero With Identifier ""{HeroIdentifier}""", heroIdentifier);

                    return false;
                }

                // Map The In-Game Hero Name To The Mastery Icon Product Name For Heroes Whose Mastery Icon Product Uses A Different Name
                heroName = heroName switch
                {
                    "Blood Hunter"  =>  "Bloodhunter",
                    "Circe"         =>  "Circe The Deceiver",
                    "Qi"            =>  "Chi",
                    "Xemplar"       =>  "Mimix",
                    _               =>  heroName
                };

                StoreItem? icon = JSONConfiguration.StoreItemsConfiguration.GetEnabledItemsByType(StoreItemType.AccountIcon)
                    .SingleOrDefault(item => item.Name.StartsWith("Mastery Icon", StringComparison.OrdinalIgnoreCase) && item.Name.EndsWith(heroName, StringComparison.OrdinalIgnoreCase));

                if (icon is null)
                {
                    logger.LogError(@"[BUG] Unable To Find Mastery Account Icon For Hero With Identifier ""{HeroIdentifier}""", heroIdentifier);

                    return false;
                }

                if (user.OwnedStoreItems.Contains(icon.PrefixedCode).Equals(false))
                    user.OwnedStoreItems.Add(icon.PrefixedCode);

                return true;
            }

            case 2:
            case 11:
                return AddMasteryCoupon(user, heroIdentifier);

            case 4:  user.SilverCoins   += 1000;  return true;
            case 6:  user.PlinkoTickets += 0100;  return true;
            case 8:  user.SilverCoins   += 2000;  return true;
            case 9:  user.PlinkoTickets += 0200;  return true;
            case 12: user.SilverCoins   += 2000;  return true;
            case 14: user.PlinkoTickets += 0200;  return true;

            default: return false;
        }
    }

    /// <summary>
    ///     Replaces any owned hero-specific mastery coupon with the all-avatar mastery coupon when the user already owns every alternative avatar the hero-specific coupon could be applied to.
    /// </summary>
    public static void ResolveUnusableCoupons(User user)
    {
        MasteryCoupon? allAvatarCoupon = JSONConfiguration.MasteryCoupons.SingleOrDefault(coupon => coupon.Hero.Equals(string.Empty));

        if (allAvatarCoupon is null)
            return;

        foreach (MasteryCoupon coupon in JSONConfiguration.MasteryCoupons.Where(coupon => coupon.Hero.Equals(string.Empty).Equals(false)))
        {
            string ownedCouponPrefix = $"{coupon.Code} * ";

            string? ownedCoupon = user.OwnedStoreItems.SingleOrDefault(code => code.StartsWith(ownedCouponPrefix, StringComparison.Ordinal));

            if (ownedCoupon is null)
                continue;

            List<string> applicableAvatars = JSONConfiguration.StoreItemsConfiguration.GetEnabledItemsByType(StoreItemType.AlternativeAvatar)
                .Where(item => item.Code.StartsWith(coupon.Hero, StringComparison.Ordinal))
                .Select(item => item.PrefixedCode).ToList();

            bool unusable = applicableAvatars.Intersect(user.OwnedStoreItems).Count() == applicableAvatars.Count;

            if (unusable.Equals(false))
                continue;

            int amount = int.TryParse(ownedCoupon[ownedCouponPrefix.Length..], out int parsed) ? parsed : 1;

            RemoveCounted(user, ownedCouponPrefix, amount);
            AddCounted(user, $"{allAvatarCoupon.Code} * ", amount);
        }
    }

    private static int GetCount(User user, string prefix)
    {
        string? item = user.OwnedStoreItems.SingleOrDefault(code => code.StartsWith(prefix, StringComparison.Ordinal));

        if (item is null)
            return 0;

        return int.TryParse(item[prefix.Length..], out int count) ? count : 0;
    }

    private static bool AddCounted(User user, string prefix, int amount)
    {
        if (amount < 1)
            return false;

        string? existing = user.OwnedStoreItems.SingleOrDefault(code => code.StartsWith(prefix, StringComparison.Ordinal));

        if (existing is null)
        {
            user.OwnedStoreItems.Add($"{prefix}{amount}");

            return true;
        }

        int owned = int.TryParse(existing[prefix.Length..], out int parsed) ? parsed : 0;

        user.OwnedStoreItems.Remove(existing);
        user.OwnedStoreItems.Add($"{prefix}{owned + amount}");

        return true;
    }

    private static bool RemoveCounted(User user, string prefix, int amount)
    {
        if (amount < 1)
            return false;

        string? existing = user.OwnedStoreItems.SingleOrDefault(code => code.StartsWith(prefix, StringComparison.Ordinal));

        if (existing is null)
            return false;

        int owned = int.TryParse(existing[prefix.Length..], out int parsed) ? parsed : 0;

        user.OwnedStoreItems.Remove(existing);

        if (owned - amount > 0)
            user.OwnedStoreItems.Add($"{prefix}{owned - amount}");

        return true;
    }
}
