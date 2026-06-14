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
<<<<<<< HEAD
=======
using Microsoft.AspNetCore.Http;
>>>>>>> 63ae425cacb8f085d875228d89a74a932d83f2df

namespace InfraReportingSystem.Services.Users.PublicUser.SubmitReport
{
    public class PublicSubmitReportService : IPublicSubmitReportService
    {
        private readonly IPublicSubmitReportRepository _repository;
        private readonly IImageService _imageService;
        private readonly ICategoryAIClient _categoryAIClient;
        private readonly IDescriptionAIClient _descriptionAIClient;

<<<<<<< HEAD
=======
        private const long MaxFileSize = 5 * 1024 * 1024;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };

>>>>>>> 63ae425cacb8f085d875228d89a74a932d83f2df
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

<<<<<<< HEAD
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
=======
        private static void ValidateImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
                throw new ArgumentException("Image file is required.");

            if (image.Length > MaxFileSize)
                throw new ArgumentException($"Image size exceeds {MaxFileSize / (1024 * 1024)} MB limit.");

            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                throw new ArgumentException($"Only {string.Join(", ", AllowedExtensions)} images are allowed.");
        }

        public async Task<CategorySuggestionDto> GenerateCategoryAsync(IFormFile image)
        {
            ValidateImage(image);
            await using var stream = image.OpenReadStream();
            var suggestedCategory = await _categoryAIClient.SuggestCategoryAsync(stream, image.FileName, image.ContentType);
            return new CategorySuggestionDto { SuggestedCategory = suggestedCategory };
        }

        public async Task<DescriptionSuggestionDto> GenerateDescriptionAsync(IFormFile image)
        {
            ValidateImage(image);
            await using var stream = image.OpenReadStream();
            var generatedDescription = await _descriptionAIClient.GenerateDescriptionAsync(stream, image.FileName, image.ContentType);
            return new DescriptionSuggestionDto { SuggestedDescription = generatedDescription };
        }

        public async Task<SubmitReportResponseDto> SubmitReportAsync(SubmitReportRequestDto request, string userId)
        {
            ValidateImage(request.Image);

            var category = await _repository.GetCategoryByIdAsync(request.CategoryId);
            if (category is null)
                throw new KeyNotFoundException($"Category with ID {request.CategoryId} was not found.");

            await using var imageStream = request.Image.OpenReadStream();
            var cloudinaryUrl = await _imageService.UploadImageAsync(
                imageStream,
                request.Image.FileName,
                folderPath: "reports");

            if (string.IsNullOrWhiteSpace(cloudinaryUrl))
                throw new InvalidOperationException("Image upload failed. Please try again.");

>>>>>>> 63ae425cacb8f085d875228d89a74a932d83f2df
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

<<<<<<< HEAD
            // 4. Create ReportPic linked to the report
=======
>>>>>>> 63ae425cacb8f085d875228d89a74a932d83f2df
            var reportPic = new ReportPic
            {
                PicUrl = cloudinaryUrl,
                Report = report
            };
<<<<<<< HEAD

            await _repository.AddReportPicAsync(reportPic);

            // 5. Create AuditLog entry
=======
            await _repository.AddReportPicAsync(reportPic);

           
>>>>>>> 63ae425cacb8f085d875228d89a74a932d83f2df
            var auditLog = new AuditLog
            {
                UserId = userId,
                ActionType = AuditActionType.ReportCreated,
                EntityName = nameof(Report),
                EntityId = report.Id.ToString(),
                Details = $"Report submitted by user {userId}.",
                Timestamp = DateTime.UtcNow
            };
<<<<<<< HEAD

            await _repository.AddAuditLogAsync(auditLog);

            // 6. Persist everything in one transaction
            await _repository.SaveChangesAsync();

            // 7. Return response
=======
            await _repository.AddAuditLogAsync(auditLog);

            await _repository.SaveChangesAsync();

>>>>>>> 63ae425cacb8f085d875228d89a74a932d83f2df
            return new SubmitReportResponseDto
            {
                ReportId = report.Id,
                Message = "Report submitted successfully.",
                Status = report.Status.ToString()
            };
        }
    }
}
