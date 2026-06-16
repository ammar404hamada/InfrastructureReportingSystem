using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.PublicUser.MarkerPopup;
using InfraReportingSystem.Services.Users.PublicUser.MarkerPopup;
using InfraReportingSystem.Shared.DTOs.UserServices.PublicUser.MarkerPopup;
using Moq;
using Xunit;

namespace InfrastructureReportingSystem.Tests.Unit.Users.PublicUser.MarkerPopup
{
    public class PublicMarkerPopupServiceTests
    {
        private readonly Mock<IPublicMarkerPopupRepository> _repositoryMock = new();

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public async Task GetReportDetailsAsync_WhenReportIdIsZeroOrNegative_ReturnsNullImmediatelyWithoutQueryingRepo(int invalidId)
        {
            // Arrange
            var service = CreateService();

            // Action
            var result = await service.GetReportDetailsAsync(invalidId);

            // Assert
            result.Should().BeNull();
            _repositoryMock.Verify(r => r.GetReportAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetReportDetailsAsync_WhenReportNotFound_ReturnsNull()
        {
            // Arrange
            int reportId = 99;
            _repositoryMock
                .Setup(r => r.GetReportAsync(reportId))
                .ReturnsAsync((Report?)null);

            var service = CreateService();

            // Action
            var result = await service.GetReportDetailsAsync(reportId);

            // Assert
            result.Should().BeNull();
            _repositoryMock.Verify(r => r.GetReportAsync(reportId), Times.Once);
        }

        [Fact]
        public async Task GetReportDetailsAsync_WhenReportExists_MapsAllPropertiesCorrectly()
        {
            // Arrange
            int reportId = 42;
            var report = new Report
            {
                Id = reportId,
                Status = ReportStatus.InProgress,
                Latitude = 30.1234,
                Longitude = 31.5678,
                Description = "Broken street lamp.",
                UploadedAt = new DateTime(2026, 5, 25, 14, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 5, 26, 9, 30, 0, DateTimeKind.Utc),
                Category = new Category { Id = 3, Name = "Lighting" },
                SubmittedBy = new User { Id = "user-1", Name = "Alice Smith" },
                AssignedWorkerId = "worker-1",
                AssignedWorker = new InfraReportingSystem.Domain.Entities.Worker {Id = "worker-1",Name = "Worker Bob"},
                ReportPics = new List<ReportPic>
                {
                    new ReportPic { PicId = 1, PicUrl = "url1.jpg" },
                    new ReportPic { PicId = 2, PicUrl = "url2.jpg" }
                }
            };

            _repositoryMock
                .Setup(r => r.GetReportAsync(reportId))
                .ReturnsAsync(report);

            var service = CreateService();

            // Action
            var result = await service.GetReportDetailsAsync(reportId);

            // Assert
            result.Should().NotBeNull();
            result!.ReportId.Should().Be(42);
            result.Title.Should().Be("Broken street lamp.");
            result.Description.Should().Be("Broken street lamp.");
            result.Address.Should().BeNull();
            result.Latitude.Should().Be(30.1234);
            result.Longitude.Should().Be(31.5678);
            result.CategoryName.Should().Be("Lighting");
            result.Status.Should().Be("InProgress");
            result.CreatedAt.Should().Be(report.UploadedAt);
            result.UpdatedAt.Should().Be(report.UpdatedAt);

            result.Photos.Should().HaveCount(2);
            result.Photos.Select(p => p.ImageUrl).Should().Equal("url1.jpg", "url2.jpg");

            result.ReportedBy.Should().NotBeNull();
            result.ReportedBy.UserId.Should().Be("user-1");
            result.ReportedBy.FullName.Should().Be("Alice Smith");

            result.AssignedWorker.Should().NotBeNull();
            result.AssignedWorker!.UserId.Should().Be("worker-1");
            result.AssignedWorker.FullName.Should().Be("Worker Bob");

            _repositoryMock.Verify(r => r.GetReportAsync(reportId), Times.Once);
        }

        [Fact]
        public async Task GetReportDetailsAsync_WhenCategoryOrReporterOrWorkerAreNull_MapsEmptyStringOrNullForProperties()
        {
            // Arrange
            int reportId = 42;
            var report = new Report
            {
                Id = reportId,
                Status = ReportStatus.Submitted,
                Latitude = 30.0,
                Longitude = 31.0,
                Description = "No category report",
                UploadedAt = DateTime.UtcNow,
                Category = null!,
                SubmittedBy = null!,
                AssignedWorkerId = null,
                AssignedWorker = null,
                ReportPics = new List<ReportPic>()
            };

            _repositoryMock
                .Setup(r => r.GetReportAsync(reportId))
                .ReturnsAsync(report);

            var service = CreateService();

            // Action
            var result = await service.GetReportDetailsAsync(reportId);

            // Assert
            result.Should().NotBeNull();
            result!.CategoryName.Should().BeEmpty();
            result.ReportedBy.UserId.Should().BeEmpty();
            result.ReportedBy.FullName.Should().Be("Unknown");
            result.AssignedWorker.Should().BeNull();
            result.Photos.Should().BeEmpty();
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("   ", "")]
        [InlineData("12345678901234567890123456789012345678901234567890123456789012345678901234567890", "12345678901234567890123456789012345678901234567890123456789012345678901234567890")]
        [InlineData("123456789012345678901234567890123456789012345678901234567890123456789012345678901", "12345678901234567890123456789012345678901234567890123456789012345678901234567890…")]
        public async Task GetReportDetailsAsync_ChecksTitleEllipsisTruncation(string? description, string expectedTitle)
        {
            // Arrange
            var report = new Report
            {
                Id = 1,
                Description = description!,
                UploadedAt = DateTime.UtcNow,
                Status = ReportStatus.Submitted,
                ReportPics = new List<ReportPic>()
            };

            _repositoryMock
                .Setup(r => r.GetReportAsync(1))
                .ReturnsAsync(report);

            var service = CreateService();

            // Action
            var result = await service.GetReportDetailsAsync(1);

            // Assert
            result!.Title.Should().Be(expectedTitle);
        }

        [Fact]
        public async Task GetReportDetailsAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            int reportId = 42;
            _repositoryMock
                .Setup(r => r.GetReportAsync(reportId))
                .ThrowsAsync(new InvalidOperationException("DB connection failed"));

            var service = CreateService();

            // Action & Assert
            Func<Task> action = async () => await service.GetReportDetailsAsync(reportId);
            await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("DB connection failed");
        }

        private PublicMarkerPopupService CreateService()
        {
            return new PublicMarkerPopupService(_repositoryMock.Object);
        }
    }
}
