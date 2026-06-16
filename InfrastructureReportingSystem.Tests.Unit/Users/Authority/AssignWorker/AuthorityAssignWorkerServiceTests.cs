using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Shared;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Authority.AssignWorker;
using InfraReportingSystem.ServiceAbstractions.Shared.Email;
using InfraReportingSystem.Services.Users.Authority.AssignWorker;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace InfrastructureReportingSystem.Tests.Unit.Users.Authority.AssignWorker
{
    public class AuthorityAssignWorkerServiceTests
    {
        private readonly Mock<IAuthorityAssignWorkerRepository> _repositoryMock = new();
        private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock = new();
        private readonly Mock<IEmailService> _emailServiceMock = new();
        private readonly Mock<UserManager<User>> _userManagerMock = CreateUserManagerMock();
        private readonly Mock<ILogger<AuthorityAssignWorkerService>> _loggerMock = new();

        [Fact]
        public async Task AssignWorkerAsync_WhenReportNotFound_ReturnsErrorAndNoStateChanges()
        {
            // Arrange
            int reportId = 123;
            string workerId = "worker-1";
            string authorityId = "auth-1";

            _repositoryMock
                .Setup(r => r.GetByIdAsync(reportId))
                .ReturnsAsync((Report?)null);

            var service = CreateService();

            // Action
            var result = await service.AssignWorkerAsync(reportId, workerId, authorityId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Report not found.");

            _userManagerMock.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Never);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
            _auditLogRepositoryMock.Verify(r => r.AddAsync(It.IsAny<AuditLog>()), Times.Never);
            _emailServiceMock.Verify(m => m.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData(ReportStatus.Assigned)]
        [InlineData(ReportStatus.InProgress)]
        [InlineData(ReportStatus.Resolved)]
        [InlineData(ReportStatus.Blocked)]
        [InlineData(ReportStatus.PendingConfirmation)]
        [InlineData(ReportStatus.Rejected)]
        [InlineData(ReportStatus.FixRejected)]
        [InlineData(ReportStatus.OnHold)]
        public async Task AssignWorkerAsync_WhenReportStatusIsNotSubmitted_ReturnsErrorAndNoStateChanges(ReportStatus invalidStatus)
        {
            // Arrange
            int reportId = 123;
            string workerId = "worker-1";
            string authorityId = "auth-1";
            var report = new Report { Id = reportId, Status = invalidStatus };

            _repositoryMock
                .Setup(r => r.GetByIdAsync(reportId))
                .ReturnsAsync(report);

            var service = CreateService();

            // Action
            var result = await service.AssignWorkerAsync(reportId, workerId, authorityId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be($"Report cannot be assigned. Current status is '{invalidStatus}'. Only Submitted reports can be assigned.");

            _userManagerMock.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Never);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
            _auditLogRepositoryMock.Verify(r => r.AddAsync(It.IsAny<AuditLog>()), Times.Never);
            _emailServiceMock.Verify(m => m.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AssignWorkerAsync_WhenWorkerNotFound_ReturnsErrorAndNoStateChanges()
        {
            // Arrange
            int reportId = 123;
            string workerId = "worker-1";
            string authorityId = "auth-1";
            var report = new Report { Id = reportId, Status = ReportStatus.Submitted };

            _repositoryMock
                .Setup(r => r.GetByIdAsync(reportId))
                .ReturnsAsync(report);

            _userManagerMock
                .Setup(m => m.FindByIdAsync(workerId))
                .ReturnsAsync((User?)null);

            var service = CreateService();

            // Action
            var result = await service.AssignWorkerAsync(reportId, workerId, authorityId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Worker not found.");

            _userManagerMock.Verify(m => m.GetRolesAsync(It.IsAny<User>()), Times.Never);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
            _auditLogRepositoryMock.Verify(r => r.AddAsync(It.IsAny<AuditLog>()), Times.Never);
            _emailServiceMock.Verify(m => m.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData(UserStatus.Inactive)]
        [InlineData(UserStatus.Suspended)]
        public async Task AssignWorkerAsync_WhenWorkerNotActive_ReturnsErrorAndNoStateChanges(UserStatus inactiveStatus)
        {
            // Arrange
            int reportId = 123;
            string workerId = "worker-1";
            string authorityId = "auth-1";
            var report = new Report { Id = reportId, Status = ReportStatus.Submitted };
            var worker = new User { Id = workerId, Status = inactiveStatus };

            _repositoryMock
                .Setup(r => r.GetByIdAsync(reportId))
                .ReturnsAsync(report);

            _userManagerMock
                .Setup(m => m.FindByIdAsync(workerId))
                .ReturnsAsync(worker);

            var service = CreateService();

            // Action
            var result = await service.AssignWorkerAsync(reportId, workerId, authorityId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Worker is not active and cannot be assigned.");

            _userManagerMock.Verify(m => m.GetRolesAsync(It.IsAny<User>()), Times.Never);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
            _auditLogRepositoryMock.Verify(r => r.AddAsync(It.IsAny<AuditLog>()), Times.Never);
            _emailServiceMock.Verify(m => m.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AssignWorkerAsync_WhenUserDoesNotHaveWorkerRole_ReturnsErrorAndNoStateChanges()
        {
            // Arrange
            int reportId = 123;
            string workerId = "worker-1";
            string authorityId = "auth-1";
            var report = new Report { Id = reportId, Status = ReportStatus.Submitted };
            var worker = new User { Id = workerId, Status = UserStatus.Active };

            _repositoryMock
                .Setup(r => r.GetByIdAsync(reportId))
                .ReturnsAsync(report);

            _userManagerMock
                .Setup(m => m.FindByIdAsync(workerId))
                .ReturnsAsync(worker);

            _userManagerMock
                .Setup(m => m.GetRolesAsync(worker))
                .ReturnsAsync(new List<string> { "PublicUser" });

            var service = CreateService();

            // Action
            var result = await service.AssignWorkerAsync(reportId, workerId, authorityId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("The specified user is not a worker.");

            _repositoryMock.Verify(r => r.HasActiveTaskAsync(It.IsAny<string>()), Times.Never);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
            _auditLogRepositoryMock.Verify(r => r.AddAsync(It.IsAny<AuditLog>()), Times.Never);
            _emailServiceMock.Verify(m => m.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AssignWorkerAsync_WhenWorkerHasActiveTask_ReturnsErrorAndNoStateChanges()
        {
            // Arrange
            int reportId = 123;
            string workerId = "worker-1";
            string authorityId = "auth-1";
            var report = new Report { Id = reportId, Status = ReportStatus.Submitted };
            var worker = new User { Id = workerId, Status = UserStatus.Active };

            _repositoryMock
                .Setup(r => r.GetByIdAsync(reportId))
                .ReturnsAsync(report);

            _userManagerMock
                .Setup(m => m.FindByIdAsync(workerId))
                .ReturnsAsync(worker);

            _userManagerMock
                .Setup(m => m.GetRolesAsync(worker))
                .ReturnsAsync(new List<string> { "Worker" });

            _repositoryMock
                .Setup(r => r.HasActiveTaskAsync(workerId))
                .ReturnsAsync(true);

            var service = CreateService();

            // Action
            var result = await service.AssignWorkerAsync(reportId, workerId, authorityId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Worker already has an active task in progress.");

            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
            _auditLogRepositoryMock.Verify(r => r.AddAsync(It.IsAny<AuditLog>()), Times.Never);
            _emailServiceMock.Verify(m => m.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AssignWorkerAsync_WhenValidInput_AssignsWorkerPersistsChangesAddsAuditLogSendsEmailAndReturnsSuccess()
        {
            // Arrange
            int reportId = 123;
            string workerId = "worker-1";
            string authorityId = "auth-1";
            var report = new Report
            {
                Id = reportId,
                Status = ReportStatus.Submitted,
                Description = "Pipe burst in street.",
                Latitude = 30.0444,
                Longitude = 31.2357
            };
            var worker = new User
            {
                Id = workerId,
                Name = "John Doe",
                Email = "john@example.com",
                Status = UserStatus.Active
            };

            _repositoryMock
                .Setup(r => r.GetByIdAsync(reportId))
                .ReturnsAsync(report);

            _userManagerMock
                .Setup(m => m.FindByIdAsync(workerId))
                .ReturnsAsync(worker);

            _userManagerMock
                .Setup(m => m.GetRolesAsync(worker))
                .ReturnsAsync(new List<string> { "Worker" });

            _repositoryMock
                .Setup(r => r.HasActiveTaskAsync(workerId))
                .ReturnsAsync(false);

            _auditLogRepositoryMock
                .Setup(a => a.AddAsync(It.IsAny<AuditLog>()))
                .Returns(Task.CompletedTask);

            _repositoryMock
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _emailServiceMock
                .Setup(e => e.SendEmailAsync(worker.Email, It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Action
            var result = await service.AssignWorkerAsync(reportId, workerId, authorityId);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Worker assigned successfully.");

            report.Status.Should().Be(ReportStatus.Assigned);
            report.AssignedWorkerId.Should().Be(workerId);
            report.AssignedByAuthorityId.Should().Be(authorityId);
            report.AssignedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            report.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            _auditLogRepositoryMock.Verify(
                a => a.AddAsync(It.Is<AuditLog>(log =>
                    log.UserId == authorityId &&
                    log.ActionType == AuditActionType.ReportAssigned &&
                    log.EntityName == "Report" &&
                    log.EntityId == reportId.ToString() &&
                    log.Details == $"Authority {authorityId} assigned report {reportId} to worker {workerId} (John Doe)." &&
                    log.Timestamp <= DateTime.UtcNow
                )),
                Times.Once);

            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            _emailServiceMock.Verify(
                e => e.SendEmailAsync(
                    "john@example.com",
                    It.Is<string>(subject =>
                        subject.Contains("New Task Assigned") &&
                        subject.Contains("Infrastructure Reporting System")
                    ),
                    It.Is<string>(body =>
                        body.Contains("John Doe") &&
                        body.Contains("123") &&
                        body.Contains("Pipe burst in street.") &&
                        body.Contains("30.0444, 31.2357")
                    )
                ),
                Times.Once);
        }

        [Fact]
        public async Task AssignWorkerAsync_WhenEmailFails_StillAssignsWorkerAndReturnsSuccess()
        {
            // Arrange
            int reportId = 123;
            string workerId = "worker-1";
            string authorityId = "auth-1";
            var report = new Report { Id = reportId, Status = ReportStatus.Submitted, Description = "Task desc" };
            var worker = new User { Id = workerId, Name = "John Doe", Email = "john@example.com", Status = UserStatus.Active };

            _repositoryMock
                .Setup(r => r.GetByIdAsync(reportId))
                .ReturnsAsync(report);

            _userManagerMock
                .Setup(m => m.FindByIdAsync(workerId))
                .ReturnsAsync(worker);

            _userManagerMock
                .Setup(m => m.GetRolesAsync(worker))
                .ReturnsAsync(new List<string> { "Worker" });

            _repositoryMock
                .Setup(r => r.HasActiveTaskAsync(workerId))
                .ReturnsAsync(false);

            _auditLogRepositoryMock
                .Setup(a => a.AddAsync(It.IsAny<AuditLog>()))
                .Returns(Task.CompletedTask);

            _repositoryMock
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _emailServiceMock
                .Setup(e => e.SendEmailAsync(worker.Email, It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("SMTP failure"));

            var service = CreateService();

            // Action
            var result = await service.AssignWorkerAsync(reportId, workerId, authorityId);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Worker assigned successfully.");

            report.Status.Should().Be(ReportStatus.Assigned);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _auditLogRepositoryMock.Verify(a => a.AddAsync(It.IsAny<AuditLog>()), Times.Once);
        }

        [Fact]
        public async Task AssignWorkerAsync_WhenSaveThrows_PropagatesException()
        {
            // Arrange
            int reportId = 123;
            string workerId = "worker-1";
            string authorityId = "auth-1";
            var report = new Report { Id = reportId, Status = ReportStatus.Submitted };
            var worker = new User { Id = workerId, Name = "John Doe", Email = "john@example.com", Status = UserStatus.Active };

            _repositoryMock
                .Setup(r => r.GetByIdAsync(reportId))
                .ReturnsAsync(report);

            _userManagerMock
                .Setup(m => m.FindByIdAsync(workerId))
                .ReturnsAsync(worker);

            _userManagerMock
                .Setup(m => m.GetRolesAsync(worker))
                .ReturnsAsync(new List<string> { "Worker" });

            _repositoryMock
                .Setup(r => r.HasActiveTaskAsync(workerId))
                .ReturnsAsync(false);

            _auditLogRepositoryMock
                .Setup(a => a.AddAsync(It.IsAny<AuditLog>()))
                .Returns(Task.CompletedTask);

            _repositoryMock
                .Setup(r => r.SaveChangesAsync())
                .ThrowsAsync(new InvalidOperationException("DB error"));

            var service = CreateService();

            // Action & Assert
            Func<Task> action = async () => await service.AssignWorkerAsync(reportId, workerId, authorityId);
            await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("DB error");

            _emailServiceMock.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        private AuthorityAssignWorkerService CreateService()
        {
            return new AuthorityAssignWorkerService(
                _repositoryMock.Object,
                _auditLogRepositoryMock.Object,
                _emailServiceMock.Object,
                _userManagerMock.Object,
                _loggerMock.Object);
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
}
