using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Authority.MarkerPopup;
using InfraReportingSystem.ServiceAbstractions.Users.Authority.MarkerPopup;
using InfraReportingSystem.Shared.DTOs.UserServices.Authority.MarkerPopup;

namespace InfraReportingSystem.Services.Users.Authority.MarkerPopup
{
    public class AuthorityMarkerPopupService : IAuthorityMarkerPopupService
    {
        private readonly IAuthorityMarkerPopupRepository _repository;

        public AuthorityMarkerPopupService(IAuthorityMarkerPopupRepository repository)
        {
            _repository = repository;
        }

        public async Task<AuthorityMarkerPopupDto?> GetReportPopupAsync(int reportId)
        {
            if (reportId <= 0)
                return null;

            var report = await _repository.GetReportAsync(reportId);
            if (report is null)
                return null;

            return MapToDto(report);
        }

        private static AuthorityMarkerPopupDto MapToDto(Report report)
        {
            return new AuthorityMarkerPopupDto
            {
                ReportId = report.Id,
                Status = report.Status.ToString(),
                CategoryName = report.Category?.Name ?? string.Empty,
                CreatedAt = report.UploadedAt,
                Photos = report.ReportPics.Select(p => p.PicUrl).ToList(),
                Description = report.Description,
                Latitude = report.Latitude,
                Longitude = report.Longitude,
                MapUrl = $"https://www.google.com/maps?q={report.Latitude},{report.Longitude}",
                ReporterName = report.SubmittedBy?.Name ?? string.Empty,
            };
        }
    }
}
