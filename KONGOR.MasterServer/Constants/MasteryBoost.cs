namespace KONGOR.MasterServer.Constants;

/// <summary>
///     These are the mastery boost products that can be purchased in the in-game store.
/// </summary>
/// <remarks>
///     The values should always be in accordance with the store items list.
/// </remarks>
public static class MasteryBoost
{
    public static class Regular
    {
        public const int ProductCode = 3609;
        public const int GoldCost = 150;
    }

    public static class Super
    {
        public const int ProductCode = 4605;
        public const int GoldCost = 1500;
    }

    public static class Bundle
    {
        public const int ProductCode = 4606;
        public const int GoldCost = 1500;
    }
}
