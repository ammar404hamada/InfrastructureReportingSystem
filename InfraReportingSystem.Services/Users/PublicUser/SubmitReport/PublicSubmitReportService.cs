using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.AI;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.PublicUser.SubmitReport;
using InfraReportingSystem.ServiceAbstractions.Shared.Images;
using InfraReportingSystem.ServiceAbstractions.Users.PublicUser.SubmitReport;
using InfraReportingSystem.Shared.DTOs.Users.PublicUser.SubmitReport;

namespace InfraReportingSystem.Services.Users.PublicUser.SubmitReport
{
    public class PublicSubmitReportService : IPublicSubmitReportService
    {
        private readonly IPublicSubmitReportRepository _repository;
        private readonly IImageService _imageService;
        private readonly ICategoryAIClient _categoryAIClient;
        private readonly IDescriptionAIClient _descriptionAIClient;

        public PublicSubmitReportService(
            IPublicSubmitReportRepository repository,
            IImageService imageService,
            ICategoryAIClient categoryAIClient,
            IDescriptionAIClient descriptionAIClient)
        {
            _repository = repository;
            _imageService = imageService;
            _categoryAIClient = categoryAIClient;
            _descriptionAIClient = descriptionAIClient;
        }

        // ─── AI Category Suggestion ─────────────────────────────────────────────

        public async Task<CategorySuggestionDto> GenerateCategoryAsync(
            Stream imageStream, string fileName)
        {
            var suggestedCategory = await _categoryAIClient
                .SuggestCategoryAsync(imageStream, fileName);

            return new CategorySuggestionDto
            {
                SuggestedCategory = suggestedCategory
            };
        }

        // ─── AI Description Generation ──────────────────────────────────────────

        public async Task<DescriptionSuggestionDto> GenerateDescriptionAsync(
            Stream imageStream, string fileName)
        {
            var generatedDescription = await _descriptionAIClient
                .GenerateDescriptionAsync(imageStream, fileName);

            return new DescriptionSuggestionDto
            {
                SuggestedDescription = generatedDescription
            };
        }

        // ─── Final Report Submission ────────────────────────────────────────────

        public async Task<SubmitReportResponseDto> SubmitReportAsync(
            SubmitReportRequestDto request,
            Stream imageStream,
            string fileName,
            string userId)
        {
            // 1. Validate category exists
            var category = await _repository.GetCategoryByIdAsync(request.CategoryId);
            if (category is null)
                throw new KeyNotFoundException(
                    $"Category with ID {request.CategoryId} was not found.");

            // 2. Upload image to Cloudinary — ONLY happens here at submission
            var cloudinaryUrl = await _imageService.UploadImageAsync(
                imageStream,
                fileName,
                folderPath: "reports");

            if (string.IsNullOrWhiteSpace(cloudinaryUrl))
                throw new InvalidOperationException(
                    "Image upload failed. Please try again.");

            // 3. Create Report entity
            // UploadedAt is set by DB default GETUTCDATE() — do not assign it here
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

            // 4. Create ReportPic linked to the report
            var reportPic = new ReportPic
            {
                PicUrl = cloudinaryUrl,
                Report = report
            };

            await _repository.AddReportPicAsync(reportPic);

            // 5. Create AuditLog entry
            var auditLog = new AuditLog
            {
                UserId = userId,
                ActionType = AuditActionType.ReportCreated,
                EntityName = nameof(Report),
                EntityId = report.Id.ToString(),
                Details = $"Report submitted by user {userId}.",
                Timestamp = DateTime.UtcNow
            };

            await _repository.AddAuditLogAsync(auditLog);

            // 6. Persist everything in one transaction
            await _repository.SaveChangesAsync();

            // 7. Return response
            return new SubmitReportResponseDto
            {
                ReportId = report.Id,
                Message = "Report submitted successfully.",
                Status = report.Status.ToString()
            };
        }
    }
}
