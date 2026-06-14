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
using Microsoft.AspNetCore.Http;

namespace InfraReportingSystem.Services.Users.PublicUser.SubmitReport
{
    public class PublicSubmitReportService : IPublicSubmitReportService
    {
        private readonly IPublicSubmitReportRepository _repository;
        private readonly IImageService _imageService;
        private readonly ICategoryAIClient _categoryAIClient;
        private readonly IDescriptionAIClient _descriptionAIClient;

        private const long MaxFileSize = 5 * 1024 * 1024;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };

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

            var reportPic = new ReportPic
            {
                PicUrl = cloudinaryUrl,
                Report = report
            };
            await _repository.AddReportPicAsync(reportPic);

           
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

            await _repository.SaveChangesAsync();

            return new SubmitReportResponseDto
            {
                ReportId = report.Id,
                Message = "Report submitted successfully.",
                Status = report.Status.ToString()
            };
        }
    }
}
