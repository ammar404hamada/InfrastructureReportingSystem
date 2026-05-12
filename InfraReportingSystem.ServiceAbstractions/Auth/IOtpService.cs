using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.ServiceAbstractions.Auth
{ 
    public interface IOtpService
    {
        Task GenerateOtp(string userId, OtpPurpose purpose);
        Task<bool> VerifyOtp(string userId, OtpPurpose purpose, string otpCode);
    }
}