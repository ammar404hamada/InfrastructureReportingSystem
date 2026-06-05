using FluentAssertions;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Worker.TasksScreen;
using InfraReportingSystem.Services.Users.Worker.TasksScreen;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace InfrastructureReportingSystem.Tests.Unit.Users.Worker.TasksScreen;

public class WorkerTasksServiceTests
{
    private readonly Mock<IWorkerTasksRepository> _workerTasksRepositoryMock = new();
    private readonly Mock<ILogger<WorkerTasksService>> _loggerMock = new();

    [Fact]
    public async Task GetMyTasksAsync_WhenNoTasksFound_LogsInformationAndReturnsEmptyPaginatedResultWithCustomMessage()
    {
        const string workerId = "worker-123";
        const string searchTerm = "plumbing";

        _workerTasksRepositoryMock
            .Setup(repo => repo.GetMyTasksAsync(workerId, searchTerm, 1, 10))
            .ReturnsAsync((new List<Report>(), 0));

        var service = CreateService();

        var result = await service.GetMyTasksAsync(workerId, searchTerm, 1, 10);

        result.Should().NotBeNull();
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
        result.Message.Should().Be("No tasks found. Try adjusting your search or check back later.");

        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Worker {workerId} has no assigned tasks. SearchTerm: {searchTerm}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetMyTasksAsync_WhenTasksFound_ReturnsMappedPaginatedResultWithEmptyMessage()
    {
        const string workerId = "worker-123";
        var report = CreateReport(45, workerId, ReportStatus.Assigned);
        var reports = new List<Report> { report };

        _workerTasksRepositoryMock
            .Setup(repo => repo.GetMyTasksAsync(workerId, null, 1, 10))
            .ReturnsAsync((reports, 1));

        var service = CreateService();

        var result = await service.GetMyTasksAsync(workerId, null, 1, 10);

        result.Should().NotBeNull();
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(1);
        result.Message.Should().BeEmpty();
        result.Items.Should().ContainSingle();

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

        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    private WorkerTasksService CreateService()
    {
        return new WorkerTasksService(
            _workerTasksRepositoryMock.Object,
            _loggerMock.Object);
    }

    private static Report CreateReport(int id, string workerId, ReportStatus status)
    {
        var category = new Category { Id = 1, Name = "Roads" };
        var submitter = new User { Id = "user-abc", Name = "Ali" };
        var authority = new InfraReportingSystem.Domain.Entities.Authority { Id = "auth-xyz", Name = "Highway Dept" };
        var reportPics = new List<ReportPic>
        {
            new() { PicId = 10, ReportId = id, PicUrl = "http://example.com/pic1.jpg" }
        };

        return new Report
        {
            Id = id,
            Description = "Traffic light signal out",
            Status = status,
            Latitude = 30.0777,
            Longitude = 31.2888,
            UploadedAt = DateTime.UtcNow.AddDays(-3),
            AssignedAt = DateTime.UtcNow.AddDays(-2),
            CategoryId = category.Id,
            Category = category,
            SubmittedById = submitter.Id,
            SubmittedBy = submitter,
            AssignedWorkerId = workerId,
            AssignedByAuthorityId = authority.Id,
            AssignedByAuthority = authority,
            ReportPics = reportPics
        };
    }
}
