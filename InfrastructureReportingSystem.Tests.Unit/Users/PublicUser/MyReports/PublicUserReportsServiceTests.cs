using FluentAssertions;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.PublicUser.MyReports;
using InfraReportingSystem.Services.Users.PublicUser.MyReports;
using Moq;

namespace InfrastructureReportingSystem.Tests.Unit.Users.PublicUser.MyReports
{
    public class PublicUserReportsServiceTests
    {
        private readonly Mock<IPublicUserReportsRepository> _repositoryMock = new();

        [Fact]
        public async Task UpdateFixConfirmationStatusAsync_WhenReportNotFound_ReturnsErrorAndDoesNotUpdate()
        {
            // Arrange
            string userId = "user-1";
            int reportId = 123;

            _repositoryMock
                .Setup(r => r.GetUserReportByIdAsync(userId, reportId))
                .ReturnsAsync((Report?)null);

            var service = CreateService();

            // Action
            var result = await service.UpdateFixConfirmationStatusAsync(
                userId,
                reportId,
                ReportStatus.Resolved);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Report not found.");

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Report>()), Times.Never);
        }

        [Theory]
        [InlineData(ReportStatus.Submitted)]
        [InlineData(ReportStatus.InProgress)]
        [InlineData(ReportStatus.Resolved)]
        [InlineData(ReportStatus.FixRejected)]
        public async Task UpdateFixConfirmationStatusAsync_WhenReportIsNotPendingConfirmation_ReturnsErrorAndDoesNotUpdate(
            ReportStatus currentStatus)
        {
            // Arrange
            string userId = "user-1";
            int reportId = 123;
            var report = new Report { Id = reportId, SubmittedById = userId, Status = currentStatus };

            _repositoryMock
                .Setup(r => r.GetUserReportByIdAsync(userId, reportId))
                .ReturnsAsync(report);

            var service = CreateService();

            // Action
            var result = await service.UpdateFixConfirmationStatusAsync(
                userId,
                reportId,
                ReportStatus.Resolved);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Only reports waiting for confirmation can be updated.");

            report.Status.Should().Be(currentStatus);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Report>()), Times.Never);
        }

        [Fact]
        public async Task UpdateFixConfirmationStatusAsync_WhenConfirmingPendingReport_UpdatesToResolved()
        {
            // Arrange
            string userId = "user-1";
            int reportId = 123;
            var report = new Report { Id = reportId, SubmittedById = userId, Status = ReportStatus.PendingConfirmation };

            _repositoryMock
                .Setup(r => r.GetUserReportByIdAsync(userId, reportId))
                .ReturnsAsync(report);

            _repositoryMock
                .Setup(r => r.UpdateAsync(report))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Action
            var result = await service.UpdateFixConfirmationStatusAsync(
                userId,
                reportId,
                ReportStatus.Resolved);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Fix confirmed successfully.");

            report.Status.Should().Be(ReportStatus.Resolved);
            report.UpdatedAt.Should().NotBeNull();
            _repositoryMock.Verify(r => r.UpdateAsync(report), Times.Once);
        }

        [Fact]
        public async Task UpdateFixConfirmationStatusAsync_WhenRejectingPendingReport_UpdatesToFixRejected()
        {
            // Arrange
            string userId = "user-1";
            int reportId = 123;
            var report = new Report { Id = reportId, SubmittedById = userId, Status = ReportStatus.PendingConfirmation };

            _repositoryMock
                .Setup(r => r.GetUserReportByIdAsync(userId, reportId))
                .ReturnsAsync(report);

            _repositoryMock
                .Setup(r => r.UpdateAsync(report))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Action
            var result = await service.UpdateFixConfirmationStatusAsync(
                userId,
                reportId,
                ReportStatus.FixRejected);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Fix rejected successfully.");

            report.Status.Should().Be(ReportStatus.FixRejected);
            report.UpdatedAt.Should().NotBeNull();
            _repositoryMock.Verify(r => r.UpdateAsync(report), Times.Once);
        }

        private PublicUserReportsService CreateService()
        {
            return new PublicUserReportsService(_repositoryMock.Object);
        }
    }
}
