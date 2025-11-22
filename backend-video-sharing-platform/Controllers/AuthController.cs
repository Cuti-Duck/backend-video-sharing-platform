using backend_video_sharing_platform.Application.DTOs.Auth;
using backend_video_sharing_platform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_video_sharing_platform.API.Controllers
{
    [ApiController]
    [Route("api/auths")]
    public sealed class AuthController : ControllerBase
    {
        private readonly ICognitoAuthService _auth;
        public AuthController(ICognitoAuthService auth) => _auth = auth;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var result = await _auth.RegisterAsync(request, ct);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("confirm")]
        public async Task<IActionResult> Confirm([FromBody] ConfirmSignUpRequest request, CancellationToken ct)
        {
            var result = await _auth.ConfirmSignUpAsync(request, ct);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("resend-code")]
        public async Task<IActionResult> ResendCode([FromBody] string email, CancellationToken ct)
        {
            var result = await _auth.ResendConfirmationCodeAsync(email, ct);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
        {
            var result = await _auth.LoginAsync(request, ct);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [Authorize]
        [HttpGet("validate-token")]
        public IActionResult ValidateToken()
        {
            // Lấy toàn bộ claims từ token
            var claims = User.Claims.Select(c => new
            {
                Type = c.Type,
                Value = c.Value
            }).ToList();

            return Ok(new
            {
                Message = "Token hợp lệ — Đây là danh sách claims từ AWS Cognito:",
                Claims = claims
            });
        }
    }
}
