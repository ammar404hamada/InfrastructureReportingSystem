using FluentAssertions;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Admin.DashboardScreen;
using InfraReportingSystem.Services.Users.Admin.DashboardScreen;
using Moq;
using Xunit;

namespace InfrastructureReportingSystem.Tests.Unit.Users.Admin.DashboardScreen;

public class AdminDashboardServiceTests
{
    private readonly Mock<IAdminDashboardRepository> _repositoryMock = new();

    [Fact]
    public async Task GetStatsAsync_WhenDataAvailable_ReturnsMappedStatsDto()
    {
        _repositoryMock.Setup(r => r.GetTotalUsersCountAsync()).ReturnsAsync(100);
        _repositoryMock.Setup(r => r.GetActiveWorkerCountAsync()).ReturnsAsync(15);
        _repositoryMock.Setup(r => r.GetPendingIssuesCountAsync()).ReturnsAsync(23);
        _repositoryMock.Setup(r => r.GetResolvedIssuesCountAsync()).ReturnsAsync(45);

        var service = CreateService();

        var result = await service.GetStatsAsync();

        result.TotalUsers.Should().Be(100);
        result.ActiveWorkers.Should().Be(15);
        result.PendingIssues.Should().Be(23);
        result.ResolvedIssues.Should().Be(45);
    }

    [Fact]
    public async Task GetWeeklyTrafficAsync_WhenDataAvailable_ReturnsMappedWeeklyTrafficList()
    {
        var data = new List<(string DayOfWeek, int ReportedCount, int ResolvedCount)>
        {
            ("Monday", 10, 5),
            ("Tuesday", 8, 3)
        };
        _repositoryMock
            .Setup(r => r.GetWeeklyTrafficAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(data);

        var service = CreateService();

        var result = await service.GetWeeklyTrafficAsync();

        result.Should().HaveCount(2);
        result[0].DayOfWeek.Should().Be("Monday");
        result[0].ReportedCount.Should().Be(10);
        result[0].ResolvedCount.Should().Be(5);
        result[1].DayOfWeek.Should().Be("Tuesday");
        result[1].ReportedCount.Should().Be(8);
        result[1].ResolvedCount.Should().Be(3);
    }

    [Fact]
    public async Task GetCategoryStatsAsync_WhenDataAvailable_ReturnsMappedCategoryStatList()
    {
        var data = new List<(string CategoryName, int TotalReported, int ResolvedCount)>
        {
            ("Roads", 20, 10),
            ("Lighting", 15, 8)
        };
        _repositoryMock
            .Setup(r => r.GetCategoryStatsAsync())
            .ReturnsAsync(data);

        var service = CreateService();

        var result = await service.GetCategoryStatsAsync();

        result.Should().HaveCount(2);
        result[0].CategoryName.Should().Be("Roads");
        result[0].TotalReported.Should().Be(20);
        result[0].ResolvedCount.Should().Be(10);
        result[1].CategoryName.Should().Be("Lighting");
        result[1].TotalReported.Should().Be(15);
        result[1].ResolvedCount.Should().Be(8);
    }

    [Fact]
    public async Task GetLiveIssuesAsync_WhenReportIsRecent_ReturnsNormalPriority()
    {
        var report = CreateReport(id: 1, uploadedAt: DateTime.UtcNow.AddDays(-2));
        _repositoryMock.Setup(r => r.GetLiveIssuesAsync()).ReturnsAsync(new List<Report> { report });

        var service = CreateService();

        var result = await service.GetLiveIssuesAsync();

        result.Should().ContainSingle();
        result[0].Priority.Should().Be("Normal");
        result[0].ReportId.Should().Be(1);
    }

    [Fact]
    public async Task GetLiveIssuesAsync_WhenReportIsOlderThanWeek_ReturnsHighPriority()
    {
        var report = CreateReport(id: 2, uploadedAt: DateTime.UtcNow.AddDays(-10));
        _repositoryMock.Setup(r => r.GetLiveIssuesAsync()).ReturnsAsync(new List<Report> { report });

        var service = CreateService();

        var result = await service.GetLiveIssuesAsync();

        result.Should().ContainSingle();
        result[0].Priority.Should().Be("High");
        result[0].ReportId.Should().Be(2);
    }

    [Fact]
    public async Task GetRecentActionsAsync_WhenLogHasUserAndEntity_ReturnsMappedAction()
    {
        var log = CreateAuditLog(userName: "Ahmed", entityName: "Report", entityId: "15");
        _repositoryMock
            .Setup(r => r.GetRecentWorkerActionsAsync(10))
            .ReturnsAsync(new List<AuditLog> { log });

        var service = CreateService();

        var result = await service.GetRecentActionsAsync();

        result.Should().ContainSingle();
        result[0].WorkerName.Should().Be("Ahmed");
        result[0].TargetEntity.Should().Be("Report 15");
    }

    [Fact]
    public async Task GetRecentActionsAsync_WhenLogHasNoUser_UsesUnknownForWorkerName()
    {
        var log = CreateAuditLog(userName: null, entityName: "Report", entityId: "15");
        _repositoryMock
            .Setup(r => r.GetRecentWorkerActionsAsync(10))
            .ReturnsAsync(new List<AuditLog> { log });

        var service = CreateService();

        var result = await service.GetRecentActionsAsync();

        result[0].WorkerName.Should().Be("Unknown");
    }

    [Fact]
    public async Task GetRecentActionsAsync_WhenLogHasNoEntity_UsesSystemForTarget()
    {
        var log = CreateAuditLog(userName: "Ahmed", entityName: "", entityId: "");
        _repositoryMock
            .Setup(r => r.GetRecentWorkerActionsAsync(10))
            .ReturnsAsync(new List<AuditLog> { log });

        var service = CreateService();

        var result = await service.GetRecentActionsAsync();

        result[0].TargetEntity.Should().Be("System");
    }

    [Fact]
    public async Task GetDashboardAsync_AggregatesAllComponentsAndReturnsCompositeResponse()
    {
        _repositoryMock.Setup(r => r.GetTotalUsersCountAsync()).ReturnsAsync(100);
        _repositoryMock.Setup(r => r.GetActiveWorkerCountAsync()).ReturnsAsync(15);
        _repositoryMock.Setup(r => r.GetPendingIssuesCountAsync()).ReturnsAsync(23);
        _repositoryMock.Setup(r => r.GetResolvedIssuesCountAsync()).ReturnsAsync(45);
        _repositoryMock
            .Setup(r => r.GetWeeklyTrafficAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<(string, int, int)>());
        _repositoryMock
            .Setup(r => r.GetCategoryStatsAsync())
            .ReturnsAsync(new List<(string, int, int)>());
        _repositoryMock
            .Setup(r => r.GetLiveIssuesAsync())
            .ReturnsAsync(new List<Report>());
        _repositoryMock
            .Setup(r => r.GetRecentWorkerActionsAsync(10))
            .ReturnsAsync(new List<AuditLog>());

        var service = CreateService();

        var result = await service.GetDashboardAsync();

        result.Should().NotBeNull();
        result.Stats.Should().NotBeNull();
        result.Stats.TotalUsers.Should().Be(100);
        result.WeeklyTraffic.Should().NotBeNull();
        result.CategoryStats.Should().NotBeNull();
        result.LiveIssues.Should().NotBeNull();
        result.RecentActions.Should().NotBeNull();
    }

    private AdminDashboardService CreateService()
    {
        return new AdminDashboardService(_repositoryMock.Object);
    }

    private static Report CreateReport(int id, DateTime uploadedAt)
    {
        var category = new Category { Id = 1, Name = "Roads" };
        var submitter = new User { Id = "user-1", Name = "Ali" };

        return new Report
        {
            Id = id,
            Description = "Test issue",
            Status = ReportStatus.Submitted,
            Latitude = 30.0,
            Longitude = 31.0,
            UploadedAt = uploadedAt,
            CategoryId = category.Id,
            Category = category,
            SubmittedById = submitter.Id,
            SubmittedBy = submitter
        };
    }

    private static AuditLog CreateAuditLog(string? userName, string entityName, string entityId)
    {
        User? user = userName is not null ? new User { Id = "u-1", Name = userName } : null;

        return new AuditLog
        {
            Id = 1,
            UserId = user?.Id,
            User = user,
            ActionType = AuditActionType.WorkerAcceptedTask,
            EntityName = entityName,
            EntityId = entityId,
            Details = "Action details",
            Timestamp = DateTime.UtcNow.AddHours(-1)
        };
    }
}
