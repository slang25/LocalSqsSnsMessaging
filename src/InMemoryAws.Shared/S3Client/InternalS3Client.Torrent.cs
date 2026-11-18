using InMemoryAws.S3.Model;

namespace InMemoryAws.S3;

internal sealed partial class InternalS3Client
{
    /// <summary>
    /// GetObjectTorrent: S3 has retired BitTorrent delivery, and answers every request for it, on any
    /// bucket that exists, with <c>MethodNotAllowed</c>.
    /// </summary>
    public override Task<GetObjectTorrentOutput> GetObjectTorrentAsync(GetObjectTorrentRequest request, CancellationToken cancellationToken)
    {
        lock (Store.Gate)
        {
            GetBucket(request.Bucket);
            throw S3Errors.MethodNotAllowed("GET", "OBJECT");
        }
    }
}
