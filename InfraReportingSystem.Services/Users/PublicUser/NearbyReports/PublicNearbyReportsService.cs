using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.PublicUser.NearbyReports;
using InfraReportingSystem.ServiceAbstractions.Users.PublicUser.NearbyReports;
using InfraReportingSystem.Shared.DTOs.UserServices.PublicUser.NearbyReports;

namespace InfraReportingSystem.Services.Users.PublicUser.NearbyReports
{
    public class PublicNearbyReportsService : IPublicNearbyReportsService
    {
        private const double MaxRadiusKm = 50.0;
        private readonly IPublicNearbyReportsRepository _repository;

        public PublicNearbyReportsService(IPublicNearbyReportsRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<NearbyReportDto>> GetNearbyReportsAsync(
            double latitude,
            double longitude,
            double radiusInKm = 3)
        {
            if (radiusInKm > MaxRadiusKm)
                radiusInKm = MaxRadiusKm;

            return await _repository.GetNearbyReportsAsync(latitude, longitude, radiusInKm);
        }
    }
}
