using FluentAssertions;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Shared;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Worker.CurrentTaskScreen;
using InfraReportingSystem.Services.Users.Worker.CurrentTaskScreen;
using InfraReportingSystem.Shared.DTOs.UserServices.Worker.CurrentTaskScreen;
using Moq;
using Xunit;

namespace InfrastructureReportingSystem.Tests.Unit.Users.Worker.CurrentTaskScreen;

public class WorkerCurrentTaskActionsServiceTests
{
    private readonly Mock<IWorkerCurrentTaskActionsRepository> _repositoryMock = new();
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock = new();

    [Fact]
    public async Task MarkAsFixedAsync_WhenNoActiveTaskFound_ReturnsFailureAndNoStateChanges()
    {
        const string workerId = "worker-123";
        var dto = new MarkFixedDto { Comment = "Fixed it!" };

        _repositoryMock
            .Setup(repo => repo.GetCurrentTaskAsync(workerId))
            .ReturnsAsync((Report?)null);

        var service = CreateService();

        var result = await service.MarkAsFixedAsync(workerId, dto);

        result.Should().Be("No active task found to mark as fixed.");

        _repositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Report>()), Times.Never);
        _auditLogRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<AuditLog>()), Times.Never);
    }

    [Theory]
    [InlineData(null, "None")]
    [InlineData("", "None")]
    [InlineData("   ", "None")]
    [InlineData("Fixed the wiring", "Fixed the wiring")]
    [InlineData("  Fixed the wiring  ", "Fixed the wiring")]
    public async Task MarkAsFixedAsync_WhenActiveTaskExists_UpdatesStatusCreatesAuditLogAndReturnsSuccess(
        string? inputComment, 
        string expectedCommentInDetails)
    {
        const string workerId = "worker-123";
        var dto = new MarkFixedDto { Comment = inputComment };
        var report = CreateReport(45, workerId, ReportStatus.InProgress);

        _repositoryMock
            .Setup(repo => repo.GetCurrentTaskAsync(workerId))
            .ReturnsAsync(report);

        var service = CreateService();

        var result = await service.MarkAsFixedAsync(workerId, dto);

        result.Should().Be("Task marked as fixed. Waiting for user confirmation.");

        report.Status.Should().Be(ReportStatus.PendingConfirmation);

        _repositoryMock.Verify(repo => repo.UpdateAsync(report), Times.Once);

        _auditLogRepositoryMock.Verify(
            repo => repo.AddAsync(It.Is<AuditLog>(log =>
                log.UserId == workerId &&
                log.ActionType == AuditActionType.WorkerMarkedTaskAsFixed &&
                log.EntityName == "Report" &&
                log.EntityId == report.Id.ToString() &&
                log.Details == $"Status changed from InProgress to PendingConfirmation. Comment: {expectedCommentInDetails}"
            )),
            Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task MarkAsBlockedAsync_WhenReasonIsMissing_ReturnsFailureAndNoStateChanges(string? missingReason)
    {
        const string workerId = "worker-123";
        var dto = new MarkBlockedDto { Reason = missingReason! };

        var service = CreateService();

        var result = await service.MarkAsBlockedAsync(workerId, dto);

        result.Should().Be("Reason is required when blocking a task.");

        _repositoryMock.Verify(repo => repo.GetCurrentTaskAsync(It.IsAny<string>()), Times.Never);
        _repositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Report>()), Times.Never);
        _auditLogRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<AuditLog>()), Times.Never);
    }

    [Fact]
    public async Task MarkAsBlockedAsync_WhenNoActiveTaskFound_ReturnsFailureAndNoStateChanges()
    {
        const string workerId = "worker-123";
        var dto = new MarkBlockedDto { Reason = "Traffic was blocked" };

        _repositoryMock
            .Setup(repo => repo.GetCurrentTaskAsync(workerId))
            .ReturnsAsync((Report?)null);

        var service = CreateService();

        var result = await service.MarkAsBlockedAsync(workerId, dto);

        result.Should().Be("No active task found to mark as blocked.");

        _repositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Report>()), Times.Never);
        _auditLogRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<AuditLog>()), Times.Never);
    }

    [Fact]
    public async Task MarkAsBlockedAsync_WhenActiveTaskExists_UpdatesStatusSetsReasonCreatesAuditLogAndReturnsSuccess()
    {
        const string workerId = "worker-123";
        var dto = new MarkBlockedDto { Reason = "  Heavy equipment required  " };
        var report = CreateReport(45, workerId, ReportStatus.InProgress);

        _repositoryMock
            .Setup(repo => repo.GetCurrentTaskAsync(workerId))
            .ReturnsAsync(report);

        var service = CreateService();

        var result = await service.MarkAsBlockedAsync(workerId, dto);

        result.Should().Be("Task marked as blocked. Authority will review.");

        report.Status.Should().Be(ReportStatus.Blocked);
        report.RejectionReason.Should().Be("Heavy equipment required");

        _repositoryMock.Verify(repo => repo.UpdateAsync(report), Times.Once);

        _auditLogRepositoryMock.Verify(
            repo => repo.AddAsync(It.Is<AuditLog>(log =>
                log.UserId == workerId &&
                log.ActionType == AuditActionType.ReportBlocked &&
                log.EntityName == "Report" &&
                log.EntityId == report.Id.ToString() &&
                log.Details == "Status changed from InProgress to Blocked. Reason: Heavy equipment required"
            )),
            Times.Once);
    }

    private WorkerCurrentTaskActionsService CreateService()
    {
        return new WorkerCurrentTaskActionsService(
            _repositoryMock.Object,
            _auditLogRepositoryMock.Object);
    }

    private static Report CreateReport(int id, string workerId, ReportStatus status)
    {
        return new Report
        {
            Id = id,
            Description = "Test infrastructure issue",
            Latitude = 30.0444,
            Longitude = 31.2357,
            Status = status,
            AssignedWorkerId = workerId,
            UploadedAt = DateTime.UtcNow
        };
    }
}
