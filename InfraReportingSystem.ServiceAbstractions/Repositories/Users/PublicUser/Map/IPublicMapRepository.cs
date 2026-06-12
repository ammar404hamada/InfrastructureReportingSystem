using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;

namespace InfraReportingSystem.ServiceAbstractions.Repositories.Users.PublicUser.Map
{
    public interface IPublicMapRepository
    {
        Task<IEnumerable<Report>> GetMapReportsAsync(
            int? categoryId,
            ReportStatus? status);
    }
}
