namespace ASPIRE.Common.Constants;

public static class OOTB
{
    public static class Accounts
    {
        /// <summary>
        ///     The built-in, out-of-the-box <see cref="GUEST"/> account which ships with a publicly-known password.
        ///     It exists so that self-hosters can authenticate to the client and join matches out-of-the-box.
        ///     This account has USER permissions on the user portal.
        /// </summary>
        public static class GUEST
        {
            private const string Name = "GUEST";

            public const string Password = "KONGOR";

            public const string EmailAddress = "guest@project.kongor";

            /// <summary>
            ///     Returns the name of the GUEST account with the given index.
            ///     The database is seeded with 50 GUEST accounts, so the index must be between 1 and 50 (inclusive).
            /// </summary>
            public static string? GetName(int index)
                => index >= 1 && index <= 50 ? $"{Name}-{index:00}" : null;
        }

        /// <summary>
        ///     The built-in, out-of-the-box <see cref="OPERATOR"/> account which ships with a publicly-known password.
        ///     It exists so that self-hosters can host match servers out-of-the-box.
        ///     This account has CUSTODIAN permissions on the user portal.
        /// </summary>
        public static class OPERATOR
        {
            public const string Name = "OPERATOR";

            public const string Password = "KONGOR";

            public const string EmailAddress = "operator@project.kongor";
        }
    }
}
