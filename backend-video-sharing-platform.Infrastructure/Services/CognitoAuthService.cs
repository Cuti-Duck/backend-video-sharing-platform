using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using backend_video_sharing_platform.Application.DTOs.Auth;
using backend_video_sharing_platform.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

// Alias để tránh trùng tên DTO vs SDK
using ConfirmSignUpRequestModel = Amazon.CognitoIdentityProvider.Model.ConfirmSignUpRequest;
using DTOConfirmSignUpRequest = backend_video_sharing_platform.Application.DTOs.Auth.ConfirmSignUpRequest;

namespace backend_video_sharing_platform.Infrastructure.Services
{
    public class CognitoAuthService : ICognitoAuthService
    {
        private readonly IAmazonCognitoIdentityProvider _provider;
        private readonly IConfiguration _cfg;
        private readonly ILogger<CognitoAuthService> _logger;

        public CognitoAuthService(
            IAmazonCognitoIdentityProvider provider,
            IConfiguration cfg,
            ILogger<CognitoAuthService> logger)
        {
            _provider = provider;
            _cfg = cfg;
            _logger = logger;
        }

        private string GenerateSecretHash(string username)
        {
            var clientId = _cfg["AWS:Cognito:ClientId"];
            var clientSecret = _cfg["AWS:Cognito:ClientSecret"];
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(clientSecret));
            var data = Encoding.UTF8.GetBytes(username + clientId);
            return Convert.ToBase64String(hmac.ComputeHash(data));
        }

        private static string ToE164(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
            var p = phone.Trim();
            if (p.StartsWith("+")) return p;
            if (p.StartsWith("0")) return "+84" + p.TrimStart('0'); // Việt Nam
            return "+" + p;
        }

        public async Task<RegisterUserResponse> RegisterAsync(RegisterUserRequest request, CancellationToken ct = default)
        {
            try
            {
                var attrs = new List<AttributeType>
                {
                    new AttributeType { Name = "email", Value = request.Email },
                    new AttributeType { Name = "name", Value = request.Name },
                    new AttributeType { Name = "gender", Value = request.Gender },
                    new AttributeType { Name = "birthdate", Value = request.BirthDate }
                };

                var phone = ToE164(request.PhoneNumber);
                if (!string.IsNullOrWhiteSpace(phone))
                    attrs.Add(new AttributeType { Name = "phone_number", Value = phone });

                var resp = await _provider.SignUpAsync(new SignUpRequest
                {
                    ClientId = _cfg["AWS:Cognito:ClientId"],
                    SecretHash = GenerateSecretHash(request.Email),
                    Username = request.Email,
                    Password = request.Password,
                    UserAttributes = attrs
                }, ct);

                return new RegisterUserResponse
                {
                    Success = true,
                    Message = resp.UserConfirmed == true
                        ? "Registered and already confirmed."
                        : "Registered successfully. Please check your email for verification code."
                };
            }
            catch (UsernameExistsException)
            {
                return new RegisterUserResponse { Success = false, Message = "This email is already registered." };
            }
            catch (InvalidPasswordException ex)
            {
                return new RegisterUserResponse { Success = false, Message = $"Weak password: {ex.Message}" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register error for {Email}", request.Email);
                return new RegisterUserResponse { Success = false, Message = "Registration failed. Please try again." };
            }
        }

        public async Task<BasicResponse> ConfirmSignUpAsync(DTOConfirmSignUpRequest request, CancellationToken ct = default)
        {
            try
            {
                var confirmReq = new ConfirmSignUpRequestModel
                {
                    ClientId = _cfg["AWS:Cognito:ClientId"],
                    SecretHash = GenerateSecretHash(request.Email),
                    Username = request.Email,
                    ConfirmationCode = request.Code
                };

                await _provider.ConfirmSignUpAsync(confirmReq, ct);
                return new BasicResponse { Success = true, Message = "Email verified successfully." };
            }
            catch (CodeMismatchException)
            {
                return new BasicResponse { Success = false, Message = "Invalid verification code." };
            }
            catch (ExpiredCodeException)
            {
                return new BasicResponse { Success = false, Message = "Verification code expired." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Confirm error for {Email}", request.Email);
                return new BasicResponse { Success = false, Message = "Confirm failed. Please try again." };
            }
        }

        public async Task<BasicResponse> ResendConfirmationCodeAsync(string email, CancellationToken ct = default)
        {
            try
            {
                await _provider.ResendConfirmationCodeAsync(new ResendConfirmationCodeRequest
                {
                    ClientId = _cfg["AWS:Cognito:ClientId"],
                    SecretHash = GenerateSecretHash(email),
                    Username = email
                }, ct);

                return new BasicResponse { Success = true, Message = "A new verification code has been sent." };
            }
            catch (UserNotFoundException)
            {
                return new BasicResponse { Success = false, Message = "User not found." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resend code error for {Email}", email);
                return new BasicResponse { Success = false, Message = "Resend failed. Please try again." };
            }
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            try
            {
                var authReq = new InitiateAuthRequest
                {
                    AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                    ClientId = _cfg["AWS:Cognito:ClientId"],
                    AuthParameters = new Dictionary<string, string>
                    {
                        { "USERNAME", request.Email },
                        { "PASSWORD", request.Password },
                        { "SECRET_HASH", GenerateSecretHash(request.Email) }
                    }
                };

                var resp = await _provider.InitiateAuthAsync(authReq, ct);

                return new LoginResponse
                {
                    Success = true,
                    Message = "Login successful.",
                    AccessToken = resp.AuthenticationResult.AccessToken,
                    IdToken = resp.AuthenticationResult.IdToken,
                    RefreshToken = resp.AuthenticationResult.RefreshToken
                };
            }
            catch (UserNotConfirmedException)
            {
                return new LoginResponse { Success = false, Message = "Account not confirmed. Please verify your email." };
            }
            catch (NotAuthorizedException)
            {
                return new LoginResponse { Success = false, Message = "Invalid email or password." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error for {Email}", request.Email);
                return new LoginResponse { Success = false, Message = "Login failed. Please try again later." };
            }
        }
    }
}
