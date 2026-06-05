using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Shared.Auth;
using InfraReportingSystem.ServiceAbstractions.Shared.Auth;
using InfraReportingSystem.ServiceAbstractions.Shared.Email;
using InfrastructureReportingSystem.Shared.EmailTemplate;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Services.Shared.Auth
{
    public class OtpService : IOtpService
    {
        private readonly IOtpRepository _repository;
        private readonly IEmailService _emailService;
        private readonly UserManager<User> _userManager;

        public OtpService(
            IOtpRepository otpRepository,
            IEmailService emailService,
            UserManager<User> userManager
            )
        {
            _repository = otpRepository;
            _emailService = emailService;
            _userManager = userManager;
        }

        public async Task GenerateOtp(string userId, OtpPurpose otpPurpose)
        {
            await _repository.Invalidate(userId, otpPurpose);

            var otpCode = GenerateCode();

            OtpVerification otp = new OtpVerification
            {
                OtpCode = otpCode,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false,
                Purpose = otpPurpose,
                UserId = userId
            };

            await _repository.SaveOtp(otp);
            await SendOtp(otp);
        }

        private async Task SendOtp(OtpVerification otp)
        {
            var user = await _userManager.FindByIdAsync(otp.UserId);
            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                throw new InvalidOperationException("Cannot send OTP because the user or user email was not found.");

            var emailBody = EmailTemplates.OtpEmailTemplate(
                user.Name,
                otp.OtpCode,
                user.Email
                );

            await _emailService.SendEmailAsync(
                user.Email,
                $"Your OTP code for {otp.Purpose.ToString()}",
                emailBody
                );
            
        }

        public async Task<bool> VerifyOtp(string userId, OtpPurpose otpPurpose, string otpCode)
        {
            if (string.IsNullOrWhiteSpace(otpCode)) return false;

            OtpVerification? otp = await _repository.FindOtp(userId, otpPurpose);
            if (otp == null) return false;

            if (otp.OtpCode != otpCode.Trim()) return false;

            otp.IsUsed = true;
            {
                await _repository.Invalidate(userId, otpPurpose);
                return true;
            }
        }

        private string GenerateCode()
        {
            int code = RandomNumberGenerator.GetInt32(100000, 1000000);
            return code.ToString();
        }
    }

}

