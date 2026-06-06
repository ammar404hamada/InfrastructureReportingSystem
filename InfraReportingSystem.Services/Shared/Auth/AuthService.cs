using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Shared.Auth;
using InfraReportingSystem.ServiceAbstractions.Shared.Auth;
using InfraReportingSystem.Shared.DTOs.Shared.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace InfraReportingSystem.Services.Shared.Auth {

    public class AuthService : IAuthService
    {
        private const string PublicUserRole = "PublicUser";
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly IOtpService _otpService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public AuthService(
            UserManager<User> userManager, 
            IConfiguration configuration,
            IOtpService otpService,
            IRefreshTokenRepository refreshTokenRepository
            )
        {
            _userManager = userManager;
            _configuration = configuration;
            _otpService = otpService;
            _refreshTokenRepository = refreshTokenRepository;
        }
        public async Task<bool> ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto)
        {
            var user = await _userManager.FindByEmailAsync(confirmEmailDto.UserEmail);
            if (user == null) return false;

            var isValid = await _otpService.VerifyOtp(
                user.Id,
                OtpPurpose.EmailConfirmation,
                confirmEmailDto.OtpCode
                );

            if (!isValid) return false;

            user.EmailConfirmed = true;
            user.Status = UserStatus.Active;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> ResendConfirmationEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;
            if (user.EmailConfirmed) return false;

            await _otpService.GenerateOtp(user.Id, OtpPurpose.EmailConfirmation);

            return true;
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
            if (user == null) return;

            await _otpService.GenerateOtp(user.Id, OtpPurpose.PasswordReset);
                
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "User does not exist."
                };

            if (await _userManager.IsLockedOutAsync(user))
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Your account is temporarily locked. Please try again later.",
                    Code = "ACCOUNT_LOCKED"
                };

            if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                await _userManager.AccessFailedAsync(user);

                if (await _userManager.IsLockedOutAsync(user))
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "Your account is temporarily locked. Please try again later.",
                        Code = "ACCOUNT_LOCKED"
                    };

                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Invalid password.",
                    Code = "INVALID_CREDENTIALS"
                };
            }

            if (!user.EmailConfirmed)
            {
                await _otpService.GenerateOtp(user.Id, OtpPurpose.EmailConfirmation);
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Please confirm your email first.",
                    Code = "EMAIL_CONFIRMATION_REQUIRED"
                };
            }

            if (user.Status == UserStatus.Inactive)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Your account is inactive.",
                    Code = "ACCOUNT_INACTIVE"
                };
            }
            if (user.Status == UserStatus.Suspended)
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Your account has been suspended.",
                    Code = "ACCOUNT_SUSPENDED"
                };
            if (user.Status == UserStatus.Locked)
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Your account is locked.",
                    Code = "ACCOUNT_LOCKED"
                };

            var userRole = await _userManager.GetRolesAsync(user);

            var token = GenerateJwtToken(user, userRole);
            var refreshToken = GenerateRefreshToken();

            refreshToken.UserId = user.Id;
            user.RefreshTokens ??= new List<RefreshToken>();
            user.RefreshTokens.Add(refreshToken);
            await _userManager.ResetAccessFailedCountAsync(user);
            await _userManager.UpdateAsync(user);

                return new LoginResponseDto
                {
                    Success = true,
                    Message = "Login successful.",
                    User = new UserDto
                    {
                        Name = user.Name,
                        Email = user.Email!,
                        Role = userRole.FirstOrDefault() ?? PublicUserRole
                    },
                    Token = token,
                    RefreshToken = refreshToken.Token,
                    RefreshTokenExpiresOn = refreshToken.ExpiresOn
                };
        }

        public async Task<RegisterResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null && !existingUser.EmailConfirmed)
            {
                try
                {
                    await _otpService.GenerateOtp(existingUser.Id, OtpPurpose.EmailConfirmation);
                }
                catch
                {
                    return new RegisterResponseDto
                    {
                        Success = false,
                        Message = "The account exists but the verification code could not be sent. Please try again later."
                    };
                }

                return new RegisterResponseDto
                {
                    Success = false,
                    Message = "Please confirm your email first.",
                    Code = "EMAIL_CONFIRMATION_REQUIRED"
                };

            } 
            
            else if (existingUser != null && existingUser.EmailConfirmed)
                return new RegisterResponseDto
                {
                    Success = false,
                    Message = "This email already exists."
                };

            var user = new User
            {
                Name = registerDto.Name,
                UserName = registerDto.Email,
                Email = registerDto.Email,
                Status = UserStatus.Inactive,
                PhoneNumber = registerDto.Phone,
                CreatedAt = DateTime.UtcNow,

            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
                return new RegisterResponseDto
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };

            var roleResult = await _userManager.AddToRoleAsync(user, PublicUserRole);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return new RegisterResponseDto
                {
                    Success = false,
                    Message = string.Join(", ", roleResult.Errors.Select(e => e.Description))
                };
            }

            try
            {
                await _otpService.GenerateOtp(user.Id, OtpPurpose.EmailConfirmation);
            }
            catch
            {
                await _userManager.DeleteAsync(user);
                return new RegisterResponseDto
                {
                    Success = false,
                    Message = "Registration failed because the verification code could not be sent. Please try again later."
                };
            }

            return new RegisterResponseDto
            {
                Success = true,
                Message = "Registration successful. Please confirm your email.",
                User = new UserDto
                {
                    Name = registerDto.Name,
                    Email = registerDto.Email,
                    Role = PublicUserRole
                }
                
            };
        }

        public async Task<ResponseDto> VerifyOtpForPasswordResetAsync(VerifyPasswordResetOtpDto verifyPasswordResetOtpDto)
        {
            var user = await _userManager.FindByEmailAsync(verifyPasswordResetOtpDto.Email);
            if (user == null)
            {
                return new ResponseDto
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            var isValidOtp = await _otpService.ValidateOtp(user.Id, OtpPurpose.PasswordReset, verifyPasswordResetOtpDto.OtpCode);
            if (!isValidOtp)
            {
                return new ResponseDto
                {
                    Success = false,
                    Message = "Invalid or expired OTP."
                };
            }

            return new ResponseDto
            {
                Success = true,
                Message = "OTP is valid."
            };
        }

        public async Task<ResetPasswordResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
            { 
                return new ResetPasswordResponseDto 
                { 
                    Success = false, 
                    Message = "User not found." 
                };
            }
            var isValidOtp = await _otpService.VerifyOtp(user.Id, OtpPurpose.PasswordReset, resetPasswordDto.OtpCode);
            if (!isValidOtp) 
            { 
                return new ResetPasswordResponseDto 
                { 
                    Success = false, 
                    Message = "Invalid or expired OTP." 
                };
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, resetPasswordDto.NewPassword);
            if (!result.Succeeded)
            { 
                return new ResetPasswordResponseDto 
                { 
                    Success = false, 
                    Message = string.Join(", ", result.Errors.Select(e => e.Description)) 
                };
            }
            if (user.Status == UserStatus.Inactive)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Worker") || roles.Contains("Authority"))
                {
                    user.Status = UserStatus.Active;
                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    { 
                        return new ResetPasswordResponseDto 
                        { 
                            Success = false, 
                            Message = "Password reset but failed to activate account." 
                        };
                    }
                }
            }

            return new ResetPasswordResponseDto 
            { 
                Success = true, 
                Message = "Password reset successfully." 
            };
        }



        public async Task<LoginResponseDto> RefreshTokenAsync(string token)
        {
            var user = await _refreshTokenRepository.FindUserWithRefreshTokenAsync(token);
            if (user == null)
            { 
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "No user has this refresh token"

                };
            }

            var incomeToken = await _refreshTokenRepository.FindRefreshTokenAsync(token);

            if (incomeToken is null || !incomeToken.IsActive)
            { 
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "This refresh token is not active"

                };
            }

            incomeToken.RevokedOn = DateTime.UtcNow;
            
            var newRefreshToken = GenerateRefreshToken();
            user.RefreshTokens!.Add(newRefreshToken);
            var userRole = await _userManager.GetRolesAsync(user);
            
            await _userManager.UpdateAsync(user);

            var jwtToken = GenerateJwtToken(user, userRole);

            return new LoginResponseDto
            {
                Success = true,
                Message = "New refresh token generated successflly",
                Token = jwtToken,
                RefreshToken = newRefreshToken.Token,
                RefreshTokenExpiresOn = newRefreshToken.ExpiresOn
            };

        }

        public async Task<bool> LogoutAllAsync(string userId)
        {
            ICollection<RefreshToken> allRefreshTokens =
                await _refreshTokenRepository.GetAllRefreshTokenByUserIdAsync(userId);

            foreach (RefreshToken refreshToken in allRefreshTokens)
            {
                refreshToken.RevokedOn = DateTime.UtcNow;
            }

            await _refreshTokenRepository.SaveTokenChangesAsync();

            return true;
        }

        private RefreshToken GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            RandomNumberGenerator.Fill(randomBytes);
            var token = Convert.ToBase64String(randomBytes);
            var durationInDays = int.Parse(
                _configuration["JwtSettings:RefreshTokenDurationInDays"]!
                );

            RefreshToken refreshToken = new RefreshToken
            {
                Token = token,
                CreatedOn = DateTime.UtcNow,
                ExpiresOn = DateTime.UtcNow.AddDays(durationInDays)
            };
            
            return refreshToken;
        }

        private string GenerateJwtToken(User user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.Name)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(
                    double.Parse(_configuration["JwtSettings:DurationInDays"]!)),
                signingCredentials: credentials
                );
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        
    }
}
