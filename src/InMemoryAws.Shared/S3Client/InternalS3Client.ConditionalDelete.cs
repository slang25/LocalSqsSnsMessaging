namespace InMemoryAws.S3;

/// <summary>Conditional deletes.</summary>
internal sealed partial class InternalS3Client
{
    /// <summary>
    /// DeleteObject's <c>If-Match</c>: the key's current version must have that ETag
    /// (<c>PreconditionFailed</c> otherwise), and a key with no current version is <c>NoSuchKey</c>.
    /// </summary>
    private static void CheckDeleteIfMatch(S3Bucket bucket, string key, string? ifMatch)
    {
        if (ifMatch is null)
        {
            return;
        }

        if (bucket.Current(key) is not { IsDeleteMarker: false } current)
        {
            throw S3Errors.NoSuchKey(key);
        }

        if (ifMatch.Trim() != "*" && !string.Equals(ifMatch.Trim().Trim('"'), current.ETag.Trim('"'), StringComparison.Ordinal))
        {
            throw S3Errors.PreconditionFailed("If-Match");
        }
    }
}
