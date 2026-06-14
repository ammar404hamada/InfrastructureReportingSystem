using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Shared.DTOs.Users.PublicUser.SubmitReport;

namespace InfraReportingSystem.ServiceAbstractions.Users.PublicUser.SubmitReport
{
    public interface IPublicSubmitReportService
    {
        Task<CategorySuggestionDto> GenerateCategoryAsync(IFormFile image);
        Task<DescriptionSuggestionDto> GenerateDescriptionAsync(IFormFile image);
        Task<SubmitReportResponseDto> SubmitReportAsync(SubmitReportRequestDto request, string userId);
    }
}
