using KONGOR.MasterServer.Models.RequestResponse.Store;

using PhpSerializerNET;

namespace ASPIRE.Tests.KONGOR.MasterServer.Tests.Serialisation;

public class StoreItemDiscountCouponTests
{
    [Test]
    public async Task StoreItemDiscountCoupon_SerialisesCorrectly()
    {
        StoreItemDiscountCoupon storeItemDiscountCoupon = new()
        {
            Id = 99999,
            Name = "discount_coupon",
            Hero = "test.hero",
            ApplicableProducts = "test.hero.avatar1,test.hero.avatar2",
            ApplicableProductsList = ["test.hero.avatar1", "test.hero.avatar2"],
            DiscountExpirationDate = "1000000000",
        };

        string serialisedData = PhpSerialization.Serialize(storeItemDiscountCoupon);

        const string expectedSerialisationOutput = @"a:6:{s:10:""product_id"";i:99999;s:9:""coupon_id"";i:99999;s:15:""coupon_products"";s:35:""test.hero.avatar1,test.hero.avatar2"";s:8:""discount"";d:0.75;s:12:""mmp_discount"";d:0.75;s:8:""end_time"";s:10:""1000000000"";}";

        await Assert.That(serialisedData).IsEqualTo(expectedSerialisationOutput);
    }
}
