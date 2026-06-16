using FluentAssertions;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Shared;
using InfraReportingSystem.ServiceAbstractions.Repositories.Shared.Profile.ProfileManagement;
using InfraReportingSystem.ServiceAbstractions.Shared.Images;
using InfraReportingSystem.Shared.DTOs.Shared.Profile.ProfileManagement;
using Moq;
using Xunit;

namespace InfrastructureReportingSystem.Tests.Unit.Shared.Profile.ProfileManagement;

public class ProfileManagementServiceTests
{
    private readonly Mock<IProfileManagementRepository> _repositoryMock = new();
    private readonly Mock<IImageService> _imageServiceMock = new();
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock = new();

    [Fact]
    public async Task UpdateProfileInfoAsync_WhenUserNotFound_ReturnsFailure()
    {
        _repositoryMock
            .Setup(r => r.GetUserByIdAsync("missing-id"))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        var result = await service.UpdateProfileInfoAsync("missing-id", new UpdateProfileInfoDto());

        result.Success.Should().BeFalse();
        result.Message.Should().Be("User not found.");
    }

    [Fact]
    public async Task UpdateProfileInfoAsync_WhenNameChanged_UpdatesAndAudits()
    {
        var user = CreateUser("user-1", name: "OldName", phone: "01000000000");
        var dto = new UpdateProfileInfoDto { Name = "NewName", PhoneNumber = "01000000000" };

        _repositoryMock.Setup(r => r.GetUserByIdAsync("user-1")).ReturnsAsync(user);
        _repositoryMock.Setup(r => r.UpdateUserAsync(It.IsAny<User>())).ReturnsAsync(true);

        var service = CreateService();

        var result = await service.UpdateProfileInfoAsync("user-1", dto);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Profile updated successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("NewName");
        user.Name.Should().Be("NewName");
        user.PhoneNumber.Should().Be("01000000000");

        _repositoryMock.Verify(r => r.UpdateUserAsync(user), Times.Once);
        _auditLogRepositoryMock.Verify(r => r.AddAsync(It.Is<AuditLog>(a =>
            a.UserId == "user-1" &&
            a.ActionType == AuditActionType.AccountUpdated &&
            a.EntityName == "User" &&
            a.EntityId == "user-1" &&
            a.Details == "User updated their profile information.")), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileInfoAsync_WhenPhoneChanged_UpdatesAndAudits()
    {
        var user = CreateUser("user-1", name: "SameName", phone: "01000000000");
        var dto = new UpdateProfileInfoDto { Name = "SameName", PhoneNumber = "01011111111" };

        _repositoryMock.Setup(r => r.GetUserByIdAsync("user-1")).ReturnsAsync(user);
        _repositoryMock.Setup(r => r.UpdateUserAsync(It.IsAny<User>())).ReturnsAsync(true);

        var service = CreateService();

        var result = await service.UpdateProfileInfoAsync("user-1", dto);

        result.Success.Should().BeTrue();
        user.PhoneNumber.Should().Be("01011111111");
        _repositoryMock.Verify(r => r.UpdateUserAsync(user), Times.Once);
        _auditLogRepositoryMock.Verify(r => r.AddAsync(It.IsAny<AuditLog>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileInfoAsync_WhenNoChanges_ReturnsSuccessWithoutCallingUpdate()
    {
        var user = CreateUser("user-1", name: "SameName", phone: "01000000000");
        var dto = new UpdateProfileInfoDto { Name = "SameName", PhoneNumber = "01000000000" };

        _repositoryMock.Setup(r => r.GetUserByIdAsync("user-1")).ReturnsAsync(user);

        var service = CreateService();

        var result = await service.UpdateProfileInfoAsync("user-1", dto);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Profile updated successfully.");
        _repositoryMock.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        _auditLogRepositoryMock.Verify(r => r.AddAsync(It.IsAny<AuditLog>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProfileInfoAsync_WhenDbUpdateFails_ReturnsFailure()
    {
        var user = CreateUser("user-1", name: "OldName", phone: "01000000000");
        var dto = new UpdateProfileInfoDto { Name = "NewName" };

        _repositoryMock.Setup(r => r.GetUserByIdAsync("user-1")).ReturnsAsync(user);
        _repositoryMock.Setup(r => r.UpdateUserAsync(It.IsAny<User>())).ReturnsAsync(false);

        var service = CreateService();

        var result = await service.UpdateProfileInfoAsync("user-1", dto);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Failed to update profile info.");
        _auditLogRepositoryMock.Verify(r => r.AddAsync(It.IsAny<AuditLog>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProfilePhotoAsync_WhenUserNotFound_ReturnsFailure()
    {
        _repositoryMock
            .Setup(r => r.GetUserByIdAsync("missing-id"))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        using var stream = new MemoryStream([1, 2, 3]);
        var result = await service.UpdateProfilePhotoAsync("missing-id", stream, "photo.jpg");

        result.Success.Should().BeFalse();
        result.Message.Should().Be("User not found.");
        _imageServiceMock.Verify(s => s.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProfilePhotoAsync_WhenUploadReturnsNull_ReturnsFailure()
    {
        var user = CreateUser("user-1");
        _repositoryMock.Setup(r => r.GetUserByIdAsync("user-1")).ReturnsAsync(user);
        _imageServiceMock
            .Setup(s => s.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), "profiles/user-1"))
            .ReturnsAsync((string?)null);

        var service = CreateService();

        using var stream = new MemoryStream([1, 2, 3]);
        var result = await service.UpdateProfilePhotoAsync("user-1", stream, "photo.jpg");

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Failed to upload the profile picture.");
        _repositoryMock.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProfilePhotoAsync_WhenDbUpdateFails_ReturnsFailure()
    {
        var user = CreateUser("user-1");
        _repositoryMock.Setup(r => r.GetUserByIdAsync("user-1")).ReturnsAsync(user);
        _imageServiceMock
            .Setup(s => s.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), "profiles/user-1"))
            .ReturnsAsync("https://cloudinary.com/photo.jpg");
        _repositoryMock.Setup(r => r.UpdateUserAsync(It.IsAny<User>())).ReturnsAsync(false);

        var service = CreateService();

        using var stream = new MemoryStream([1, 2, 3]);
        var result = await service.UpdateProfilePhotoAsync("user-1", stream, "photo.jpg");

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Failed to update user's profile picture in the database.");
        _auditLogRepositoryMock.Verify(r => r.AddAsync(It.IsAny<AuditLog>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProfilePhotoAsync_WhenValid_UploadsPhotoAndUpdatesAudits()
    {
        var user = CreateUser("user-1", email: "user@test.com");
        const string photoUrl = "https://cloudinary.com/profile.jpg";

        _repositoryMock.Setup(r => r.GetUserByIdAsync("user-1")).ReturnsAsync(user);
        _imageServiceMock
            .Setup(s => s.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), "profiles/user-1"))
            .ReturnsAsync(photoUrl);
        _repositoryMock.Setup(r => r.UpdateUserAsync(It.IsAny<User>())).ReturnsAsync(true);

        var service = CreateService();

        using var stream = new MemoryStream([1, 2, 3]);
        var result = await service.UpdateProfilePhotoAsync("user-1", stream, "photo.jpg");

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Profile picture updated successfully.");
        result.Data.Should().NotBeNull();
        result.Data!.PhotoUrl.Should().Be(photoUrl);
        result.Data.Email.Should().Be("user@test.com");
        user.ProfilePictureUrl.Should().Be(photoUrl);

        _repositoryMock.Verify(r => r.UpdateUserAsync(user), Times.Once);
        _auditLogRepositoryMock.Verify(r => r.AddAsync(It.Is<AuditLog>(a =>
            a.UserId == "user-1" &&
            a.ActionType == AuditActionType.AccountUpdated &&
            a.EntityName == "User" &&
            a.EntityId == "user-1" &&
            a.Details == "User updated their profile picture.")), Times.Once);
    }

    private InfraReportingSystem.Services.Shared.Profile.ProfileManagement.ProfileManagementService CreateService()
    {
        return new InfraReportingSystem.Services.Shared.Profile.ProfileManagement.ProfileManagementService(
            _repositoryMock.Object,
            _imageServiceMock.Object,
            _auditLogRepositoryMock.Object);
    }

    private static User CreateUser(string id, string? name = null, string? phone = null, string? email = null)
    {
        return new User
        {
            Id = id,
            Name = name ?? "Test User",
            UserName = email ?? "test@example.com",
            Email = email ?? "test@example.com",
            PhoneNumber = phone,
            ProfilePictureUrl = null,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
    }
}
