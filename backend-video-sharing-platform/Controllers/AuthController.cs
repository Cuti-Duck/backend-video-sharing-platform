using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using backend_video_sharing_platform.Application.DTOs.Auth;
using backend_video_sharing_platform.Application.Interfaces;

namespace backend_video_sharing_platform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class AuthController : ControllerBase
    {
        private readonly ICognitoAuthService _auth;

        public AuthController(ICognitoAuthService auth)
        {
            _auth = auth;
        }

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
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
                return BadRequest(new { message = "Email and Code are required." });

            var result = await _auth.ConfirmSignUpAsync(request, ct);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("resend-code")]
        public async Task<IActionResult> ResendCode([FromBody] string email, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "Email is required." });

            var result = await _auth.ResendConfirmationCodeAsync(email, ct);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
        {
            var result = await _auth.LoginAsync(request, ct);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
