namespace KONGOR.MasterServer.Constants;

/// <summary>
///     These are the mastery boost products that can be purchased in the in-game store.
/// </summary>
/// <remarks>
///     The values should always be in accordance with the store items list.
/// </remarks>
public static class MasteryBoost
{
    /// <summary>
    ///     The window of time after a match is recorded during which a mastery boost can be applied to it.
    ///     The applied boost cache entries expire after this window, so binding the boost eligibility to it guarantees that an entry can never expire while its match is still eligible for boosting.
    /// </summary>
    public static readonly TimeSpan ApplicationWindow = TimeSpan.FromDays(7);

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
