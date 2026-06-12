using FluentAssertions;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Worker.TasksHistoryScreen;
using InfraReportingSystem.Services.Users.Worker.TasksHistoryScreen;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace InfrastructureReportingSystem.Tests.Unit.Users.Worker.TasksHistoryScreen;

public class WorkerHistoryServiceTests
{
    private readonly Mock<IWorkerHistoryRepository> _repositoryMock = new();
    private readonly Mock<ILogger<WorkerHistoryService>> _loggerMock = new();

    [Fact]
    public async Task GetHistoryAsync_WhenStatusFilterIsProvidedButContainsNoAllowedStatuses_ReturnsEmptyPaginatedResult()
    {
        const string workerId = "worker-123";
        const string statusFilter = "Submitted, Assigned, InProgress"; // None of these are allowed in history

        var service = CreateService();

        var result = await service.GetHistoryAsync(workerId, null, statusFilter, 1, 10);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
        result.Message.Should().Be("No tasks found with the specified status.");

        _repositoryMock.Verify(
            repo => repo.GetHistoryAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<List<ReportStatus>>(), 
                It.IsAny<int>(), 
                It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task GetHistoryAsync_WhenNoHistoryMatches_LogsInfoAndReturnsMessage()
    {
        const string workerId = "worker-123";
        var expectedStatuses = new List<ReportStatus> { ReportStatus.Blocked, ReportStatus.Resolved, ReportStatus.Rejected, ReportStatus.FixRejected };

        _repositoryMock
            .Setup(repo => repo.GetHistoryAsync(workerId, "search", It.Is<List<ReportStatus>>(l => l.SequenceEqual(expectedStatuses)), 1, 10))
            .ReturnsAsync((new List<Report>(), 0));

        var service = CreateService();

        var result = await service.GetHistoryAsync(workerId, "search", null, 1, 10);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
        result.Message.Should().Be("No history found. Try adjusting your search or filter.");

        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Worker {workerId} has no history tasks matching criteria.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetHistoryAsync_WhenHistoryExists_ReturnsMappedTasks()
    {
        const string workerId = "worker-123";
        var report = CreateReport(45, workerId, ReportStatus.Resolved, "Repaired successfully");
        var reports = new List<Report> { report };
        var expectedStatuses = new List<ReportStatus> { ReportStatus.Resolved };

        _repositoryMock
            .Setup(repo => repo.GetHistoryAsync(workerId, null, It.Is<List<ReportStatus>>(l => l.SequenceEqual(expectedStatuses)), 1, 10))
            .ReturnsAsync((reports, 1));

        var service = CreateService();

        var result = await service.GetHistoryAsync(workerId, null, "Resolved", 1, 10);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Message.Should().BeEmpty();

        var item = result.Items.Single();
        item.Id.Should().Be(report.Id);
        item.Category.Should().Be(report.Category.Name);
        item.Description.Should().Be(report.Description);
        item.Photos.Should().Equal("http://example.com/pic1.jpg");
        item.Status.Should().Be(report.Status.ToString());
        item.Latitude.Should().Be(report.Latitude);
        item.Longitude.Should().Be(report.Longitude);
        item.SubmittedByName.Should().Be(report.SubmittedBy!.Name);
        item.SubmittedAt.Should().Be(report.UploadedAt);
        item.AssignedByName.Should().Be(report.AssignedByAuthority!.Name);
        item.AssignedAt.Should().Be(report.AssignedAt);
        item.MapUrl.Should().Be($"https://www.google.com/maps?q={report.Latitude},{report.Longitude}");
        item.RejectionReason.Should().Be("Repaired successfully");
    }

    [Fact]
    public async Task GetHistoryAsync_WhenStatusFilterHasMixedStatuses_FiltersOutDisallowedStatuses()
    {
        const string workerId = "worker-123";
        // Blocked is allowed, Assigned is not allowed
        const string statusFilter = "Blocked, Assigned";
        var expectedStatuses = new List<ReportStatus> { ReportStatus.Blocked };

        _repositoryMock
            .Setup(repo => repo.GetHistoryAsync(workerId, null, It.Is<List<ReportStatus>>(l => l.SequenceEqual(expectedStatuses)), 1, 10))
            .ReturnsAsync((new List<Report>(), 0));

        var service = CreateService();

        await service.GetHistoryAsync(workerId, null, statusFilter, 1, 10);

        _repositoryMock.Verify(
            repo => repo.GetHistoryAsync(workerId, null, It.Is<List<ReportStatus>>(l => l.SequenceEqual(expectedStatuses)), 1, 10),
            Times.Once);
    }

    private WorkerHistoryService CreateService()
    {
        return new WorkerHistoryService(_repositoryMock.Object, _loggerMock.Object);
    }

    private static Report CreateReport(int id, string workerId, ReportStatus status, string? rejectionReason)
    {
        var category = new Category { Id = 1, Name = "Electricity" };
        var submitter = new User { Id = "user-abc", Name = "Mariam" };
        var authority = new InfraReportingSystem.Domain.Entities.Authority { Id = "auth-xyz", Name = "Electricity Board" };
        var reportPics = new List<ReportPic>
        {
            new() { PicId = 10, ReportId = id, PicUrl = "http://example.com/pic1.jpg" }
        };

        return new Report
        {
            Id = id,
            Description = "Downed power line",
            Status = status,
            Latitude = 30.0555,
            Longitude = 31.2444,
            UploadedAt = DateTime.UtcNow.AddDays(-5),
            AssignedAt = DateTime.UtcNow.AddDays(-4),
            CategoryId = category.Id,
            Category = category,
            SubmittedById = submitter.Id,
            SubmittedBy = submitter,
            AssignedWorkerId = workerId,
            AssignedByAuthorityId = authority.Id,
            AssignedByAuthority = authority,
            ReportPics = reportPics,
            RejectionReason = rejectionReason
        };
    }
}
