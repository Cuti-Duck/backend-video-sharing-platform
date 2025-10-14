using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using backend_video_sharing_platform.Application.DTOs.Auth;
using backend_video_sharing_platform.Application.Interfaces;
using System.Diagnostics.CodeAnalysis;
using DtoConfirmSignUpRequest = backend_video_sharing_platform.Application.DTOs.Auth.ConfirmSignUpRequest;


namespace backend_video_sharing_platform.Infrastructure.Services
{
    [ExcludeFromCodeCoverage]
    public sealed class CognitoAuthService : ICognitoAuthService
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

        /// <summary>
        /// Đăng ký tài khoản mới vào Cognito User Pool.
        /// </summary>
        public async Task<RegisterUserResponse> RegisterAsync(RegisterUserRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return new RegisterUserResponse { Success = false, Message = "Email is required." };
            if (!request.Email.Contains("@"))
                return new RegisterUserResponse { Success = false, Message = "Invalid email format." };

            try
            {
                var e164 = NormalizePhone(request.PhoneNumber);

                var signUp = new SignUpRequest
                {
                    ClientId = _cfg["AWS:Cognito:ClientId"],
                    Username = request.Email,
                    Password = request.Password,
                    UserAttributes = new List<AttributeType>
                    {
                        new AttributeType { Name = "email", Value = request.Email },
                        new AttributeType { Name = "name", Value = request.Name },
                        new AttributeType { Name = "gender", Value = string.IsNullOrWhiteSpace(request.Gender) ? "unspecified" : request.Gender },
                        new AttributeType { Name = "birthdate", Value = request.BirthDate }
                    }
                };

                if (!string.IsNullOrWhiteSpace(e164))
                    signUp.UserAttributes.Add(new AttributeType { Name = "phone_number", Value = e164 });

                var resp = await _provider.SignUpAsync(signUp, ct);

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
            catch (InvalidLambdaResponseException ex)
            {
                _logger.LogError(ex, "Lambda trigger error during signup for {Email}", request.Email);
                return new RegisterUserResponse { Success = false, Message = "Internal signup trigger error." };
            }
            catch (InvalidParameterException ex)
            {
                _logger.LogWarning(ex, "Invalid parameter during signup for {Email}", request.Email);
                return new RegisterUserResponse { Success = false, Message = "Invalid input or attribute mismatch." };
            }
            catch (TooManyRequestsException)
            {
                return new RegisterUserResponse { Success = false, Message = "Too many requests. Please try again later." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during signup for {Email}", request.Email);
                return new RegisterUserResponse { Success = false, Message = "Registration failed. Please try again." };
            }
        }

        public async Task<BasicResponse> ConfirmSignUpAsync(DtoConfirmSignUpRequest request, CancellationToken ct = default)
        {
            try
            {
                await _provider.ConfirmSignUpAsync(new Amazon.CognitoIdentityProvider.Model.ConfirmSignUpRequest
                {
                    ClientId = _cfg["AWS:Cognito:ClientId"],
                    Username = request.Email,
                    ConfirmationCode = request.Code
                }, ct);


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
            catch (UserNotFoundException)
            {
                return new BasicResponse { Success = false, Message = "User not found." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Confirm signup error for {Email}", request.Email);
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

        private static string NormalizePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
            var p = phone.Trim();
            if (p.StartsWith("+")) return p;
            if (p.StartsWith("0")) return "+84" + p.TrimStart('0');
            return "+" + p;
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
                        { "PASSWORD", request.Password }
                    }
                };

                var resp = await _provider.InitiateAuthAsync(authReq, ct);

                if (resp.AuthenticationResult != null)
                {
                    return new LoginResponse
                    {
                        Success = true,
                        Message = "Login successful.",
                        AccessToken = resp.AuthenticationResult.AccessToken,
                        IdToken = resp.AuthenticationResult.IdToken,
                        RefreshToken = resp.AuthenticationResult.RefreshToken
                    };
                }

                return new LoginResponse { Success = false, Message = "Authentication failed." };
            }
            catch (UserNotConfirmedException)
            {
                return new LoginResponse { Success = false, Message = "Account not confirmed. Please verify your email." };
            }
            catch (NotAuthorizedException)
            {
                return new LoginResponse { Success = false, Message = "Invalid email or password." };
            }
            catch (UserNotFoundException)
            {
                return new LoginResponse { Success = false, Message = "User does not exist." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error for {Email}", request.Email);
                return new LoginResponse { Success = false, Message = "Login failed. Please try again later." };
            }
        }
    }
}
