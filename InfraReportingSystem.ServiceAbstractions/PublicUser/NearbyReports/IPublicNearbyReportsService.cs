using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Shared.DTOs.PublicUser.NearbyReports;

namespace InfraReportingSystem.ServiceAbstractions.PublicUser.NearbyReports
{
    public interface IPublicNearbyReportsService
    {
        Task<IEnumerable<NearbyReportDto>> GetNearbyReportsAsync(
            double latitude,
            double longitude,
            double radiusInKm = 3);
    }
}
