using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;

namespace InfraReportingSystem.ServiceAbstractions.Repositories.PublicUser.AlsoSuffer
{
    public interface IPublicAlsoSufferRepository
    {
        Task<ReportStatus?> GetReportStatusAsync(int reportId);
        Task<bool> AlreadyAffectedAsync(int reportId, string userId);
        Task AddAsync(ReportAffectedUser entity);
        Task SaveChangesAsync();
    }
}
