using FluentAssertions;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Admin.AuditLogsScreen;
using InfraReportingSystem.Services.Users.Admin.AuditLogsScreen;
using InfraReportingSystem.Shared.DTOs.UserServices.Admin.AuditLogsScreen;
using Moq;
using Xunit;

namespace InfrastructureReportingSystem.Tests.Unit.Users.Admin.AuditLogsScreen;

public class AdminAuditLogsServiceTests
{
    private readonly Mock<IAdminAuditLogsRepository> _repositoryMock = new();

    [Fact]
    public async Task GetLogsAsync_WhenDateRangeIsInvalid_ReturnsFailureAndSkipsQuery()
    {
        var filter = new AuditLogsFilterDto
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(-1)
        };

        var service = CreateService();

        var result = await service.GetLogsAsync(filter);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
        result.Message.Should().Be("Invalid date range.");

        _repositoryMock.Verify(
            repo => repo.GetAuditLogsAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<string>(),
                It.IsAny<AuditActionType?>()),
            Times.Never);
    }

    [Fact]
    public async Task GetLogsAsync_WhenRoleFilterIsInvalid_ReturnsFailureAndSkipsQuery()
    {
        var filter = new AuditLogsFilterDto
        {
            Role = "InvalidRole"
        };

        var service = CreateService();

        var result = await service.GetLogsAsync(filter);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
        result.Message.Should().Be("Invalid role filter.");

        _repositoryMock.Verify(
            repo => repo.GetAuditLogsAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<string>(),
                It.IsAny<AuditActionType?>()),
            Times.Never);
    }

    [Fact]
    public async Task GetLogsAsync_WhenActionTypeIsInvalid_ReturnsFailureAndSkipsQuery()
    {
        var filter = new AuditLogsFilterDto
        {
            ActionType = "InvalidAction"
        };

        var service = CreateService();

        var result = await service.GetLogsAsync(filter);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
        result.Message.Should().Be("Invalid action type filter.");

        _repositoryMock.Verify(
            repo => repo.GetAuditLogsAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<string>(),
                It.IsAny<AuditActionType?>()),
            Times.Never);
    }

    [Fact]
    public async Task GetLogsAsync_WhenNoLogsFound_ReturnsEmptyResultWithCustomMessage()
    {
        var filter = new AuditLogsFilterDto
        {
            PageNumber = 1,
            PageSize = 10,
            Search = "  missing  ",
            Role = "  worker  ",
            ActionType = "AccountCreated"
        };

        _repositoryMock
            .Setup(repo => repo.GetAuditLogsAsync(1, 10, "missing", It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), "worker", AuditActionType.AccountCreated))
            .ReturnsAsync((new List<AuditLog>(), 0, new Dictionary<string, string>()));

        var service = CreateService();

        var result = await service.GetLogsAsync(filter);

        result.Should().NotBeNull();
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
        result.Message.Should().Be("No audit logs found matching the criteria.");
    }

    [Theory]
    [InlineData("Admin", "admin")]
    [InlineData("Authority", "authority")]
    [InlineData("Worker", "worker")]
    [InlineData("PublicUser", "publicuser")]
    [InlineData("System", "system")]
    [InlineData("AllRoles", null)]
    public async Task GetLogsAsync_WhenValidFiltersProvided_NormalizesParametersAndQueriesRepository(string roleFilter, string? expectedRoleForRepo)
    {
        var filter = new AuditLogsFilterDto
        {
            PageNumber = 0, // boundary: should clamp to 1
            PageSize = 100, // boundary: should clamp to 50
            Role = roleFilter
        };

        _repositoryMock
            .Setup(repo => repo.GetAuditLogsAsync(1, 50, null, null, null, expectedRoleForRepo, null))
            .ReturnsAsync((new List<AuditLog>(), 0, new Dictionary<string, string>()));

        var service = CreateService();

        await service.GetLogsAsync(filter);

        _repositoryMock.Verify(
            repo => repo.GetAuditLogsAsync(1, 50, null, null, null, expectedRoleForRepo, null),
            Times.Once);
    }

    [Fact]
    public async Task GetLogsAsync_WhenLogsFound_MapsDtosAndReturnsSuccess()
    {
        var filter = new AuditLogsFilterDto { PageNumber = 2, PageSize = 15 };
        var logs = new List<AuditLog>
        {
            new()
            {
                Id = 1,
                Timestamp = DateTime.UtcNow.AddMinutes(-5),
                UserId = "user-1",
                User = new User { Name = "Mostafa", Email = "mostafa@example.com" },
                ActionType = AuditActionType.AccountCreated,
                EntityName = "User",
                EntityId = "user-2"
            },
            new()
            {
                Id = 2,
                Timestamp = DateTime.UtcNow.AddMinutes(-10),
                UserId = null,
                User = null,
                ActionType = AuditActionType.ReportBlocked,
                EntityName = null,
                EntityId = null
            }
        };

        var userRoles = new Dictionary<string, string>
        {
            ["user-1"] = "Admin"
        };

        _repositoryMock
            .Setup(repo => repo.GetAuditLogsAsync(2, 15, null, null, null, null, null))
            .ReturnsAsync((logs, 2, userRoles));

        var service = CreateService();

        var result = await service.GetLogsAsync(filter);

        result.Should().NotBeNull();
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(15);
        result.TotalCount.Should().Be(2);
        result.Message.Should().BeEmpty();
        result.Items.Should().HaveCount(2);

        var first = result.Items[0];
        first.Timestamp.Should().Be(logs[0].Timestamp);
        first.ActorName.Should().Be("Mostafa");
        first.ActorEmail.Should().Be("mostafa@example.com");
        first.ActorRole.Should().Be("Admin");
        first.ActionType.Should().Be("AccountCreated");
        first.Target.Should().Be("User user-2");

        var second = result.Items[1];
        second.Timestamp.Should().Be(logs[1].Timestamp);
        second.ActorName.Should().Be("System");
        second.ActorEmail.Should().Be("System");
        second.ActorRole.Should().Be("System");
        second.ActionType.Should().Be("ReportBlocked");
        second.Target.Should().Be("System");
    }

    private AdminAuditLogsService CreateService()
    {
        return new AdminAuditLogsService(_repositoryMock.Object);
    }
}
