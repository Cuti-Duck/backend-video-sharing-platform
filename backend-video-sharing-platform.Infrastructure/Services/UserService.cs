using backend_video_sharing_platform.Application.DTOs.User;
using backend_video_sharing_platform.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace backend_video_sharing_platform.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IChannelService _channelService;
        private readonly ILogger<UserService> _logger;
        private readonly IStorageService _storageService;

        public UserService(IUserRepository userRepository, IChannelService channelService, ILogger<UserService> logger, IStorageService storageService)
        {
            _userRepository = userRepository;
            _channelService = channelService;
            _storageService = storageService;
            _logger = logger;
        }

        public async Task<bool> UpdateUserAsync(string userId, UpdateUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User {userId} không tồn tại.");
                return false;
            }

            bool isUpdated = false;

            if (!string.IsNullOrEmpty(request.Name) && request.Name != user.Name)
            {
                user.Name = request.Name;
                await _channelService.UpdateChannelNameByUserIdAsync(user.UserId, request.Name);
                isUpdated = true;
            }

            if (!string.IsNullOrEmpty(request.Gender) && request.Gender != user.Gender)
            {
                user.Gender = request.Gender;
                isUpdated = true;
            }

            if (!string.IsNullOrEmpty(request.BirthDate) && request.BirthDate != user.BirthDate)
            {
                user.BirthDate = request.BirthDate;
                isUpdated = true;
            }

            if (!string.IsNullOrEmpty(request.PhoneNumber) && request.PhoneNumber != user.PhoneNumber)
            {
                user.PhoneNumber = request.PhoneNumber;
                isUpdated = true;
            }

            if (isUpdated)
            {
                await _userRepository.SaveAsync(user);
                _logger.LogInformation($"Đã cập nhật thông tin user {userId} thành công.");
            }

            return isUpdated;
        }
        public async Task<UploadAvatarResponse?> UploadAvatarAsync(
            string userId,
            Stream fileStream,
            string fileName,
            string contentType,
            CancellationToken ct = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} không tồn tại", userId);
                return null;
            }

            if (contentType != "image/jpeg" && contentType != "image/png")
                throw new InvalidOperationException("Chỉ chấp nhận ảnh .jpg hoặc .png.");

            var ext = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(ext))
                ext = contentType == "image/png" ? ".png" : ".jpg";

            var key = $"avatars/{userId}/{DateTime.UtcNow.Ticks}{ext}";

            var url = await _storageService.UploadFileAsync(fileStream, key, contentType, ct);

            user.AvatarUrl = url;
            await _userRepository.SaveAsync(user, ct);

            _logger.LogInformation("User {UserId} cập nhật avatar thành công", userId);

            return new UploadAvatarResponse { AvatarUrl = url };
        }
    }
}
