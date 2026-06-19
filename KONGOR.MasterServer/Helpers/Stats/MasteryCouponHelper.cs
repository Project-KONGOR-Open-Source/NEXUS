namespace KONGOR.MasterServer.Helpers.Stats;

/// <summary>
///     Builds the discount coupon data surfaced to the client for owned mastery coupons, and resolves the coupon applied to a store purchase.
/// </summary>
public static class MasteryCouponHelper
{
    /// <summary>
    ///     Builds the discount coupon surfaced to the client for an owned coupon entry (for example "cp.Accursed Mastery Coupon * 1").
    /// </summary>
    /// <returns>
    ///     The discount coupon, or <see langword="null"/> if the owned entry does not correspond to a known mastery coupon.
    /// </returns>
    public static StoreItemDiscountCoupon? BuildDiscountCoupon(string ownedCouponCode)
    {
        MasteryCoupon? coupon = JSONConfiguration.MasteryCoupons
            .SingleOrDefault(coupon => ownedCouponCode.StartsWith($"{coupon.Code} * ", StringComparison.Ordinal));

        if (coupon is null)
            return null;

        List<string> applicableAvatars = ApplicableAvatars(coupon.Hero);

        return new StoreItemDiscountCoupon
        {
            Id = coupon.ID,
            Name = coupon.Name,
            Hero = coupon.Hero,
            ApplicableProducts = string.Join(',', applicableAvatars),
            ApplicableProductsList = applicableAvatars
        };
    }

    /// <summary>
    ///     Resolves the mastery coupon to apply to a store purchase, given the discount code submitted by the client.
    ///     The coupon is only applied when it is owned by the user and the product being purchased is one of the coupon's applicable alternative avatars.
    /// </summary>
    /// <returns>
    ///     The applicable owned coupon, or <see langword="null"/> if no discount should be applied.
    /// </returns>
    public static MasteryCoupon? ResolveApplicableCoupon(User user, int discountCode, StoreItem storeItem)
    {
        if (discountCode is 0)
            return null;

        MasteryCoupon? coupon = JSONConfiguration.MasteryCoupons.SingleOrDefault(coupon => coupon.ID == discountCode);

        if (coupon is null)
            return null;

        bool owned = user.OwnedStoreItems.Any(item => item.StartsWith($"{coupon.Code} * ", StringComparison.Ordinal));

        if (owned.Equals(false))
            return null;

        return ApplicableAvatars(coupon.Hero).Contains(storeItem.PrefixedCode) ? coupon : null;
    }

    /// <summary>
    ///     The prefixed codes of the alternative avatars a coupon for the given hero applies to.
    ///     The all-avatar coupon (an empty hero identifier) applies to every alternative avatar.
    /// </summary>
    private static List<string> ApplicableAvatars(string heroIdentifier)
    {
        IEnumerable<StoreItem> avatars = JSONConfiguration.StoreItemsConfiguration.GetEnabledItemsByType(StoreItemType.AlternativeAvatar);

        if (heroIdentifier.Equals(string.Empty))
            return avatars.Select(avatar => avatar.PrefixedCode).ToList();

        return avatars.Where(avatar => avatar.Code.StartsWith($"{heroIdentifier}.", StringComparison.Ordinal))
            .Select(avatar => avatar.PrefixedCode).ToList();
    }
}
