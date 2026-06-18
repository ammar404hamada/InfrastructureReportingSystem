using FluentAssertions;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Shared;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Admin.UsersManagementScreen;
using InfraReportingSystem.Services.Users.Admin.UsersManagementScreen;
using InfraReportingSystem.Shared.DTOs.UserServices.Admin.UsersManagementScreen;
using Moq;
using Xunit;

namespace InfrastructureReportingSystem.Tests.Unit.Users.Admin.UsersManagementScreen;

public class AdminUsersServiceTests
{
    private readonly Mock<IAdminUsersRepository> _repositoryMock = new();
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock = new();

    [Fact]
    public async Task GetUsersAsync_WhenNoUsersFound_ReturnsEmptyResultWithCustomMessage()
    {
        _repositoryMock
            .Setup(repo => repo.GetUsersAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<UserStatus?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync((new List<User>(), 0));

        var service = CreateService();

        var result = await service.GetUsersAsync(null, null, null, null, null, 1, 10);

        result.Should().NotBeNull();
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
        result.Message.Should().Be("No users found. Try adjusting your search or filters.");
    }

    [Fact]
    public async Task GetUsersAsync_WhenUsersFound_ReturnsMappedResults()
    {
        var users = new List<User>
        {
            new InfraReportingSystem.Domain.Entities.Worker
            {
                Id = "w-1",
                Name = "Worker User",
                Email = "worker@example.com",
                PhoneNumber = "01011111111",
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow.AddMonths(-2),
                Specialization = "Plumbing"
            },
            new InfraReportingSystem.Domain.Entities.Authority
            {
                Id = "a-1",
                Name = "Authority User",
                Email = "authority@example.com",
                PhoneNumber = "01122222222",
                Status = UserStatus.Inactive,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new User
            {
                Id = "p-1",
                Name = "Public User",
                Email = "public@example.com",
                PhoneNumber = "01033333333",
                Status = UserStatus.Suspended,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            }
        };

        _repositoryMock
            .Setup(repo => repo.GetUsersAsync("searchTerm", "Worker", UserStatus.Active, "name", "asc", 2, 15))
            .ReturnsAsync((users, 3));

        var service = CreateService();

        var result = await service.GetUsersAsync("searchTerm", "Worker", "Active", "name", "asc", 2, 15);

        result.Should().NotBeNull();
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(15);
        result.TotalCount.Should().Be(3);
        result.Message.Should().BeEmpty();
        result.Items.Should().HaveCount(3);

        var first = result.Items[0];
        first.Id.Should().Be("w-1");
        first.Name.Should().Be("Worker User");
        first.Email.Should().Be("worker@example.com");
        first.PhoneNumber.Should().Be("01011111111");
        first.Role.Should().Be("Worker");
        first.Status.Should().Be("Active");
        first.JoinDate.Should().Be(users[0].CreatedAt);

        var second = result.Items[1];
        second.Id.Should().Be("a-1");
        second.Name.Should().Be("Authority User");
        second.Email.Should().Be("authority@example.com");
        second.PhoneNumber.Should().Be("01122222222");
        second.Role.Should().Be("Authority");
        second.Status.Should().Be("Inactive");
        second.JoinDate.Should().Be(users[1].CreatedAt);

        var third = result.Items[2];
        third.Id.Should().Be("p-1");
        third.Name.Should().Be("Public User");
        third.Email.Should().Be("public@example.com");
        third.PhoneNumber.Should().Be("01033333333");
        third.Role.Should().Be("Public User");
        third.Status.Should().Be("Suspended");
        third.JoinDate.Should().Be(users[2].CreatedAt);
    }

    [Theory]
    [InlineData(0, 1)]      // page number < 1 clamps to 1
    [InlineData(-5, 1)]     // negative page number clamps to 1
    public async Task GetUsersAsync_WhenInvalidPageNumberProvided_ClampsToMinimumOf1(int inputPage, int expectedPage)
    {
        _repositoryMock
            .Setup(repo => repo.GetUsersAsync(null, null, null, "joindate", "desc", expectedPage, 10))
            .ReturnsAsync((new List<User>(), 0));

        var service = CreateService();

        await service.GetUsersAsync(null, null, null, null, null, inputPage, 10);

        _repositoryMock.Verify(
            repo => repo.GetUsersAsync(null, null, null, "joindate", "desc", expectedPage, 10),
            Times.Once);
    }

    [Theory]
    [InlineData(0, 1)]      // size < 1 clamps to 1
    [InlineData(100, 50)]   // size > 50 clamps to 50
    [InlineData(25, 25)]    // within range stays same
    public async Task GetUsersAsync_WhenPageSizeProvided_ClampsBetween1And50(int inputSize, int expectedSize)
    {
        _repositoryMock
            .Setup(repo => repo.GetUsersAsync(null, null, null, "joindate", "desc", 1, expectedSize))
            .ReturnsAsync((new List<User>(), 0));

        var service = CreateService();

        await service.GetUsersAsync(null, null, null, null, null, 1, inputSize);

        _repositoryMock.Verify(
            repo => repo.GetUsersAsync(null, null, null, "joindate", "desc", 1, expectedSize),
            Times.Once);
    }

    [Theory]
    [InlineData(null, "joindate")]
    [InlineData("", "joindate")]
    [InlineData("invalidSort", "joindate")]
    [InlineData("name", "name")]
    [InlineData("email", "email")]
    [InlineData("joindate", "joindate")]
    [InlineData("NAME", "NAME")] // case-insensitive hashset match test: AllowedSortFields has StringComparer.OrdinalIgnoreCase
    public async Task GetUsersAsync_WhenSortByFieldProvided_ValidatesAndDefaultsToJoinDate(string? inputSortBy, string expectedSortByForRepo)
    {
        _repositoryMock
            .Setup(repo => repo.GetUsersAsync(null, null, null, expectedSortByForRepo, "desc", 1, 10))
            .ReturnsAsync((new List<User>(), 0));

        var service = CreateService();

        await service.GetUsersAsync(null, null, null, inputSortBy, null, 1, 10);

        _repositoryMock.Verify(
            repo => repo.GetUsersAsync(null, null, null, expectedSortByForRepo, "desc", 1, 10),
            Times.Once);
    }

    [Theory]
    [InlineData(null, "desc")]
    [InlineData("", "desc")]
    [InlineData("invalidDirection", "desc")]
    [InlineData("asc", "asc")]
    [InlineData("desc", "desc")]
    [InlineData("ASC", "ASC")]
    public async Task GetUsersAsync_WhenSortDirectionProvided_ValidatesAndDefaultsToDesc(string? inputDirection, string expectedDirectionForRepo)
    {
        _repositoryMock
            .Setup(repo => repo.GetUsersAsync(null, null, null, "joindate", expectedDirectionForRepo, 1, 10))
            .ReturnsAsync((new List<User>(), 0));

        var service = CreateService();

        await service.GetUsersAsync(null, null, null, null, inputDirection, 1, 10);

        _repositoryMock.Verify(
            repo => repo.GetUsersAsync(null, null, null, "joindate", expectedDirectionForRepo, 1, 10),
            Times.Once);
    }

    [Theory]
    [InlineData("  Worker  ", "Worker")]
    [InlineData("Authority", "Authority")]
    [InlineData("PublicUser", "PublicUser")]
    [InlineData("Manager", null)] // Invalid role normalizes to null
    public async Task GetUsersAsync_WhenRoleProvided_NormalizesOrSetsToNull(string? inputRole, string? expectedRoleForRepo)
    {
        _repositoryMock
            .Setup(repo => repo.GetUsersAsync(null, expectedRoleForRepo, null, "joindate", "desc", 1, 10))
            .ReturnsAsync((new List<User>(), 0));

        var service = CreateService();

        await service.GetUsersAsync(null, inputRole, null, null, null, 1, 10);

        _repositoryMock.Verify(
            repo => repo.GetUsersAsync(null, expectedRoleForRepo, null, "joindate", "desc", 1, 10),
            Times.Once);
    }

    [Theory]
    [InlineData("Active", UserStatus.Active)]
    [InlineData("Inactive", UserStatus.Inactive)]
    [InlineData("Suspended", UserStatus.Suspended)]
    [InlineData("Locked", UserStatus.Locked)]
    [InlineData("invalidStatus", null)]
    [InlineData("", null)]
    public async Task GetUsersAsync_WhenStatusProvided_ParsesToEnumOrSetsToNull(string? inputStatus, UserStatus? expectedStatusForRepo)
    {
        _repositoryMock
            .Setup(repo => repo.GetUsersAsync(null, null, expectedStatusForRepo, "joindate", "desc", 1, 10))
            .ReturnsAsync((new List<User>(), 0));

        var service = CreateService();

        await service.GetUsersAsync(null, null, inputStatus, null, null, 1, 10);

        _repositoryMock.Verify(
            repo => repo.GetUsersAsync(null, null, expectedStatusForRepo, "joindate", "desc", 1, 10),
            Times.Once);
    }

    [Fact]
    public async Task GetUserProfileByIdAsync_WhenUserNotFound_ReturnsFailureResponse()
    {
        _repositoryMock
            .Setup(repo => repo.GetUserProfileByIdAsync("missing-id"))
            .ReturnsAsync((null, null));

        var service = CreateService();

        var result = await service.GetUserProfileByIdAsync("missing-id");

        result.Success.Should().BeFalse();
        result.Message.Should().Be("User not found.");
        result.Profile.Should().BeNull();
    }

    [Fact]
    public async Task GetUserProfileByIdAsync_WhenAdminUserFound_ReturnsProfileWithAdminRole()
    {
        var user = new User
        {
            Id = "admin-1",
            Name = "Admin User",
            Email = "admin@example.com",
            PhoneNumber = "01011111111",
            ProfilePictureUrl = "https://example.com/admin.jpg",
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow.AddYears(-1)
        };

        _repositoryMock
            .Setup(repo => repo.GetUserProfileByIdAsync("admin-1"))
            .ReturnsAsync((user, "Admin"));

        var service = CreateService();

        var result = await service.GetUserProfileByIdAsync("admin-1");

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Profile retrieved successfully.");
        result.Profile.Should().NotBeNull();
        result.Profile!.Id.Should().Be("admin-1");
        result.Profile.Name.Should().Be("Admin User");
        result.Profile.Email.Should().Be("admin@example.com");
        result.Profile.PhoneNumber.Should().Be("01011111111");
        result.Profile.ProfilePictureUrl.Should().Be("https://example.com/admin.jpg");
        result.Profile.Role.Should().Be("Admin");
        result.Profile.Status.Should().Be("Active");
        result.Profile.JoinDate.Should().Be(user.CreatedAt);
        result.Profile.Specialization.Should().BeNull();
    }

    [Fact]
    public async Task GetUserProfileByIdAsync_WhenWorkerFound_ReturnsSpecialization()
    {
        var user = new InfraReportingSystem.Domain.Entities.Worker
        {
            Id = "worker-1",
            Name = "Worker User",
            Email = "worker@example.com",
            PhoneNumber = "01022222222",
            ProfilePictureUrl = "https://example.com/worker.jpg",
            Status = UserStatus.Inactive,
            CreatedAt = DateTime.UtcNow.AddMonths(-6),
            Specialization = "Electrical"
        };

        _repositoryMock
            .Setup(repo => repo.GetUserProfileByIdAsync("worker-1"))
            .ReturnsAsync((user, "Worker"));

        var service = CreateService();

        var result = await service.GetUserProfileByIdAsync("worker-1");

        result.Success.Should().BeTrue();
        result.Profile.Should().NotBeNull();
        result.Profile!.Role.Should().Be("Worker");
        result.Profile.Specialization.Should().Be("Electrical");
    }

    [Theory]
    [InlineData("Authority")]
    [InlineData("PublicUser")]
    public async Task GetUserProfileByIdAsync_WhenNonWorkerFound_ReturnsNullSpecialization(string role)
    {
        User user = role == "Authority"
            ? new InfraReportingSystem.Domain.Entities.Authority()
            : new User();

        user.Id = "user-1";
        user.Name = "Non Worker User";
        user.Email = "user@example.com";
        user.Status = UserStatus.Active;
        user.CreatedAt = DateTime.UtcNow;

        _repositoryMock
            .Setup(repo => repo.GetUserProfileByIdAsync("user-1"))
            .ReturnsAsync((user, role));

        var service = CreateService();

        var result = await service.GetUserProfileByIdAsync("user-1");

        result.Success.Should().BeTrue();
        result.Profile.Should().NotBeNull();
        result.Profile!.Role.Should().Be(role);
        result.Profile.Specialization.Should().BeNull();
    }

    [Fact]
    public void AdminUserProfileDto_ShouldNotExposeReportRelatedProperties()
    {
        var propertyNames = typeof(AdminUserProfileDto)
            .GetProperties()
            .Select(property => property.Name);

        propertyNames.Should().NotContain(propertyName =>
            propertyName.Contains("Report", StringComparison.OrdinalIgnoreCase) ||
            propertyName.Contains("Task", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ChangeUserStatusAsync_WhenUserIdIsMissing_ReturnsFailure()
    {
        var request = new AdminChangeUserStatusRequestDto { UserId = "", Status = "Active" };

        var service = CreateService();

        var result = await service.ChangeUserStatusAsync(request, "admin-1");

        result.Success.Should().BeFalse();
        result.Message.Should().Be("User ID is required.");
        _repositoryMock.Verify(repo => repo.GetUserProfileByIdAsync(It.IsAny<string>()), Times.Never);
        _auditLogRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<AuditLog>()), Times.Never);
    }

    [Fact]
    public async Task ChangeUserStatusAsync_WhenStatusIsMissing_ReturnsFailure()
    {
        var request = new AdminChangeUserStatusRequestDto { UserId = "user-1", Status = "" };

        var service = CreateService();

        var result = await service.ChangeUserStatusAsync(request, "admin-1");

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Status is required.");
        _repositoryMock.Verify(repo => repo.GetUserProfileByIdAsync(It.IsAny<string>()), Times.Never);
        _auditLogRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<AuditLog>()), Times.Never);
    }

    [Fact]
    public async Task ChangeUserStatusAsync_WhenStatusIsInvalid_ReturnsFailure()
    {
        var request = new AdminChangeUserStatusRequestDto { UserId = "user-1", Status = "Bogus" };

        var service = CreateService();

        var result = await service.ChangeUserStatusAsync(request, "admin-1");

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid status value 'Bogus'. Valid values are Active, Inactive, Suspended, and Locked.");
        _repositoryMock.Verify(repo => repo.GetUserProfileByIdAsync(It.IsAny<string>()), Times.Never);
        _auditLogRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<AuditLog>()), Times.Never);
    }

    [Fact]
    public async Task ChangeUserStatusAsync_WhenStatusIsDeleted_ReturnsFailure()
    {
        var request = new AdminChangeUserStatusRequestDto { UserId = "user-1", Status = "Deleted" };

        var service = CreateService();

        var result = await service.ChangeUserStatusAsync(request, "admin-1");

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Status cannot be set to Deleted.");
        _repositoryMock.Verify(repo => repo.GetUserProfileByIdAsync(It.IsAny<string>()), Times.Never);
        _auditLogRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<AuditLog>()), Times.Never);
    }

    [Fact]
    public async Task ChangeUserStatusAsync_WhenAdminChangesOwnStatus_ReturnsFailure()
    {
        var request = new AdminChangeUserStatusRequestDto { UserId = "admin-1", Status = "Inactive" };

        var service = CreateService();

        var result = await service.ChangeUserStatusAsync(request, "admin-1");

        result.Success.Should().BeFalse();
        result.Message.Should().Be("You cannot change your own status.");
        _repositoryMock.Verify(repo => repo.GetUserProfileByIdAsync(It.IsAny<string>()), Times.Never);
        _auditLogRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<AuditLog>()), Times.Never);
    }

    [Fact]
    public async Task ChangeUserStatusAsync_WhenUserNotFound_ReturnsFailure()
    {
        var request = new AdminChangeUserStatusRequestDto { UserId = "missing-id", Status = "Suspended" };

        _repositoryMock
            .Setup(repo => repo.GetUserProfileByIdAsync("missing-id"))
            .ReturnsAsync((null, null));

        var service = CreateService();

        var result = await service.ChangeUserStatusAsync(request, "admin-1");

        result.Success.Should().BeFalse();
        result.Message.Should().Be("User not found.");
        _repositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
        _auditLogRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<AuditLog>()), Times.Never);
    }

    [Theory]
    [InlineData("Active", UserStatus.Active, AuditActionType.AccountActivated)]
    [InlineData("Inactive", UserStatus.Inactive, AuditActionType.AccountDeactivated)]
    [InlineData("Suspended", UserStatus.Suspended, AuditActionType.AccountSuspended)]
    [InlineData("Locked", UserStatus.Locked, AuditActionType.AccountLocked)]
    public async Task ChangeUserStatusAsync_WhenValidRequest_UpdatesStatusLogsAuditAndReturnsProfile(
        string statusString, UserStatus expectedStatus, AuditActionType expectedAuditAction)
    {
        var user = new User
        {
            Id = "target-1",
            Name = "Target User",
            Email = "target@example.com",
            PhoneNumber = "01099999999",
            ProfilePictureUrl = "https://example.com/pic.jpg",
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow.AddMonths(-3)
        };

        var request = new AdminChangeUserStatusRequestDto { UserId = "target-1", Status = statusString };

        _repositoryMock
            .Setup(repo => repo.GetUserProfileByIdAsync("target-1"))
            .ReturnsAsync((user, "Worker"));

        User? updatedUser = null;
        _repositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
            .Callback<User>(u => updatedUser = u)
            .Returns(Task.CompletedTask);

        AuditLog? capturedAuditLog = null;
        _auditLogRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<AuditLog>()))
            .Callback<AuditLog>(log => capturedAuditLog = log)
            .Returns(Task.CompletedTask);

        var service = CreateService();

        var result = await service.ChangeUserStatusAsync(request, "admin-1");

        result.Success.Should().BeTrue();
        result.Message.Should().Be("User status updated successfully.");
        result.Profile.Should().NotBeNull();
        result.Profile!.Id.Should().Be("target-1");
        result.Profile.Name.Should().Be("Target User");
        result.Profile.Email.Should().Be("target@example.com");
        result.Profile.PhoneNumber.Should().Be("01099999999");
        result.Profile.ProfilePictureUrl.Should().Be("https://example.com/pic.jpg");
        result.Profile.Status.Should().Be(statusString);
        result.Profile.Specialization.Should().BeNull();

        updatedUser.Should().NotBeNull();
        updatedUser!.Status.Should().Be(expectedStatus);

        capturedAuditLog.Should().NotBeNull();
        capturedAuditLog!.UserId.Should().Be("admin-1");
        capturedAuditLog.ActionType.Should().Be(expectedAuditAction);
        capturedAuditLog.EntityName.Should().Be("User");
        capturedAuditLog.EntityId.Should().Be("target-1");
        capturedAuditLog.Details.Should().Be(
            "Admin changed user status for Target User (target@example.com) from Active to " + statusString + ".");
    }

    [Fact]
    public async Task ChangeUserStatusAsync_WhenTargetIsAnotherAdmin_UpdatesSuccessfully()
    {
        var user = new User
        {
            Id = "other-admin-1",
            Name = "Other Admin",
            Email = "otheradmin@example.com",
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var request = new AdminChangeUserStatusRequestDto { UserId = "other-admin-1", Status = "Inactive" };

        _repositoryMock
            .Setup(repo => repo.GetUserProfileByIdAsync("other-admin-1"))
            .ReturnsAsync((user, "Admin"));

        _repositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _auditLogRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        var result = await service.ChangeUserStatusAsync(request, "current-admin-1");

        result.Success.Should().BeTrue();
        result.Message.Should().Be("User status updated successfully.");
    }

    [Fact]
    public async Task ChangeUserStatusAsync_WhenWorkerIsUpdated_ReturnsSpecializationInProfile()
    {
        var user = new InfraReportingSystem.Domain.Entities.Worker
        {
            Id = "worker-1",
            Name = "Worker User",
            Email = "worker@example.com",
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow,
            Specialization = "Electrical"
        };

        var request = new AdminChangeUserStatusRequestDto { UserId = "worker-1", Status = "Suspended" };

        _repositoryMock
            .Setup(repo => repo.GetUserProfileByIdAsync("worker-1"))
            .ReturnsAsync((user, "Worker"));

        _repositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _auditLogRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        var result = await service.ChangeUserStatusAsync(request, "admin-1");

        result.Success.Should().BeTrue();
        result.Profile.Should().NotBeNull();
        result.Profile!.Role.Should().Be("Worker");
        result.Profile.Specialization.Should().Be("Electrical");
    }

    [Fact]
    public async Task DeleteUserAsync_WhenUserIdIsBlank_ReturnsFailure()
    {
        var service = CreateService();

        var result = await service.DeleteUserAsync("", "admin-1");

        result.Success.Should().BeFalse();
        result.Message.Should().Be("User ID is required.");
        _repositoryMock.Verify(repo => repo.GetUserProfileByIdAsync(It.IsAny<string>()), Times.Never);
        _auditLogRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<AuditLog>()), Times.Never);
    }

    [Fact]
    public async Task DeleteUserAsync_WhenAdminDeletesOwnAccount_ReturnsFailure()
    {
        var service = CreateService();

        var result = await service.DeleteUserAsync("admin-1", "admin-1");

        result.Success.Should().BeFalse();
        result.Message.Should().Be("You cannot delete your own account.");
        _repositoryMock.Verify(repo => repo.GetUserProfileByIdAsync(It.IsAny<string>()), Times.Never);
        _auditLogRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<AuditLog>()), Times.Never);
    }

    [Fact]
    public async Task DeleteUserAsync_WhenUserNotFound_ReturnsFailure()
    {
        _repositoryMock
            .Setup(repo => repo.GetUserProfileByIdAsync("missing-id"))
            .ReturnsAsync((null, null));

        var service = CreateService();

        var result = await service.DeleteUserAsync("missing-id", "admin-1");

        result.Success.Should().BeFalse();
        result.Message.Should().Be("User not found.");
        _repositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
        _auditLogRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<AuditLog>()), Times.Never);
    }

    [Fact]
    public async Task DeleteUserAsync_WhenUserAlreadyDeleted_ReturnsFailure()
    {
        var user = new User
        {
            Id = "deleted-1",
            Name = "Deleted User",
            Email = "deleted@example.com",
            Status = UserStatus.Deleted,
            CreatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(repo => repo.GetUserProfileByIdAsync("deleted-1"))
            .ReturnsAsync((user, "Public User"));

        var service = CreateService();

        var result = await service.DeleteUserAsync("deleted-1", "admin-1");

        result.Success.Should().BeFalse();
        result.Message.Should().Be("User is already deleted.");
        _repositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
        _auditLogRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<AuditLog>()), Times.Never);
    }

    [Fact]
    public async Task DeleteUserAsync_WhenValidRequest_SetsStatusToDeletedLogsAuditAndReturnsSuccess()
    {
        var user = new User
        {
            Id = "target-1",
            Name = "Target User",
            Email = "target@example.com",
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow.AddMonths(-3)
        };

        _repositoryMock
            .Setup(repo => repo.GetUserProfileByIdAsync("target-1"))
            .ReturnsAsync((user, "Public User"));

        User? updatedUser = null;
        _repositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
            .Callback<User>(u => updatedUser = u)
            .Returns(Task.CompletedTask);

        AuditLog? capturedAuditLog = null;
        _auditLogRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<AuditLog>()))
            .Callback<AuditLog>(log => capturedAuditLog = log)
            .Returns(Task.CompletedTask);

        var service = CreateService();

        var result = await service.DeleteUserAsync("target-1", "admin-1");

        result.Success.Should().BeTrue();
        result.Message.Should().Be("User deleted successfully.");

        updatedUser.Should().NotBeNull();
        updatedUser!.Status.Should().Be(UserStatus.Deleted);

        capturedAuditLog.Should().NotBeNull();
        capturedAuditLog!.UserId.Should().Be("admin-1");
        capturedAuditLog.ActionType.Should().Be(AuditActionType.AccountDeleted);
        capturedAuditLog.EntityName.Should().Be("User");
        capturedAuditLog.EntityId.Should().Be("target-1");
        capturedAuditLog.Details.Should().Be("Admin deleted user Target User (target@example.com).");
    }

    [Fact]
    public async Task DeleteUserAsync_WhenTargetIsAnotherAdmin_DeletesSuccessfully()
    {
        var user = new User
        {
            Id = "other-admin-1",
            Name = "Other Admin",
            Email = "otheradmin@example.com",
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(repo => repo.GetUserProfileByIdAsync("other-admin-1"))
            .ReturnsAsync((user, "Admin"));

        _repositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _auditLogRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<AuditLog>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        var result = await service.DeleteUserAsync("other-admin-1", "current-admin-1");

        result.Success.Should().BeTrue();
        result.Message.Should().Be("User deleted successfully.");
    }

    [Fact]
    public async Task DeleteUserAsync_WhenUserHasNoEmail_AuditLogOmitsEmail()
    {
        var user = new User
        {
            Id = "noemail-1",
            Name = "No Email User",
            Email = null,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(repo => repo.GetUserProfileByIdAsync("noemail-1"))
            .ReturnsAsync((user, "Public User"));

        _repositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        AuditLog? capturedAuditLog = null;
        _auditLogRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<AuditLog>()))
            .Callback<AuditLog>(log => capturedAuditLog = log)
            .Returns(Task.CompletedTask);

        var service = CreateService();

        var result = await service.DeleteUserAsync("noemail-1", "admin-1");

        result.Success.Should().BeTrue();
        capturedAuditLog.Should().NotBeNull();
        capturedAuditLog!.Details.Should().Be("Admin deleted user No Email User.");
    }

    private AdminUsersService CreateService()
    {
        return new AdminUsersService(_repositoryMock.Object, _auditLogRepositoryMock.Object);
    }
}
