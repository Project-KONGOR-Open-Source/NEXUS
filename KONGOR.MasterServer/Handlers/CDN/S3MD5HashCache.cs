namespace KONGOR.MasterServer.Handlers.CDN;

/// <summary>
///     Caches MD5 hashes for files served by the S3 CDN endpoint, keyed by path and validated by last write time and length.
/// </summary>
public sealed class S3MD5HashCache
{
    private readonly ConcurrentDictionary<string, (DateTime LastWriteTimeUtc, long FileLength, string ETag)> cache = new();

    /// <summary>
    ///     Retrieves the cached ETag for the specified file path, or computes, caches, and returns it.
    /// </summary>
    /// <param name="filePath">The absolute path to the file.</param>
    /// <param name="fileInfo">The file information containing size and last write timestamp.</param>
    /// <returns>The quoted lowercase MD5 hex string formatted as an ETag.</returns>
    public string GetOrCreateETag(string filePath, FileInfo fileInfo)
    {
        if (cache.TryGetValue(filePath, out (DateTime LastWriteTimeUtc, long FileLength, string ETag) existingEntry))
        {
            if (existingEntry.LastWriteTimeUtc == fileInfo.LastWriteTimeUtc && existingEntry.FileLength == fileInfo.Length)
            {
                return existingEntry.ETag;
            }
        }

        using FileStream stream = File.OpenRead(filePath);
        using MD5 md5 = MD5.Create();
        byte[] hashBytes = md5.ComputeHash(stream);
        string etag = $@"""{Convert.ToHexStringLower(hashBytes)}""";

        cache[filePath] = (fileInfo.LastWriteTimeUtc, fileInfo.Length, etag);
        return etag;
    }

    /// <summary>
    ///     Retrieves the cached ETag for the specified file path, or computes, caches, and returns it.
    /// </summary>
    /// <param name="filePath">The absolute path to the file.</param>
    /// <returns>The quoted lowercase MD5 hex string formatted as an ETag.</returns>
    public string GetOrCreateETag(string filePath)
    {
        FileInfo fileInfo = new(filePath);
        return GetOrCreateETag(filePath, fileInfo);
    }
}
