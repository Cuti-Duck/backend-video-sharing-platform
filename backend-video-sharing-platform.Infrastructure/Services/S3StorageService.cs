using Amazon.S3;
using Amazon.S3.Model;
using backend_video_sharing_platform.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace backend_video_sharing_platform.Infrastructure.Services
{
    public class S3StorageService : IStorageService
    {
        private readonly IAmazonS3 _s3;
        private readonly string _bucketName;

        public S3StorageService(IAmazonS3 s3, IConfiguration config)
        {
            _s3 = s3;
            _bucketName = config["AWS:S3:BucketName"]!;
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string key, string contentType, CancellationToken ct = default)
        {
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = fileStream,
                ContentType = contentType
            };

            await _s3.PutObjectAsync(request, ct);

            // Trả về public URL vì bucket đã cấu hình Public Read
            return $"https://{_bucketName}.s3.ap-northeast-1.amazonaws.com/{key}";
        }
    }
}
