using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Persistence.Data;
using InfraReportingSystem.ServiceAbstractions.Repositories.Shared.Auth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Persistence.Repositories.Shared.Auth
{

    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _context;
        public RefreshTokenRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<RefreshToken?> FindRefreshTokenAsync(string refreshToken)
        {
            return await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken);

        }

        public async Task<User?> FindUserWithRefreshTokenAsync(string refreshToken)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens!.Any(r => r.Token == refreshToken));
        }

        public async Task<ICollection<RefreshToken>> GetAllRefreshTokenByUserIdAsync(string userId)
        {
            List < RefreshToken > tokens = await _context.RefreshTokens
                .Where(r => r.UserId == userId).ToListAsync();

            return tokens;
        }

        public async Task SaveTokenChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}