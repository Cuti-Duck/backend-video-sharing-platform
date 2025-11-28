using backend_video_sharing_platform.Application.DTOs.User;
using backend_video_sharing_platform.Application.Interfaces;
using Microsoft.Extensions.Logging;
using backend_video_sharing_platform.Application.Common.Exceptions;

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

        public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();

            //if (!users.Any())
            //{
            //    throw new NotFoundException("No users found in the system.");
            //}

            return users.Select(u => new UserResponse
            {
                UserId = u.UserId,
                Email = u.Email,
                Name = u.Name,
                Gender = u.Gender,
                BirthDate = u.BirthDate,
                PhoneNumber = u.PhoneNumber,
                AvatarUrl = u.AvatarUrl,
                ChannelId = u.ChannelId,
                CreatedAt = u.CreatedAt
            }).ToList();
        }

        public async Task<UserResponse?> GetUserByIdAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
                throw new NotFoundException($"User with id '{userId}' does not exist.");

            return new UserResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                Name = user.Name,
                Gender = user.Gender,
                BirthDate = user.BirthDate,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                ChannelId = user.ChannelId,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<bool> UpdateUserAsync(string userId, UpdateUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User {userId} does not exist.");
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
                _logger.LogInformation($"User {userId} updated successfully.");
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
                _logger.LogWarning("User {UserId} does not exist", userId);
                return null;
            }

            if (contentType != "image/jpeg" && contentType != "image/png")
                throw new InvalidOperationException("Only .jpg or .png images are accepted.");

            var ext = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(ext))
                ext = contentType == "image/png" ? ".png" : ".jpg";

            var key = $"avatars/{userId}/{DateTime.UtcNow.Ticks}{ext}";

            var url = await _storageService.UploadFileAsync(fileStream, key, contentType, ct);

            user.AvatarUrl = url;
            await _userRepository.SaveAsync(user, ct);

            _logger.LogInformation("User {UserId} updated avatar successfully", userId);

            try
            {
                await _channelService.UpdateChannelAvatarAsync(userId, url);
                _logger.LogInformation("Channel avatar synced for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to sync channel avatar for user {UserId}", userId);
                // Don't throw - user avatar already uploaded successfully
            }

            return new UploadAvatarResponse { AvatarUrl = url };
        }
    }
}
