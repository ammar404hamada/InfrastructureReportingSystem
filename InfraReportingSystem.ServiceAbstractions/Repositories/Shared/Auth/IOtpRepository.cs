using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.ServiceAbstractions.Repositories.Shared.Auth
{
    public interface IOtpRepository
    {
        Task SaveOtp(OtpVerification otpVerification);
        Task<OtpVerification?> FindOtp(string userId, OtpPurpose purpose);
        Task Invalidate(string userId, OtpPurpose purpose);
    }
}