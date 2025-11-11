namespace backend_video_sharing_platform.Application.Interfaces
{
    public interface IStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string key, string contentType, CancellationToken ct = default);
    }
}
