using FluentAssertions;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Shared;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Worker.TasksScreen;
using InfraReportingSystem.Services.Users.Worker.TasksScreen;
using InfraReportingSystem.Shared.DTOs.UserServices.Worker.TasksScreen;
using Moq;
using Xunit;

namespace InfrastructureReportingSystem.Tests.Unit.Users.Worker.TasksScreen;

public class WorkerTaskActionsServiceTests
{
    private readonly Mock<IWorkerTaskActionsRepository> _actionsRepositoryMock = new();
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock = new();

    [Fact]
    public async Task AcceptTaskAsync_WhenReportNotFound_ReturnsFailureAndNoStateChanges()
    {
        const int reportId = 45;
        const string workerId = "worker-123";

        _actionsRepositoryMock
            .Setup(repo => repo.GetByIdAsync(reportId))
            .ReturnsAsync((Report?)null);

        var service = CreateService();

        var result = await service.AcceptTaskAsync(reportId, workerId);

        result.Should().Be("Report not found.");

        _actionsRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Report>()), Times.Never);
        _auditLogRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<AuditLog>()), Times.Never);
    }

    [Fact]
    public async Task AcceptTaskAsync_WhenWorkerIsNotAssigned_ReturnsFailureAndNoStateChanges()
    {
        const int reportId = 45;
        const string workerId = "worker-123";
        var report = CreateReport(reportId, "different-worker", ReportStatus.Assigned);

        _actionsRepositoryMock
            .Setup(repo => repo.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        var service = CreateService();

        var result = await service.AcceptTaskAsync(reportId, workerId);

        result.Should().Be("You are not assigned to this task.");

        _actionsRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Report>()), Times.Never);
        _auditLogRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<AuditLog>()), Times.Never);
    }

    [Theory]
    [InlineData(ReportStatus.Submitted)]
    [InlineData(ReportStatus.InProgress)]
    [InlineData(ReportStatus.Resolved)]
    [InlineData(ReportStatus.Blocked)]
    [InlineData(ReportStatus.Rejected)]
    public async Task AcceptTaskAsync_WhenStatusIsNotAssigned_ReturnsFailureAndNoStateChanges(ReportStatus invalidStatus)
    {
        const int reportId = 45;
        const string workerId = "worker-123";
        var report = CreateReport(reportId, workerId, invalidStatus);

        _actionsRepositoryMock
            .Setup(repo => repo.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        var service = CreateService();

        var result = await service.AcceptTaskAsync(reportId, workerId);

        result.Should().Be("This task cannot be accepted in its current status.");

        _actionsRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Report>()), Times.Never);
        _auditLogRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<AuditLog>()), Times.Never);
    }

    [Fact]
    public async Task AcceptTaskAsync_WhenWorkerAlreadyHasActiveTask_ReturnsFailureAndNoStateChanges()
    {
        const int reportId = 45;
        const string workerId = "worker-123";
        var report = CreateReport(reportId, workerId, ReportStatus.Assigned);

        _actionsRepositoryMock
            .Setup(repo => repo.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        _actionsRepositoryMock
            .Setup(repo => repo.HasActiveTaskAsync(workerId))
            .ReturnsAsync(true);

        var service = CreateService();

        var result = await service.AcceptTaskAsync(reportId, workerId);

        result.Should().Be("You already have an active task.");

        _actionsRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Report>()), Times.Never);
        _auditLogRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<AuditLog>()), Times.Never);
    }

    [Fact]
    public async Task AcceptTaskAsync_WhenSuccessful_UpdatesStatusCreatesAuditLogAndReturnsSuccess()
    {
        const int reportId = 45;
        const string workerId = "worker-123";
        var report = CreateReport(reportId, workerId, ReportStatus.Assigned);

        _actionsRepositoryMock
            .Setup(repo => repo.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        _actionsRepositoryMock
            .Setup(repo => repo.HasActiveTaskAsync(workerId))
            .ReturnsAsync(false);

        var service = CreateService();

        var result = await service.AcceptTaskAsync(reportId, workerId);

        result.Should().Be("Task accepted successfully.");

        report.Status.Should().Be(ReportStatus.InProgress);

        _actionsRepositoryMock.Verify(repo => repo.UpdateAsync(report), Times.Once);

        _auditLogRepositoryMock.Verify(
            repo => repo.AddAsync(It.Is<AuditLog>(log =>
                log.UserId == workerId &&
                log.ActionType == AuditActionType.WorkerAcceptedTask &&
                log.EntityName == "Report" &&
                log.EntityId == reportId.ToString() &&
                log.Details == $"Worker {workerId} accepted task for report {reportId}."
            )),
            Times.Once);
    }

    [Fact]
    public async Task RejectTaskAsync_WhenReportNotFound_ReturnsFailureAndNoStateChanges()
    {
        const int reportId = 45;
        const string workerId = "worker-123";
        var dto = new RejectTaskDto { Reason = "Not my specialty" };

        _actionsRepositoryMock
            .Setup(repo => repo.GetByIdAsync(reportId))
            .ReturnsAsync((Report?)null);

        var service = CreateService();

        var result = await service.RejectTaskAsync(reportId, workerId, dto);

        result.Should().Be("Report not found.");

        _actionsRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Report>()), Times.Never);
        _auditLogRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<AuditLog>()), Times.Never);
    }

    [Fact]
    public async Task RejectTaskAsync_WhenWorkerIsNotAssigned_ReturnsFailureAndNoStateChanges()
    {
        const int reportId = 45;
        const string workerId = "worker-123";
        var dto = new RejectTaskDto { Reason = "Not my specialty" };
        var report = CreateReport(reportId, "different-worker", ReportStatus.Assigned);

        _actionsRepositoryMock
            .Setup(repo => repo.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        var service = CreateService();

        var result = await service.RejectTaskAsync(reportId, workerId, dto);

        result.Should().Be("You are not assigned to this task.");

        _actionsRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Report>()), Times.Never);
        _auditLogRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<AuditLog>()), Times.Never);
    }

    [Theory]
    [InlineData(ReportStatus.Submitted)]
    [InlineData(ReportStatus.InProgress)]
    [InlineData(ReportStatus.Resolved)]
    [InlineData(ReportStatus.Blocked)]
    [InlineData(ReportStatus.Rejected)]
    public async Task RejectTaskAsync_WhenStatusIsNotAssigned_ReturnsFailureAndNoStateChanges(ReportStatus invalidStatus)
    {
        const int reportId = 45;
        const string workerId = "worker-123";
        var dto = new RejectTaskDto { Reason = "Not my specialty" };
        var report = CreateReport(reportId, workerId, invalidStatus);

        _actionsRepositoryMock
            .Setup(repo => repo.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        var service = CreateService();

        var result = await service.RejectTaskAsync(reportId, workerId, dto);

        result.Should().Be("This task cannot be rejected in its current status.");

        _actionsRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Report>()), Times.Never);
        _auditLogRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<AuditLog>()), Times.Never);
    }

    [Fact]
    public async Task RejectTaskAsync_WhenSuccessful_UpdatesStatusSetsReasonCreatesAuditLogAndReturnsSuccess()
    {
        const int reportId = 45;
        const string workerId = "worker-123";
        var dto = new RejectTaskDto { Reason = "Out of materials" };
        var report = CreateReport(reportId, workerId, ReportStatus.Assigned);

        _actionsRepositoryMock
            .Setup(repo => repo.GetByIdAsync(reportId))
            .ReturnsAsync(report);

        var service = CreateService();

        var result = await service.RejectTaskAsync(reportId, workerId, dto);

        result.Should().Be("Task rejected successfully.");

        report.Status.Should().Be(ReportStatus.Rejected);
        report.RejectionReason.Should().Be(dto.Reason);

        _actionsRepositoryMock.Verify(repo => repo.UpdateAsync(report), Times.Once);

        _auditLogRepositoryMock.Verify(
            repo => repo.AddAsync(It.Is<AuditLog>(log =>
                log.UserId == workerId &&
                log.ActionType == AuditActionType.WorkerRejectedTask &&
                log.EntityName == "Report" &&
                log.EntityId == reportId.ToString() &&
                log.Details == $"Worker {workerId} rejected task for report {reportId}. Reason: {dto.Reason}"
            )),
            Times.Once);
    }

    private WorkerTaskActionsService CreateService()
    {
        return new WorkerTaskActionsService(
            _actionsRepositoryMock.Object,
            _auditLogRepositoryMock.Object);
    }

    private static Report CreateReport(int id, string workerId, ReportStatus status)
    {
        return new Report
        {
            Id = id,
            Description = "Pothole on Main Street",
            Latitude = 30.0444,
            Longitude = 31.2357,
            Status = status,
            AssignedWorkerId = workerId,
            UploadedAt = DateTime.UtcNow
        };
    }
}
