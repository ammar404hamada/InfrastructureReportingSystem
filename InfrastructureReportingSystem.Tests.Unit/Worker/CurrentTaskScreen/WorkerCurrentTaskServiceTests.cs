using FluentAssertions;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Worker.CurrentTaskScreen;
using InfraReportingSystem.Services.Worker.CurrentTaskScreen;
using Moq;
using Xunit;

namespace InfrastructureReportingSystem.Tests.Unit.Worker.CurrentTaskScreen;

public class WorkerCurrentTaskServiceTests
{
    private readonly Mock<IWorkerCurrentTaskRepository> _repositoryMock = new();

    [Fact]
    public async Task GetCurrentTaskAsync_WhenNoCurrentTask_ReturnsEmptyPaginatedResult()
    {
        const string workerId = "worker-123";
        _repositoryMock
            .Setup(repo => repo.GetCurrentTaskAsync(workerId))
            .ReturnsAsync((Report?)null);

        var service = CreateService();

        var result = await service.GetCurrentTaskAsync(workerId);

        result.Should().NotBeNull();
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(1);
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
        result.Message.Should().Be("No current task in progress");
    }

    [Fact]
    public async Task GetCurrentTaskAsync_WhenCurrentTaskExists_ReturnsMappedPaginatedResult()
    {
        const string workerId = "worker-123";
        var report = CreateReportWithDetails(45, workerId, ReportStatus.InProgress);

        _repositoryMock
            .Setup(repo => repo.GetCurrentTaskAsync(workerId))
            .ReturnsAsync(report);

        var service = CreateService();

        var result = await service.GetCurrentTaskAsync(workerId);

        result.Should().NotBeNull();
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(1);
        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Message.Should().BeEmpty();

        var item = result.Items.Single();
        item.Id.Should().Be(report.Id);
        item.Category.Should().Be(report.Category.Name);
        item.Description.Should().Be(report.Description);
        item.Photos.Should().Equal("http://example.com/pic1.jpg", "http://example.com/pic2.jpg");
        item.Status.Should().Be(report.Status.ToString());
        item.Latitude.Should().Be(report.Latitude);
        item.Longitude.Should().Be(report.Longitude);
        item.SubmittedByName.Should().Be(report.SubmittedBy!.Name);
        item.SubmittedAt.Should().Be(report.UploadedAt);
        item.AssignedByName.Should().Be(report.AssignedByAuthority!.Name);
        item.AssignedAt.Should().Be(report.AssignedAt);
        item.MapUrl.Should().Be($"https://www.google.com/maps?q={report.Latitude},{report.Longitude}");
    }

    private WorkerCurrentTaskService CreateService()
    {
        return new WorkerCurrentTaskService(_repositoryMock.Object);
    }

    private static Report CreateReportWithDetails(int id, string workerId, ReportStatus status)
    {
        var category = new Category { Id = 1, Name = "Roads" };
        var submitter = new User { Id = "user-abc", Name = "Ahmad" };
        var authority = new Authority { Id = "auth-xyz", Name = "Municipality Admin" };
        var reportPics = new List<ReportPic>
        {
            new() { PicId = 10, ReportId = id, PicUrl = "http://example.com/pic1.jpg" },
            new() { PicId = 11, ReportId = id, PicUrl = "http://example.com/pic2.jpg" }
        };

        return new Report
        {
            Id = id,
            Description = "Pothole on Main Street",
            Status = status,
            Latitude = 30.0444,
            Longitude = 31.2357,
            UploadedAt = DateTime.UtcNow.AddDays(-2),
            AssignedAt = DateTime.UtcNow.AddDays(-1),
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
