using CloudinaryDotNet.Actions;
using FluentAssertions;
using InfraReportingSystem.ServiceAbstractions.Shared.Images;
using InfraReportingSystem.Services.Shared.Images;
using Moq;
using Xunit;

namespace InfrastructureReportingSystem.Tests.Unit.Shared.Images;

public class ImageServiceTests
{
    private readonly Mock<ICloudinaryClient> _cloudinaryClientMock = new();

    [Fact]
    public async Task UploadImageAsync_WhenStreamIsNull_ReturnsNull()
    {
        var service = CreateService();

        var result = await service.UploadImageAsync(null!, "test.jpg", "folder");

        result.Should().BeNull();
    }

    [Fact]
    public async Task UploadImageAsync_WhenStreamIsEmpty_ReturnsNull()
    {
        using var emptyStream = new MemoryStream();

        var service = CreateService();

        var result = await service.UploadImageAsync(emptyStream, "test.jpg", "folder");

        result.Should().BeNull();
    }

    [Fact]
    public async Task UploadImageAsync_WhenCloudinaryReturnsError_ThrowsException()
    {
        using var stream = new MemoryStream([1, 2, 3]);
        var uploadResult = new ImageUploadResult
        {
            Error = new Error { Message = "Invalid file format." }
        };

        _cloudinaryClientMock
            .Setup(c => c.UploadImageAsync(It.IsAny<ImageUploadParams>()))
            .ReturnsAsync(uploadResult);

        var service = CreateService();

        Func<Task> action = async () => await service.UploadImageAsync(stream, "test.jpg", "folder");

        await action.Should().ThrowAsync<Exception>()
            .WithMessage("Cloudinary error: Invalid file format.");
    }

    [Fact]
    public async Task UploadImageAsync_WhenCloudinaryReturnsNullSecureUrl_ReturnsNull()
    {
        using var stream = new MemoryStream([1, 2, 3]);
        var uploadResult = new ImageUploadResult
        {
            SecureUrl = null
        };

        _cloudinaryClientMock
            .Setup(c => c.UploadImageAsync(It.IsAny<ImageUploadParams>()))
            .ReturnsAsync(uploadResult);

        var service = CreateService();

        var result = await service.UploadImageAsync(stream, "test.jpg", "folder");

        result.Should().BeNull();
    }

    [Fact]
    public async Task UploadImageAsync_WhenSuccessful_ReturnsSecureUrlString()
    {
        using var stream = new MemoryStream([1, 2, 3]);
        var uploadResult = new ImageUploadResult
        {
            SecureUrl = new Uri("https://res.cloudinary.com/test/image/upload/v1/test.jpg")
        };

        _cloudinaryClientMock
            .Setup(c => c.UploadImageAsync(It.IsAny<ImageUploadParams>()))
            .ReturnsAsync(uploadResult);

        var service = CreateService();

        var result = await service.UploadImageAsync(stream, "test.jpg", "folder");

        result.Should().Be("https://res.cloudinary.com/test/image/upload/v1/test.jpg");
    }

    private ImageService CreateService()
    {
        return new ImageService(_cloudinaryClientMock.Object);
    }
}
