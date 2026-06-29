namespace MERRICK.DatabaseContext.Constants;

/// <summary>
///     A server-defined message seeded into every account's inbox when the account is created.
/// </summary>
public sealed record SystemMessageDefinition(string Subject, string Subtitle, string BodyTitle, string Body, string Footer);

/// <summary>
///     The system messages seeded into every account's inbox at account-creation time (see <see cref="Interceptors.SystemMessageInterceptor"/>).
///     Adding a definition here seeds it into every account created afterwards.
/// </summary>
public static class SystemMessages
{
    public static readonly IReadOnlyList<SystemMessageDefinition> All =
    [
        new SystemMessageDefinition
        (
            Subject: "Welcome To Project KONGOR !",
            Subtitle: "keeping the real Heroes Of Newerth alive since 2022",
            BodyTitle: "Hello Newerthian,",
            Body: @"<p>Project KONGOR is a community-driven effort to keep the real Heroes Of Newerth alive.</p><p class=""link""><a href=""https://github.com/Project-KONGOR-Open-Source"">Visit The Project On GitHub</a></p>",
            Footer: "[K]ONGOR"
        )
    ];

    /// <summary>
    ///     Materialises a system message definition into an inbox message for the given account.
    /// </summary>
    public static Message ToMessage(this SystemMessageDefinition definition, Account account) => new ()
    {
        Account = account,
        Subject = definition.Subject,
        Subtitle = definition.Subtitle,
        BodyTitle = definition.BodyTitle,
        Body = definition.Body,
        Footer = definition.Footer
    };
}
