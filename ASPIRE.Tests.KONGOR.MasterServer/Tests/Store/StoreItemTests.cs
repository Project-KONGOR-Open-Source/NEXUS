namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Store;

/// <summary>
///     Locks the type-code, prefix, and client-type mappings on <see cref="StoreItem"/> against the keys of the <c>ProductTypeToPrefix</c> table in <c>global_main.lua</c> and the prefixes used to construct prefixed codes throughout the master server.
///     A mismatch on <see cref="StoreItem.GetTypeName"/> silently breaks the Plinko chest reward UI by rendering <c>general_product_type_</c> as raw text; a mismatch on <see cref="StoreItem.GetTypeCode"/> or <see cref="StoreItem.GetPrefix"/> breaks prefixed-code lookups and the Plinko view-chest response.
/// </summary>
public sealed class StoreItemTests
{
    [Test]
    [Arguments(StoreItemType.ChatNameColour,     "cc")]
    [Arguments(StoreItemType.ChatSymbol,         "cs")]
    [Arguments(StoreItemType.AccountIcon,        "ai")]
    [Arguments(StoreItemType.AlternativeAvatar,  "aa")]
    [Arguments(StoreItemType.AnnouncerVoice,     "av")]
    [Arguments(StoreItemType.Taunt,              "t")]
    [Arguments(StoreItemType.Courier,            "c")]
    [Arguments(StoreItemType.Hero,               "h")]
    [Arguments(StoreItemType.EarlyAccessProduct, "eap")]
    [Arguments(StoreItemType.Status,             "s")]
    [Arguments(StoreItemType.Miscellaneous,      "m")]
    [Arguments(StoreItemType.Ward,               "w")]
    [Arguments(StoreItemType.Enhancement,        "en")]
    [Arguments(StoreItemType.Coupon,             "cp")]
    [Arguments(StoreItemType.Mastery,            "ma")]
    [Arguments(StoreItemType.Creep,              "cr")]
    [Arguments(StoreItemType.Building,           "bu")]
    [Arguments(StoreItemType.TauntBadge,         "tb")]
    [Arguments(StoreItemType.TeleportEffect,     "te")]
    [Arguments(StoreItemType.SelectionCircle,    "sc")]
    [Arguments(StoreItemType.Bundle,             "")]
    public async Task GetTypeCode_ReturnsExpectedValueForEveryStoreItemType(StoreItemType type, string expected)
    {
        await Assert.That(StoreItem.GetTypeCode(type)).IsEqualTo(expected);
    }

    [Test]
    [Arguments(StoreItemType.ChatNameColour,     "cc.")]
    [Arguments(StoreItemType.ChatSymbol,         "cs.")]
    [Arguments(StoreItemType.AccountIcon,        "ai.")]
    [Arguments(StoreItemType.AlternativeAvatar,  "aa.")]
    [Arguments(StoreItemType.AnnouncerVoice,     "av.")]
    [Arguments(StoreItemType.Taunt,              "t.")]
    [Arguments(StoreItemType.Courier,            "c.")]
    [Arguments(StoreItemType.Hero,               "h.")]
    [Arguments(StoreItemType.EarlyAccessProduct, "eap.")]
    [Arguments(StoreItemType.Status,             "s.")]
    [Arguments(StoreItemType.Miscellaneous,      "m.")]
    [Arguments(StoreItemType.Ward,               "w.")]
    [Arguments(StoreItemType.Enhancement,        "en.")]
    [Arguments(StoreItemType.Coupon,             "cp.")]
    [Arguments(StoreItemType.Mastery,            "ma.")]
    [Arguments(StoreItemType.Creep,              "cr.")]
    [Arguments(StoreItemType.Building,           "bu.")]
    [Arguments(StoreItemType.TauntBadge,         "tb.")]
    [Arguments(StoreItemType.TeleportEffect,     "te.")]
    [Arguments(StoreItemType.SelectionCircle,    "sc.")]
    [Arguments(StoreItemType.Bundle,             "")]
    public async Task GetPrefix_ReturnsExpectedValueForEveryStoreItemType(StoreItemType type, string expected)
    {
        await Assert.That(StoreItem.GetPrefix(type)).IsEqualTo(expected);
    }

    /// <summary>
    ///     The values asserted here must match the keys of the <c>ProductTypeToPrefix</c> table in <c>global_main.lua</c> exactly; the chest reward UI feeds the response's <c>"product_type"</c> through that table to build the <c>general_product_type_&lt;code&gt;</c> localisation key.
    ///     Types without an entry in the LUA table (<see cref="StoreItemType.Coupon"/>, <see cref="StoreItemType.Mastery"/>, <see cref="StoreItemType.Creep"/>, <see cref="StoreItemType.Building"/>, <see cref="StoreItemType.TauntBadge"/>, <see cref="StoreItemType.TeleportEffect"/>, <see cref="StoreItemType.SelectionCircle"/>, <see cref="StoreItemType.Bundle"/>) intentionally return an empty string; none of them are currently configured as Plinko-droppable.
    /// </summary>
    [Test]
    [Arguments(StoreItemType.ChatNameColour,     "Chat Color")]
    [Arguments(StoreItemType.ChatSymbol,         "Chat Symbol")]
    [Arguments(StoreItemType.AccountIcon,        "Account Icon")]
    [Arguments(StoreItemType.AlternativeAvatar,  "Alt Avatar")]
    [Arguments(StoreItemType.AnnouncerVoice,     "Alt Announcement")]
    [Arguments(StoreItemType.Taunt,              "Taunt")]
    [Arguments(StoreItemType.Courier,            "Couriers")]
    [Arguments(StoreItemType.Hero,               "Hero")]
    [Arguments(StoreItemType.EarlyAccessProduct, "EAP")]
    [Arguments(StoreItemType.Status,             "Status")]
    [Arguments(StoreItemType.Miscellaneous,      "Misc")]
    [Arguments(StoreItemType.Ward,               "Ward")]
    [Arguments(StoreItemType.Enhancement,        "Enhancement")]
    [Arguments(StoreItemType.Coupon,             "")]
    [Arguments(StoreItemType.Mastery,            "")]
    [Arguments(StoreItemType.Creep,              "")]
    [Arguments(StoreItemType.Building,           "")]
    [Arguments(StoreItemType.TauntBadge,         "")]
    [Arguments(StoreItemType.TeleportEffect,     "")]
    [Arguments(StoreItemType.SelectionCircle,    "")]
    [Arguments(StoreItemType.Bundle,             "")]
    public async Task GetTypeName_ReturnsExpectedValueForEveryStoreItemType(StoreItemType type, string expected)
    {
        await Assert.That(StoreItem.GetTypeName(type)).IsEqualTo(expected);
    }

    /// <summary>
    ///     Guards against new <see cref="StoreItemType"/> values being added without corresponding entries in <see cref="StoreItem.GetTypeCode"/>, <see cref="StoreItem.GetPrefix"/>, and <see cref="StoreItem.GetTypeName"/>.
    ///     If a new type is introduced, the parameterised tests above will no longer be exhaustive and this assertion will fail until they are extended.
    /// </summary>
    [Test]
    public async Task EveryStoreItemTypeIsCoveredByTheParameterisedTestsAbove()
    {
        const int expectedStoreItemTypeCount = 21;

        await Assert.That(Enum.GetValues<StoreItemType>().Length).IsEqualTo(expectedStoreItemTypeCount);
    }
}
