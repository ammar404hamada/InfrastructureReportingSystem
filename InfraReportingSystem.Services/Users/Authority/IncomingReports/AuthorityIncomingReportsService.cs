using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Authority.IncomingReports;
using InfraReportingSystem.ServiceAbstractions.Users.Authority.IncomingReports;
using InfraReportingSystem.Shared.DTOs.Common;
using InfraReportingSystem.Shared.DTOs.UserServices.Authority.IncomingReports;
using Microsoft.Extensions.Logging;

namespace InfraReportingSystem.Services.Users.Authority.IncomingReports
{
    public class AuthorityIncomingReportsService : IAuthorityIncomingReportsService
    {
        private readonly IAuthorityIncomingReportsRepository _repository;
        private readonly ILogger<AuthorityIncomingReportsService> _logger;

        private static readonly HashSet<string> AllowedSortFields =
            new(StringComparer.OrdinalIgnoreCase) { "title", "updatedat", "reporter" };

        private const int PhotoPreviewLimit = 3;

        public AuthorityIncomingReportsService(
            IAuthorityIncomingReportsRepository repository,
            ILogger<AuthorityIncomingReportsService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<PaginatedResult<IncomingReportDto>> GetIncomingReportsAsync(
            string? search,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize)
        {
            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);

           
            if (!string.IsNullOrWhiteSpace(sortBy) && !AllowedSortFields.Contains(sortBy))
            {
                _logger.LogWarning("Unknown sortBy value '{SortBy}' — falling back to default.", sortBy);
                sortBy = null;
            }

      
            if (!string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase))
            {
                sortDirection = "desc";
            }

            var (reports, totalCount) = await _repository.GetIncomingReportsAsync(
                search, sortBy, sortDirection, pageNumber, pageSize);

            if (totalCount == 0)
            {
                _logger.LogInformation("No incoming reports found. Search: '{Search}'", search ?? "none");
                return new PaginatedResult<IncomingReportDto>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Items = new List<IncomingReportDto>(),
                    Message = "No incoming reports found. Try adjusting your search or filters."
                };
            }

            var items = reports.Select(MapToDto).ToList();

            return new PaginatedResult<IncomingReportDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = items,
                Message = string.Empty
            };
        }

        private static IncomingReportDto MapToDto(Report report)
        {
            var photos = report.ReportPics
                .Select(p => p.PicUrl)
                .ToList();

            return new IncomingReportDto
            {
                ReportId = report.Id,
                Title = report.Category?.Name ?? string.Empty,
                Description = report.Description,
                Location = BuildLocation(report.Latitude, report.Longitude),
                UpdatedAt = report.UpdatedAt ?? report.UploadedAt,
                Status = report.Status.ToString(),
                ReporterName = report.SubmittedBy?.Name ?? string.Empty,
                ReporterEmail = report.SubmittedBy?.Email ?? string.Empty,
                ReporterImage = report.SubmittedBy?.ProfilePictureUrl ?? string.Empty,
                PhotosCount = photos.Count,
                PhotosPreview = photos.Take(PhotoPreviewLimit).ToList()
            };
        }

        private static string BuildLocation(double latitude, double longitude)
            => $"{latitude:F4}, {longitude:F4}";
    }
}
