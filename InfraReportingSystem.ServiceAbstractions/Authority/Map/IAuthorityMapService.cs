using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.Shared.DTOs.Authority.Map;

namespace InfraReportingSystem.ServiceAbstractions.Authority.Map
{
    public interface IAuthorityMapService
    {
        Task<IEnumerable<AuthorityMapReportDto>> GetMapReportsAsync(
            int? categoryId,
            ReportStatus? status);
    }
}
