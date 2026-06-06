using InfraReportingSystem.Shared.DTOs.Shared.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.ServiceAbstractions.Shared.Auth { 

    public interface IAuthService
    {
        Task<RegisterResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<LoginResponseDto> LoginAsync(LoginDto loginDto);
        Task<bool> ResendConfirmationEmailAsync(string email);
        Task ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
        Task<ResponseDto> VerifyOtpForPasswordResetAsync(VerifyPasswordResetOtpDto verifyPasswordResetOtpDto);
        Task<ResetPasswordResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task<bool> ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto);
        Task<LoginResponseDto> RefreshTokenAsync(string token);
        Task<bool> LogoutAllAsync(string userId);

    }
}
