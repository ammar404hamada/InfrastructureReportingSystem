using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.Persistence.Data;
using InfraReportingSystem.ServiceAbstractions.Repositories.Auth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace InfraReportingSystem.Persistence.Repositories.Auth
{
    public class OtpRepository : IOtpRepository
    {
        private readonly AppDbContext _context;
        
        public OtpRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<OtpVerification?> FindOtp(string userId, OtpPurpose purpose)
        {
            var otp = await _context.OtpVerifications.FirstOrDefaultAsync(o =>
                        o.UserId == userId &&
                        o.Purpose == purpose &&
                        o.IsUsed == false &&
                        
                        o.ExpiresAt > DateTime.UtcNow);

            return otp;
        }

        public async Task Invalidate(string userId, OtpPurpose purpose)
        {
            List<OtpVerification> allUserOtps = await _context.OtpVerifications
                .Where(o => o.UserId == userId && o.Purpose == purpose)
                .ToListAsync();

            foreach (var otp in allUserOtps)
            {
                    otp.IsUsed = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task SaveOtp(OtpVerification otpVerification)
        {
            await _context.AddAsync(otpVerification);
            await _context.SaveChangesAsync();
        }
    }

}

