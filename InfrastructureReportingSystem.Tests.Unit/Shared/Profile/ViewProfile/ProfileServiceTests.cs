using FluentAssertions;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Profile;
using InfraReportingSystem.ServiceAbstractions.Repositories.Shared;
using InfraReportingSystem.ServiceAbstractions.Repositories.Shared.Profile;
using InfraReportingSystem.Shared.DTOs.Shared.Profile.ProfileManagement;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace InfrastructureReportingSystem.Tests.Unit.Shared.Profile.ViewProfile;

public class ProfileServiceTests
{
    private readonly Mock<IProfileRepository> _profileRepositoryMock = new();
    private readonly Mock<UserManager<User>> _userManagerMock = CreateUserManagerMock();
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock = new();

    [Fact]
    public async Task GetProfileAsync_WhenUserNotFound_ReturnsFailureResponse()
    {
        _profileRepositoryMock
            .Setup(r => r.GetUserProfileAsync("missing-id"))
            .ReturnsAsync(((User?)null, Array.Empty<string>() as IList<string>));

        var service = CreateService();

        var result = await service.GetProfileAsync("missing-id");

        result.Success.Should().BeFalse();
        result.Message.Should().Be("User not found.");
        result.Profile.Should().BeNull();
    }

    [Fact]
    public async Task GetProfileAsync_WhenUserIsWorker_IncludesSpecialization()
    {
        var worker = new Worker
        {
            Id = "worker-1",
            Name = "Ahmed",
            Email = "ahmed@test.com",
            PhoneNumber = "01000000000",
            ProfilePictureUrl = "http://example.com/pic.jpg",
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            Specialization = "Plumbing"
        };
        var roles = new[] { "Worker" } as IList<string>;

        _profileRepositoryMock
            .Setup(r => r.GetUserProfileAsync("worker-1"))
            .ReturnsAsync((worker, roles));

        var service = CreateService();

        var result = await service.GetProfileAsync("worker-1");

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Profile retrieved successfully.");
        result.Profile.Should().NotBeNull();
        result.Profile!.Id.Should().Be("worker-1");
        result.Profile.Name.Should().Be("Ahmed");
        result.Profile.Email.Should().Be("ahmed@test.com");
        result.Profile.PhoneNumber.Should().Be("01000000000");
        result.Profile.ProfilePictureUrl.Should().Be("http://example.com/pic.jpg");
        result.Profile.Status.Should().Be("Active");
        result.Profile.CreatedAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(-30), TimeSpan.FromMinutes(1));
        result.Profile.Roles.Should().ContainSingle().Which.Should().Be("Worker");
        result.Profile.Specialization.Should().Be("Plumbing");
    }

    [Fact]
    public async Task GetProfileAsync_WhenUserIsRegularUser_SpecializationIsNull()
    {
        var user = new User
        {
            Id = "user-1",
            Name = "Ali",
            Email = "ali@test.com",
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        var roles = new[] { "PublicUser" } as IList<string>;

        _profileRepositoryMock
            .Setup(r => r.GetUserProfileAsync("user-1"))
            .ReturnsAsync((user, roles));

        var service = CreateService();

        var result = await service.GetProfileAsync("user-1");

        result.Success.Should().BeTrue();
        result.Profile!.Specialization.Should().BeNull();
        result.Profile.Roles.Should().ContainSingle().Which.Should().Be("PublicUser");
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenUserNotFound_ReturnsFailure()
    {
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "oldPass",
            NewPassword = "newPass",
            ConfirmNewPassword = "newPass"
        };

        _userManagerMock
            .Setup(m => m.FindByIdAsync("missing-id"))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        var result = await service.ChangePasswordAsync("missing-id", dto);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("User not found.");
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenCurrentPasswordIsIncorrect_ReturnsFailure()
    {
        var user = CreateUser("user-1");
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "wrongPass",
            NewPassword = "newPass123",
            ConfirmNewPassword = "newPass123"
        };

        _userManagerMock.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(user);
        _userManagerMock
            .Setup(m => m.CheckPasswordAsync(user, dto.CurrentPassword))
            .ReturnsAsync(false);

        var service = CreateService();

        var result = await service.ChangePasswordAsync("user-1", dto);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Current password is incorrect.");
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenNewPasswordSameAsCurrent_ReturnsFailure()
    {
        var user = CreateUser("user-1");
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "samePass",
            NewPassword = "samePass",
            ConfirmNewPassword = "samePass"
        };

        _userManagerMock.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(user);
        _userManagerMock
            .Setup(m => m.CheckPasswordAsync(user, dto.CurrentPassword))
            .ReturnsAsync(true);

        var service = CreateService();

        var result = await service.ChangePasswordAsync("user-1", dto);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("New password must be different from the current password.");
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenChangePasswordFails_ReturnsIdentityErrors()
    {
        var user = CreateUser("user-1");
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "oldPass",
            NewPassword = "newPass123",
            ConfirmNewPassword = "newPass123"
        };
        var identityError = new IdentityError { Description = "Passwords must have at least one digit." };

        _userManagerMock.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(user);
        _userManagerMock
            .Setup(m => m.CheckPasswordAsync(user, dto.CurrentPassword))
            .ReturnsAsync(true);
        _userManagerMock
            .Setup(m => m.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword))
            .ReturnsAsync(IdentityResult.Failed(identityError));

        var service = CreateService();

        var result = await service.ChangePasswordAsync("user-1", dto);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Passwords must have at least one digit.");
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenValid_ChangesPasswordCreatesAuditLogAndReturnsSuccess()
    {
        var user = CreateUser("user-1");
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "oldPass",
            NewPassword = "newPass123",
            ConfirmNewPassword = "newPass123"
        };

        _userManagerMock.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(user);
        _userManagerMock
            .Setup(m => m.CheckPasswordAsync(user, dto.CurrentPassword))
            .ReturnsAsync(true);
        _userManagerMock
            .Setup(m => m.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword))
            .ReturnsAsync(IdentityResult.Success);

        var service = CreateService();

        var result = await service.ChangePasswordAsync("user-1", dto);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Password changed successfully.");

        _auditLogRepositoryMock.Verify(r => r.AddAsync(It.Is<AuditLog>(a =>
            a.UserId == "user-1" &&
            a.ActionType == AuditActionType.PasswordChanged &&
            a.EntityName == "User" &&
            a.EntityId == "user-1" &&
            a.Details == "User changed their password.")), Times.Once);
    }

    private InfraReportingSystem.Services.Profile.ProfileService CreateService()
    {
        return new InfraReportingSystem.Services.Profile.ProfileService(
            _profileRepositoryMock.Object,
            _userManagerMock.Object,
            _auditLogRepositoryMock.Object);
    }

    private static User CreateUser(string id)
    {
        return new User
        {
            Id = id,
            Name = "Test User",
            UserName = "test@example.com",
            Email = "test@example.com",
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static Mock<UserManager<User>> CreateUserManagerMock()
    {
        var storeMock = new Mock<IUserStore<User>>();

        return new Mock<UserManager<User>>(
            storeMock.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);
    }
}
