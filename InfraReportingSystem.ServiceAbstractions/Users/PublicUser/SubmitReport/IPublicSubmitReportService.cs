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
        Task<CategorySuggestionDto> GenerateCategoryAsync(Stream imageStream, string fileName);
        Task<DescriptionSuggestionDto> GenerateDescriptionAsync(Stream imageStream, string fileName);
        Task<SubmitReportResponseDto> SubmitReportAsync(SubmitReportRequestDto request, Stream imageStream, string fileName, string userId);
    }
}
