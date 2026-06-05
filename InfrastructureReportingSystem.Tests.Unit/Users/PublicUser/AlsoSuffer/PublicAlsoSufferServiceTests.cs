using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.PublicUser.AlsoSuffer;
using InfraReportingSystem.Services.Users.PublicUser.AlsoSuffer;
using InfraReportingSystem.Shared.DTOs.UserServices.PublicUser.AlsoSuffer;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace InfrastructureReportingSystem.Tests.Unit.Users.PublicUser.AlsoSuffer
{
    public class PublicAlsoSufferServiceTests
    {
        private readonly Mock<IPublicAlsoSufferRepository> _repositoryMock = new();

        [Fact]
        public async Task ConfirmAlsoSufferAsync_WhenReportNotFound_ReturnsErrorAndNoStateChanges()
        {
            // Arrange
            int reportId = 123;
            string userId = "user-1";

            _repositoryMock
                .Setup(r => r.GetReportStatusAsync(reportId))
                .ReturnsAsync((ReportStatus?)null);

            var service = CreateService();

            // Action
            var result = await service.ConfirmAlsoSufferAsync(reportId, userId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Report not found.");

            _repositoryMock.Verify(r => r.AlreadyAffectedAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<ReportAffectedUser>()), Times.Never);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Theory]
        [InlineData(ReportStatus.Submitted)]
        [InlineData(ReportStatus.InProgress)]
        [InlineData(ReportStatus.Blocked)]
        [InlineData(ReportStatus.Resolved)]
        public async Task ConfirmAlsoSufferAsync_WhenStatusIsAllowedAndNotAlreadyConfirmed_AddsRecordSavesAndReturnsSuccess(ReportStatus allowedStatus)
        {
            // Arrange
            int reportId = 123;
            string userId = "user-1";

            _repositoryMock
                .Setup(r => r.GetReportStatusAsync(reportId))
                .ReturnsAsync(allowedStatus);

            _repositoryMock
                .Setup(r => r.AlreadyAffectedAsync(reportId, userId))
                .ReturnsAsync(false);

            _repositoryMock
                .Setup(r => r.AddAsync(It.IsAny<ReportAffectedUser>()))
                .Returns(Task.CompletedTask);

            _repositoryMock
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Action
            var result = await service.ConfirmAlsoSufferAsync(reportId, userId);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Your confirmation has been recorded.");

            _repositoryMock.Verify(
                r => r.AddAsync(It.Is<ReportAffectedUser>(u =>
                    u.ReportId == reportId &&
                    u.UserId == userId &&
                    u.CreatedAt <= DateTime.UtcNow
                )),
                Times.Once);

            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Theory]
        [InlineData(ReportStatus.Assigned)]
        [InlineData(ReportStatus.OnHold)]
        [InlineData(ReportStatus.PendingConfirmation)]
        [InlineData(ReportStatus.Rejected)]
        [InlineData(ReportStatus.FixRejected)]
        public async Task ConfirmAlsoSufferAsync_WhenStatusIsNotAllowed_ReturnsErrorAndNoStateChanges(ReportStatus invalidStatus)
        {
            // Arrange
            int reportId = 123;
            string userId = "user-1";

            _repositoryMock
                .Setup(r => r.GetReportStatusAsync(reportId))
                .ReturnsAsync(invalidStatus);

            var service = CreateService();

            // Action
            var result = await service.ConfirmAlsoSufferAsync(reportId, userId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("This report is no longer open for confirmations.");

            _repositoryMock.Verify(r => r.AlreadyAffectedAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<ReportAffectedUser>()), Times.Never);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task ConfirmAlsoSufferAsync_WhenUserAlreadyConfirmed_ReturnsErrorAndNoStateChanges()
        {
            // Arrange
            int reportId = 123;
            string userId = "user-1";

            _repositoryMock
                .Setup(r => r.GetReportStatusAsync(reportId))
                .ReturnsAsync(ReportStatus.Submitted);

            _repositoryMock
                .Setup(r => r.AlreadyAffectedAsync(reportId, userId))
                .ReturnsAsync(true);

            var service = CreateService();

            // Action
            var result = await service.ConfirmAlsoSufferAsync(reportId, userId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("You already confirmed this issue.");

            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<ReportAffectedUser>()), Times.Never);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task ConfirmAlsoSufferAsync_WhenConcurrencyExceptionOnSave_ReturnsFriendlyError()
        {
            // Arrange
            int reportId = 123;
            string userId = "user-1";

            _repositoryMock
                .Setup(r => r.GetReportStatusAsync(reportId))
                .ReturnsAsync(ReportStatus.Submitted);

            _repositoryMock
                .Setup(r => r.AlreadyAffectedAsync(reportId, userId))
                .ReturnsAsync(false);

            _repositoryMock
                .Setup(r => r.AddAsync(It.IsAny<ReportAffectedUser>()))
                .Returns(Task.CompletedTask);

            _repositoryMock
                .Setup(r => r.SaveChangesAsync())
                .ThrowsAsync(new DbUpdateException("Duplicate key violation"));

            var service = CreateService();

            // Action
            var result = await service.ConfirmAlsoSufferAsync(reportId, userId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("You already confirmed this issue.");

            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<ReportAffectedUser>()), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ConfirmAlsoSufferAsync_WhenRepositoryThrowsGeneralException_PropagatesException()
        {
            // Arrange
            int reportId = 123;
            string userId = "user-1";

            _repositoryMock
                .Setup(r => r.GetReportStatusAsync(reportId))
                .ThrowsAsync(new InvalidOperationException("DB connection error"));

            var service = CreateService();

            // Action & Assert
            Func<Task> action = async () => await service.ConfirmAlsoSufferAsync(reportId, userId);
            await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("DB connection error");
        }

        private PublicAlsoSufferService CreateService()
        {
            return new PublicAlsoSufferService(_repositoryMock.Object);
        }
    }
}
