using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace backend_video_sharing_platform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IAmazonCognitoIdentityProvider _cognitoClient;

        public UserController(IAmazonCognitoIdentityProvider cognitoClient)
        {
            _cognitoClient = cognitoClient;
        }

        /// <summary>
        /// Lấy thông tin chi tiết user từ AWS Cognito
        /// </summary>
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                // Lấy access token từ header Authorization
                var authHeader = Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return Unauthorized(new { Message = "Thiếu hoặc sai định dạng Access Token." });
                }

                var token = authHeader.Replace("Bearer ", "").Trim();

                // Gọi AWS Cognito API để lấy thông tin user
                var request = new GetUserRequest
                {
                    AccessToken = token
                };

                var response = await _cognitoClient.GetUserAsync(request);

                // Chuyển đổi danh sách attributes sang JSON
                var userAttributes = response.UserAttributes
                    .ToDictionary(attr => attr.Name, attr => attr.Value);

                return Ok(new
                {
                    Message = "Thông tin người dùng từ AWS Cognito:",
                    Username = response.Username,
                    Attributes = userAttributes
                });
            }
            catch (NotAuthorizedException)
            {
                return Unauthorized(new { Message = "Token hết hạn hoặc không hợp lệ." });
            }
            catch (UserNotFoundException)
            {
                return NotFound(new { Message = "Không tìm thấy người dùng trong Cognito." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Lỗi khi lấy thông tin người dùng.",
                    Error = ex.Message
                });
            }
        }
    }
}
