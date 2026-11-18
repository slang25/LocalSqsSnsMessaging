using Amazon.S3;
using Amazon.S3.Model;

namespace InMemoryAws.Tests.AwsEvaluationSuite;

/// <summary>What the S3 object annotation ports share: boto3's annotation calls, over the .NET SDK.</summary>
public static partial class S3Fidelity
{
    /// <summary><c>s3.put_object_annotation(Bucket=..., Key=..., AnnotationName=..., AnnotationPayload=...)</c>.</summary>
    public static Task<PutObjectAnnotationResponse> PutAnnotation(IAmazonS3 s3, string bucket, string key, string name, byte[] payload)
    {
        ArgumentNullException.ThrowIfNull(s3);
        return s3.PutObjectAnnotationAsync(new PutObjectAnnotationRequest
        {
            BucketName = bucket,
            Key = key,
            AnnotationName = name,
            AnnotationPayload = new MemoryStream(payload),
        });
    }

    /// <summary><c>s3.get_object_annotation(...)["AnnotationPayload"].read()</c>.</summary>
    public static async Task<byte[]> AnnotationPayload(IAmazonS3 s3, string bucket, string key, string name) =>
        (await GetAnnotation(s3, bucket, key, name)).Payload;

    /// <summary><c>s3.get_object_annotation(...)</c>: the payload read whole, and the response's <c>ContentLength</c>.</summary>
    public static async Task<(byte[] Payload, long ContentLength)> GetAnnotation(IAmazonS3 s3, string bucket, string key, string name)
    {
        ArgumentNullException.ThrowIfNull(s3);
        using var response = await s3.GetObjectAnnotationAsync(new GetObjectAnnotationRequest { BucketName = bucket, Key = key, AnnotationName = name });
        using var memory = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memory);
        return (memory.ToArray(), response.ContentLength);
    }

    /// <summary><c>s3.list_object_annotations(Bucket=..., Key=..., ...)</c>.</summary>
    public static Task<ListObjectAnnotationsResponse> ListAnnotations(IAmazonS3 s3, string bucket, string key, string? prefix = null, int? max = null)
    {
        ArgumentNullException.ThrowIfNull(s3);
        return s3.ListObjectAnnotationsAsync(new ListObjectAnnotationsRequest
        {
            BucketName = bucket,
            Key = key,
            AnnotationPrefix = prefix,
            MaxAnnotationResults = max,
        });
    }

    /// <summary><c>[a["AnnotationName"] for a in listed.get("Annotations", [])]</c>.</summary>
    public static List<string> AnnotationNames(ListObjectAnnotationsResponse listed)
    {
        ArgumentNullException.ThrowIfNull(listed);
        return [.. (listed.Annotations ?? []).Select(a => a.AnnotationName)];
    }
}
