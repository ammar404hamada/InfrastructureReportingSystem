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
<<<<<<< HEAD
        Task<CategorySuggestionDto> GenerateCategoryAsync(Stream imageStream, string fileName);
        Task<DescriptionSuggestionDto> GenerateDescriptionAsync(Stream imageStream, string fileName);
        Task<SubmitReportResponseDto> SubmitReportAsync(SubmitReportRequestDto request, Stream imageStream, string fileName, string userId);
=======
        Task<CategorySuggestionDto> GenerateCategoryAsync(IFormFile image);
        Task<DescriptionSuggestionDto> GenerateDescriptionAsync(IFormFile image);
        Task<SubmitReportResponseDto> SubmitReportAsync(SubmitReportRequestDto request, string userId);
>>>>>>> 63ae425cacb8f085d875228d89a74a932d83f2df
    }
}
