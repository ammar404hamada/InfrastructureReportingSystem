using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.PublicUser.MarkerPopup;
using InfraReportingSystem.ServiceAbstractions.Users.PublicUser.MarkerPopup;
using InfraReportingSystem.Shared.DTOs.UserServices.PublicUser.MarkerPopup;

namespace InfraReportingSystem.Services.Users.PublicUser.MarkerPopup
{
    public class PublicMarkerPopupService : IPublicMarkerPopupService
    {
        private readonly IPublicMarkerPopupRepository _repository;

        public PublicMarkerPopupService(IPublicMarkerPopupRepository repository)
        {
            _repository = repository;
        }

        public async Task<MarkerPopupDetailsDto?> GetReportDetailsAsync(int reportId)
        {
            if (reportId <= 0)
                return null;

            var report = await _repository.GetReportAsync(reportId);
            if (report is null)
                return null;

            return new MarkerPopupDetailsDto
            {
                ReportId = report.Id,
                Title = BuildTitle(report.Description),
                Description = report.Description,
                Address = null,
                Latitude = report.Latitude,
                Longitude = report.Longitude,
                CategoryName = report.Category?.Name ?? string.Empty,
                Status = report.Status.ToString(),
                CreatedAt = report.UploadedAt,
                UpdatedAt = report.UpdatedAt,

                Photos = report.ReportPics.Select(p => new MarkerPopupPhotoDto
                {
                    ImageUrl = p.PicUrl
                }),

                ReportedBy = new MarkerPopupUserDto
                {
                    UserId = report.SubmittedBy?.Id ?? string.Empty,
                    FullName = report.SubmittedBy?.Name ?? "Unknown"
                },

                AssignedWorker = report.AssignedWorkerId != null
                    ? new MarkerPopupUserDto
                    {
                        UserId = report.AssignedWorker!.Id,
                        FullName = report.AssignedWorker!.Name
                    }
                    : null
            };
        }

        private static string BuildTitle(string? description)
        {
            if (string.IsNullOrWhiteSpace(description)) return string.Empty;
            return description.Length <= 80 ? description : description[..80] + "…";
        }
    }
}
