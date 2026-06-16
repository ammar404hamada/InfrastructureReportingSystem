using InfraReportingSystem.Shared.DTOs.Users.PublicUser.SubmitReport;

namespace InfraReportingSystem.ServiceAbstractions.Users.PublicUser.SubmitReport
{
    public interface IPublicSubmitReportService
    {
        Task<AISuggestionDto> AnalyzeImagesAsync(IEnumerable<(Stream Stream, string FileName)> images);
        Task<SubmitReportResponseDto> SubmitReportAsync(SubmitReportRequestDto request, IEnumerable<(Stream Stream, string FileName)> images, string userId);
    }
}
