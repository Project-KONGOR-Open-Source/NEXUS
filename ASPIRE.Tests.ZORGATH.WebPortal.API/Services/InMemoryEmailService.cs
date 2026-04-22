namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Services;

/// <summary>
///     An in-memory <see cref="IEmailService"/> that records every call to the send-methods in a <see cref="ConcurrentDictionary{TKey, TValue}"/> keyed by recipient address.
///     Tests use this to assert on what a controller would have sent without paying for a real SMTP round-trip — registered as a singleton inside each <see cref="ZORGATHIntegrationWebApplicationFactory"/>, so every test gets its own empty inbox.
/// </summary>
/// <remarks>
///     Concurrent tests may share a host (and therefore a singleton instance) when TUnit parallelises variants, so the dictionary value uses <see cref="ConcurrentQueue{T}"/> and the send-methods <see cref="ConcurrentDictionary{TKey, TValue}.GetOrAdd(TKey, Func{TKey, TValue})"/> to stay thread-safe under contention.
/// </remarks>
public sealed class InMemoryEmailService : IEmailService
{
    private ConcurrentDictionary<string, ConcurrentQueue<RecordedEmail>> Store { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Returns the chronological list of emails recorded for <paramref name="emailAddress"/>.
    ///     Comparison is case-insensitive so assertions do not need to mirror the casing the controller normalised into.
    /// </summary>
    public IReadOnlyList<RecordedEmail> GetRecordedFor(string emailAddress)
    {
        return Store.TryGetValue(emailAddress, out ConcurrentQueue<RecordedEmail>? queue)
            ? queue.ToArray()
            : [];
    }

    public Task<bool> SendEmailAddressRegistrationLink(string emailAddress, string token)
        => Record(emailAddress, EmailKind.EmailAddressRegistrationLink, new Dictionary<string, string> { ["Token"] = token });

    public Task<bool> SendEmailAddressRegistrationConfirmation(string emailAddress, string accountName)
        => Record(emailAddress, EmailKind.EmailAddressRegistrationConfirmation, new Dictionary<string, string> { ["AccountName"] = accountName });

    public Task<bool> SendAccountPasswordResetLink(string emailAddress, string token, string generatedPassword, List<string> accountNames)
        => Record(emailAddress, EmailKind.AccountPasswordResetLink, new Dictionary<string, string>
        {
            ["Token"] = token,
            ["GeneratedPassword"] = generatedPassword,
            ["AccountNames"] = string.Join(",", accountNames)
        });

    public Task<bool> SendAccountPasswordResetConfirmation(string emailAddress, List<string> accountNames)
        => Record(emailAddress, EmailKind.AccountPasswordResetConfirmation, new Dictionary<string, string> { ["AccountNames"] = string.Join(",", accountNames) });

    public Task<bool> SendAccountPasswordUpdateLink(string emailAddress, string token, List<string> accountNames)
        => Record(emailAddress, EmailKind.AccountPasswordUpdateLink, new Dictionary<string, string>
        {
            ["Token"] = token,
            ["AccountNames"] = string.Join(",", accountNames)
        });

    public Task<bool> SendAccountPasswordUpdateConfirmation(string emailAddress, List<string> accountNames)
        => Record(emailAddress, EmailKind.AccountPasswordUpdateConfirmation, new Dictionary<string, string> { ["AccountNames"] = string.Join(",", accountNames) });

    public Task<bool> SendEmailAddressUpdateLink(string emailAddress, string token)
        => Record(emailAddress, EmailKind.EmailAddressUpdateLink, new Dictionary<string, string> { ["Token"] = token });

    public Task<bool> SendEmailAddressUpdateConfirmation(string oldEmailAddress, string newEmailAddress)
        => Record(newEmailAddress, EmailKind.EmailAddressUpdateConfirmation, new Dictionary<string, string> { ["OldEmailAddress"] = oldEmailAddress });

    private Task<bool> Record(string recipient, EmailKind kind, IReadOnlyDictionary<string, string> parameters)
    {
        ConcurrentQueue<RecordedEmail> queue = Store.GetOrAdd(recipient, _ => new ConcurrentQueue<RecordedEmail>());

        queue.Enqueue(new RecordedEmail(recipient, kind, parameters, DateTimeOffset.UtcNow));

        return Task.FromResult(true);
    }
}

/// <summary>
///     A single send-call captured by <see cref="InMemoryEmailService"/>.
/// </summary>
/// <param name="Recipient">The address the email was dispatched to. For update-confirmation this is the new address.</param>
/// <param name="Kind">Which <see cref="IEmailService"/> method recorded the entry.</param>
/// <param name="Parameters">The non-recipient arguments (token, account names, etc.) supplied to the send-method, captured verbatim for assertion purposes.</param>
/// <param name="SentAt">The UTC time the entry was recorded.</param>
public sealed record RecordedEmail(string Recipient, EmailKind Kind, IReadOnlyDictionary<string, string> Parameters, DateTimeOffset SentAt);

/// <summary>
///     Identifies which send-method on <see cref="IEmailService"/> produced a <see cref="RecordedEmail"/>.
/// </summary>
public enum EmailKind
{
    EmailAddressRegistrationLink,
    EmailAddressRegistrationConfirmation,
    AccountPasswordResetLink,
    AccountPasswordResetConfirmation,
    AccountPasswordUpdateLink,
    AccountPasswordUpdateConfirmation,
    EmailAddressUpdateLink,
    EmailAddressUpdateConfirmation
}
