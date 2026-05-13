using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Authority.Map;
using InfraReportingSystem.ServiceAbstractions.Repositories.Authority.Map;
using InfraReportingSystem.Shared.DTOs.Authority.Map;

namespace InfraReportingSystem.Services.Authority.Map
{
    public class AuthorityMapService : IAuthorityMapService
    {
        private readonly IAuthorityMapRepository _repository;

        public AuthorityMapService(IAuthorityMapRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<AuthorityMapReportDto>> GetMapReportsAsync(
            int? categoryId,
            ReportStatus? status)
        {
            var reports = await _repository.GetMapReportsAsync(categoryId, status);
            return reports.Select(MapToDto);
        }

        private static AuthorityMapReportDto MapToDto(Report report)
        {
            return new AuthorityMapReportDto
            {
                ReportId = report.Id,
                Latitude = report.Latitude,
                Longitude = report.Longitude,
                CategoryName = report.Category?.Name ?? string.Empty,
                Status = report.Status.ToString()
            };
        }
    }
}
