namespace ASPIRE.Tests.Infrastructure.Client;

/// <summary>
///     A scoped wrapper around <see cref="IWireMockAdminApi"/> that automatically prepends a unique path prefix to every mapping's path matchers before forwarding the call to the underlying client.
///     Concurrent tests register mappings under distinct path segments and never collide, and callers do not need to know the prefix: they specify logical paths (e.g. <c>/api/example</c>) and the wrapper rewrites them to <c>/{prefix}/api/example</c> before posting.
/// </summary>
public sealed class ScopedWireMockClient(IWireMockAdminApi client, string pathPrefix)
{
    /// <summary>
    ///     Creates a new mapping with automatic path scoping.
    /// </summary>
    public Task<StatusModel> PostMappingAsync(MappingModel mapping, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mapping);

        ApplyPathScope(mapping);

        return client.PostMappingAsync(mapping, cancellationToken);
    }

    /// <summary>
    ///     Creates multiple mappings with automatic path scoping.
    /// </summary>
    public Task<StatusModel> PostMappingsAsync(IList<MappingModel> mappings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mappings);

        foreach (MappingModel mapping in mappings)
        {
            ApplyPathScope(mapping);
        }

        return client.PostMappingsAsync(mappings, cancellationToken);
    }

    /// <summary>
    ///     Replaces an existing mapping with automatic path scoping.
    /// </summary>
    public Task<StatusModel> PutMappingAsync(Guid guid, MappingModel mapping, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mapping);

        ApplyPathScope(mapping);

        return client.PutMappingAsync(guid, mapping, cancellationToken);
    }

    /// <summary>
    ///     Deletes a mapping by its unique identifier.
    /// </summary>
    public Task<StatusModel> DeleteMappingAsync(Guid guid, CancellationToken cancellationToken = default)
        => client.DeleteMappingAsync(guid, cancellationToken);

    /// <summary>
    ///     Retrieves a mapping by its unique identifier.
    /// </summary>
    public Task<MappingModel> GetMappingAsync(Guid guid, CancellationToken cancellationToken = default)
        => client.GetMappingAsync(guid, cancellationToken);

    /// <summary>
    ///     Retrieves all registered mappings.
    /// </summary>
    public Task<IList<MappingModel>> GetMappingsAsync(CancellationToken cancellationToken = default)
        => client.GetMappingsAsync(cancellationToken);

    private void ApplyPathScope(MappingModel mapping)
    {
        if (mapping.Request?.Path is not PathModel { Matchers: { } matchers })
        {
            return;
        }

        string scopedPrefix = $"/{pathPrefix}/";

        foreach (MatcherModel matcher in matchers)
        {
            if (matcher.Pattern is string pattern && pattern.StartsWith(scopedPrefix, StringComparison.Ordinal) is false)
            {
                matcher.Pattern = $"{scopedPrefix}{pattern.TrimStart('/')}";
            }

            if (matcher.Patterns is { } patterns)
            {
                for (int index = 0; index < patterns.Length; index++)
                {
                    if (patterns[index] is string currentPattern && currentPattern.StartsWith(scopedPrefix, StringComparison.Ordinal) is false)
                    {
                        patterns[index] = $"{scopedPrefix}{currentPattern.TrimStart('/')}";
                    }
                }
            }
        }
    }
}
