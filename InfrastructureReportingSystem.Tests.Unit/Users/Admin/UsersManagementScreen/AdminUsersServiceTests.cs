using FluentAssertions;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Admin.UsersManagementScreen;
using InfraReportingSystem.Services.Users.Admin.UsersManagementScreen;
using InfraReportingSystem.Shared.DTOs.UserServices.Admin.UsersManagementScreen;
using Moq;
using Xunit;

namespace InfrastructureReportingSystem.Tests.Unit.Users.Admin.UsersManagementScreen;

public class AdminUsersServiceTests
{
    private readonly Mock<IAdminUsersRepository> _repositoryMock = new();

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

    private AdminUsersService CreateService()
    {
        return new AdminUsersService(_repositoryMock.Object);
    }
}
