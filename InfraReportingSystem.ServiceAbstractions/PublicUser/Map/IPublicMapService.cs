using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.Shared.DTOs.PublicUser.Map;

namespace InfraReportingSystem.ServiceAbstractions.PublicUser.Map
{
    public interface IPublicMapService
    {
        Task<IEnumerable<MapReportDto>> GetMapReportsAsync(
            int? categoryId,
            ReportStatus? status);
    }
}
