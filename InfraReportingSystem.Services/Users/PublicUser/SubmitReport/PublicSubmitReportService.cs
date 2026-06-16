using InfraReportingSystem.Domain.AI;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.PublicUser.SubmitReport;
using InfraReportingSystem.ServiceAbstractions.Shared.Images;
using InfraReportingSystem.ServiceAbstractions.Users.PublicUser.SubmitReport;
using InfraReportingSystem.Shared.DTOs.Users.PublicUser.SubmitReport;

public class PublicSubmitReportService : IPublicSubmitReportService
{
    private readonly IPublicSubmitReportRepository _repository;
    private readonly IImageService _imageService;
    private readonly IAnalyzeImageAIClient _analyzeImageAIClient;

    public PublicSubmitReportService(
        IPublicSubmitReportRepository repository,
        IImageService imageService,
        IAnalyzeImageAIClient analyzeImageAIClient)
    {
        _repository = repository;
        _imageService = imageService;
        _analyzeImageAIClient = analyzeImageAIClient;
    }

    public async Task<List<AISuggestionDto>> AnalyzeImagesAsync(IEnumerable<(Stream Stream, string FileName)> images)
    {
        var tasks = images.Select(async img => 
        {
            var result = await _analyzeImageAIClient.AnalyzeImageAsync(img.Stream, img.FileName);
            return new AISuggestionDto
            {
                SuggestedCategory = result.Prediction,
                SuggestedDescription = result.ImageDescription
            };
        });

        var results = await Task.WhenAll(tasks);
        return results.ToList();
    }

    public async Task<SubmitReportResponseDto> SubmitReportAsync(
        SubmitReportRequestDto request,
        IEnumerable<(Stream Stream, string FileName)> images,
        string userId)
    {
        // 1. Validate category exists
        var category = await _repository.GetCategoryByIdAsync(request.CategoryId);
        if (category is null)
            throw new KeyNotFoundException(
                $"Category with ID {request.CategoryId} was not found.");

        // 2. Create Report
        var report = new Report
        {
            Description = request.Description,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Status = ReportStatus.Submitted,
            CategoryId = request.CategoryId,
            SubmittedById = userId,
        };

        await _repository.AddReportAsync(report);

        // 3. Upload images to Cloudinary and Create ReportPics
        var uploadTasks = images.Select(img => _imageService.UploadImageAsync(img.Stream, img.FileName, "reports"));
        var cloudinaryUrls = await Task.WhenAll(uploadTasks);

        foreach (var url in cloudinaryUrls)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidOperationException("One or more image uploads failed. Please try again.");

            var reportPic = new ReportPic
            {
                PicUrl = url,
                Report = report
            };

            await _repository.AddReportPicAsync(reportPic);
        }

        // 4. Create AuditLog
        var auditLog = new AuditLog
        {
            UserId = userId,
            ActionType = AuditActionType.ReportCreated,
            EntityName = nameof(Report),
            EntityId = report.Id.ToString(),
            Details = $"Report submitted by user {userId} with {cloudinaryUrls.Length} image(s).",
            Timestamp = DateTime.UtcNow
        };

        await _repository.AddAuditLogAsync(auditLog);

        // 5. Save everything
        await _repository.SaveChangesAsync();

        return new SubmitReportResponseDto
        {
            ReportId = report.Id,
            Message = "Report submitted successfully.",
            Status = report.Status.ToString()
        };
    }
}