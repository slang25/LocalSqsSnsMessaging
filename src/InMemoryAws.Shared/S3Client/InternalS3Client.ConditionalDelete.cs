using InMemoryAws.S3.Model;

namespace InMemoryAws.S3;

internal sealed partial class InternalS3Client
{
    /// <summary>
    /// A DeleteObjects entry's own conditions (<c>ETag</c>, <c>LastModifiedTime</c>, <c>Size</c>), each
    /// checked against the version the entry deletes: one that fails is <c>PreconditionFailed</c> for that
    /// entry alone, and an entry with conditions on a key with no object is <c>NoSuchKey</c>.
    /// </summary>
    private static void CheckDeleteConditions(S3Bucket bucket, string key, ObjectIdentifier item)
    {
        if (item.ETag is null && item.LastModifiedTime is null && item.Size is null)
        {
            return;
        }

        var target = item.VersionId is { } versionId ? bucket.Version(key, versionId) : bucket.Current(key);
        if (target is null || target.IsDeleteMarker)
        {
            throw S3Errors.NoSuchKey(key);
        }

        if ((item.ETag is { } etag && !ETagMatches(etag, target.ETag))
            || (item.LastModifiedTime is { } lastModified && lastModified.ToUniversalTime() != target.LastModified)
            || (item.Size is { } size && size != target.Size))
        {
            throw S3Errors.PreconditionFailed("If-Match");
        }
    }
}
