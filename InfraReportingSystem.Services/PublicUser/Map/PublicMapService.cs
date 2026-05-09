using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.PublicUser.Map;
using InfraReportingSystem.ServiceAbstractions.Repositories.PublicUser.Map;
using InfraReportingSystem.Shared.DTOs.PublicUser.Map;

namespace InfraReportingSystem.Services.PublicUser.Map
{
    public class PublicMapService : IPublicMapService
    {
        private readonly IPublicMapRepository _repository;

        public PublicMapService(IPublicMapRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<MapReportDto>> GetMapReportsAsync(
            int? categoryId,
            ReportStatus? status)
        {
            var reports = await _repository.GetMapReportsAsync(categoryId, status);

            return reports.Select(r => new MapReportDto
            {
                ReportId = r.Id,
                Latitude = r.Latitude,
                Longitude = r.Longitude,
                CategoryName = r.Category?.Name ?? string.Empty,
                Status = r.Status.ToString()
            });
        }
    }
}
