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

        public async Task DeleteFileAsync(string key, CancellationToken ct = default)
        {
            await _s3.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            }, ct);
        }

        public async Task DeleteFolderAsync(string folderPrefix, CancellationToken ct = default)
        {
            try
            {
                var listRequest = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = folderPrefix
                };

                var list = await _s3.ListObjectsV2Async(listRequest, ct);

                // Kiểm tra null và kiểm tra có objects không
                if (list?.S3Objects == null || list.S3Objects.Count == 0)
                {
                    // Không có gì để xóa, return luôn
                    return;
                }

                // Xóa từng object
                foreach (var item in list.S3Objects)
                {
                    await _s3.DeleteObjectAsync(new DeleteObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = item.Key
                    }, ct);
                }
            }
            catch (AmazonS3Exception ex)
            {
                // Log error nhưng không throw - cho phép tiếp tục xóa video
                // Có thể folder không tồn tại hoặc đã bị xóa trước đó
                Console.WriteLine($"S3 Error deleting folder {folderPrefix}: {ex.Message}");
            }
        }

    }
}
