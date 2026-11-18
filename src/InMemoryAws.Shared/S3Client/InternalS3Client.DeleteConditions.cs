using InMemoryAws.S3.Model;

namespace InMemoryAws.S3;

internal sealed partial class InternalS3Client
{
    /// <summary>
    /// DeleteObject's conditions (<c>If-Match</c>, <c>x-amz-if-match-last-modified-time</c>,
    /// <c>x-amz-if-match-size</c>), checked against the version the delete names, or the current
    /// one: with none, or a delete marker, the delete is <c>NoSuchKey</c>; a condition that does
    /// not hold is 412, and nothing is deleted.
    /// </summary>
    private static void CheckDeleteConditions(S3Bucket bucket, string key, DeleteObjectRequest request)
    {
        if (request.IfMatch is null && request.IfMatchLastModifiedTime is null && request.IfMatchSize is null)
        {
            return;
        }

        var version = request.VersionId is { } versionId ? bucket.Version(key, versionId) : bucket.Current(key);
        if (version is null || version.IsDeleteMarker)
        {
            throw S3Errors.NoSuchKey(key);
        }

        if (request.IfMatch is { } ifMatch && !ETagMatches(ifMatch, version.ETag))
        {
            throw S3Errors.PreconditionFailed("If-Match");
        }

        if (request.IfMatchLastModifiedTime is { } lastModified && lastModified.ToUniversalTime() != version.LastModified)
        {
            throw S3Errors.PreconditionFailed("x-amz-if-match-last-modified-time");
        }

        if (request.IfMatchSize is { } size && size != version.Size)
        {
            throw S3Errors.PreconditionFailed("x-amz-if-match-size");
        }
    }
}
