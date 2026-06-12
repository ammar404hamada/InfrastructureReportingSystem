using InfraReportingSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.ServiceAbstractions.Repositories.Shared.Auth 
{ 

    public interface IRefreshTokenRepository
    {
        Task<User?> FindUserWithRefreshTokenAsync(string refreshToken);
        Task<RefreshToken?> FindRefreshTokenAsync(string refreshToken);
        Task SaveTokenChangesAsync();
        Task<ICollection<RefreshToken>> GetAllRefreshTokenByUserIdAsync(string userId);
    }
}