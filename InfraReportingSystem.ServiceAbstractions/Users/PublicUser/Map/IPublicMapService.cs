using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.Shared.DTOs.UserServices.PublicUser.Map;

namespace InfraReportingSystem.ServiceAbstractions.Users.PublicUser.Map
{
    public interface IPublicMapService
    {
        Task<IEnumerable<MapReportDto>> GetMapReportsAsync(
            int? categoryId,
            ReportStatus? status);
    }
}
