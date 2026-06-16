using FluentAssertions;
using InfraReportingSystem.Domain.AI;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.PublicUser.SubmitReport;
using InfraReportingSystem.ServiceAbstractions.Shared.Images;
using InfraReportingSystem.Shared.DTOs.Users.PublicUser.SubmitReport;
using Moq;
using Xunit;

namespace InfrastructureReportingSystem.Tests.Unit.Users.PublicUser.SubmitReport;

public class PublicSubmitReportServiceTests
{
    private readonly Mock<IPublicSubmitReportRepository> _repositoryMock = new();
    private readonly Mock<IImageService> _imageServiceMock = new();
    private readonly Mock<IAnalyzeImageAIClient> _aiClientMock = new();

    [Fact]
    public async Task AnalyzeImagesAsync_WhenImagesProvided_ReturnsMappedSuggestions()
    {
        using var stream1 = new MemoryStream([1, 2, 3]);
        using var stream2 = new MemoryStream([4, 5, 6]);
        var images = new List<(Stream Stream, string FileName)>
        {
            (stream1, "photo1.jpg"),
            (stream2, "photo2.jpg")
        };

        _aiClientMock
            .Setup(c => c.AnalyzeImageAsync(stream1, "photo1.jpg"))
            .ReturnsAsync(new AnalyzeImageResult { Prediction = "Roads", ImageDescription = "Pothole" });
        _aiClientMock
            .Setup(c => c.AnalyzeImageAsync(stream2, "photo2.jpg"))
            .ReturnsAsync(new AnalyzeImageResult { Prediction = "Lighting", ImageDescription = "Broken lamp" });

        var service = CreateService();

        var result = await service.AnalyzeImagesAsync(images);

        result.Should().HaveCount(2);
        result[0].SuggestedCategory.Should().Be("Roads");
        result[0].SuggestedDescription.Should().Be("Pothole");
        result[1].SuggestedCategory.Should().Be("Lighting");
        result[1].SuggestedDescription.Should().Be("Broken lamp");
    }

    [Fact]
    public async Task AnalyzeImagesAsync_WhenNoImages_ReturnsEmptyList()
    {
        var images = Enumerable.Empty<(Stream Stream, string FileName)>();

        var service = CreateService();

        var result = await service.AnalyzeImagesAsync(images);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SubmitReportAsync_WhenCategoryNotFound_ThrowsKeyNotFoundException()
    {
        var request = new SubmitReportRequestDto
        {
            CategoryId = 999,
            Description = "Test",
            Latitude = 30.0,
            Longitude = 31.0
        };
        var images = Enumerable.Empty<(Stream Stream, string FileName)>();

        _repositoryMock
            .Setup(r => r.GetCategoryByIdAsync(999))
            .ReturnsAsync((Category?)null);

        var service = CreateService();

        Func<Task> action = async () => await service.SubmitReportAsync(request, images, "user-1");

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Category with ID 999 was not found.");
    }

    [Fact]
    public async Task SubmitReportAsync_WhenImageUploadReturnsNull_ThrowsInvalidOperationException()
    {
        var category = new Category { Id = 1, Name = "Roads" };
        var request = new SubmitReportRequestDto
        {
            CategoryId = 1,
            Description = "Pothole",
            Latitude = 30.0,
            Longitude = 31.0
        };
        using var stream = new MemoryStream([1, 2, 3]);
        var images = new List<(Stream Stream, string FileName)> { (stream, "pic.jpg") };

        _repositoryMock.Setup(r => r.GetCategoryByIdAsync(1)).ReturnsAsync(category);
        _repositoryMock.Setup(r => r.AddReportAsync(It.IsAny<Report>())).Returns(Task.CompletedTask);
        _imageServiceMock
            .Setup(s => s.UploadImageAsync(stream, "pic.jpg", "reports"))
            .ReturnsAsync((string?)null);

        var service = CreateService();

        Func<Task> action = async () => await service.SubmitReportAsync(request, images, "user-1");

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("One or more image uploads failed. Please try again.");
    }

    [Fact]
    public async Task SubmitReportAsync_WhenImageUploadReturnsEmptyString_ThrowsInvalidOperationException()
    {
        var category = new Category { Id = 1, Name = "Roads" };
        var request = new SubmitReportRequestDto
        {
            CategoryId = 1,
            Description = "Pothole",
            Latitude = 30.0,
            Longitude = 31.0
        };
        using var stream = new MemoryStream([1, 2, 3]);
        var images = new List<(Stream Stream, string FileName)> { (stream, "pic.jpg") };

        _repositoryMock.Setup(r => r.GetCategoryByIdAsync(1)).ReturnsAsync(category);
        _repositoryMock.Setup(r => r.AddReportAsync(It.IsAny<Report>())).Returns(Task.CompletedTask);
        _imageServiceMock
            .Setup(s => s.UploadImageAsync(stream, "pic.jpg", "reports"))
            .ReturnsAsync("   ");

        var service = CreateService();

        Func<Task> action = async () => await service.SubmitReportAsync(request, images, "user-1");

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("One or more image uploads failed. Please try again.");
    }

    [Fact]
    public async Task SubmitReportAsync_WhenAllValid_CreatesReportUploadsImagesAuditsAndReturnsSuccess()
    {
        var category = new Category { Id = 1, Name = "Roads" };
        var request = new SubmitReportRequestDto
        {
            CategoryId = 1,
            Description = "Pothole on main road",
            Latitude = 30.0777,
            Longitude = 31.2888
        };
        using var stream = new MemoryStream([1, 2, 3]);
        var images = new List<(Stream Stream, string FileName)> { (stream, "pic.jpg") };
        Report? capturedReport = null;

        _repositoryMock.Setup(r => r.GetCategoryByIdAsync(1)).ReturnsAsync(category);
        _repositoryMock
            .Setup(r => r.AddReportAsync(It.IsAny<Report>()))
            .Callback<Report>(r =>
            {
                capturedReport = r;
                r.GetType().GetProperty("Id")?.SetValue(r, 42);
            })
            .Returns(Task.CompletedTask);
        _imageServiceMock
            .Setup(s => s.UploadImageAsync(stream, "pic.jpg", "reports"))
            .ReturnsAsync("https://cloudinary.com/test.jpg");
        _repositoryMock
            .Setup(r => r.AddReportPicAsync(It.IsAny<ReportPic>()))
            .Returns(Task.CompletedTask);
        _repositoryMock
            .Setup(r => r.AddAuditLogAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);
        _repositoryMock
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var service = CreateService();

        var result = await service.SubmitReportAsync(request, images, "user-1");

        result.Should().NotBeNull();
        result.ReportId.Should().Be(42);
        result.Message.Should().Be("Report submitted successfully.");
        result.Status.Should().Be("Submitted");

        capturedReport.Should().NotBeNull();
        capturedReport!.Description.Should().Be(request.Description);
        capturedReport.Latitude.Should().Be(request.Latitude);
        capturedReport.Longitude.Should().Be(request.Longitude);
        capturedReport.Status.Should().Be(ReportStatus.Submitted);
        capturedReport.CategoryId.Should().Be(1);
        capturedReport.SubmittedById.Should().Be("user-1");

        _repositoryMock.Verify(r => r.AddReportPicAsync(It.Is<ReportPic>(p => p.PicUrl == "https://cloudinary.com/test.jpg")), Times.Once);
        _repositoryMock.Verify(r => r.AddAuditLogAsync(It.Is<AuditLog>(a =>
            a.UserId == "user-1" &&
            a.ActionType == AuditActionType.ReportCreated &&
            a.EntityName == nameof(Report) &&
            a.EntityId == "42")), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    private PublicSubmitReportService CreateService()
    {
        return new PublicSubmitReportService(
            _repositoryMock.Object,
            _imageServiceMock.Object,
            _aiClientMock.Object);
    }
}
