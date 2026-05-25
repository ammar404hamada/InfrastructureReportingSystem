using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Authority.MarkerPopup;
using InfraReportingSystem.Services.Authority.MarkerPopup;
using InfraReportingSystem.Shared.DTOs.Authority.MarkerPopup;
using Moq;
using Xunit;

namespace InfrastructureReportingSystem.Tests.Unit.Authority.MarkerPopup
{
    public class AuthorityMarkerPopupServiceTests
    {
        private readonly Mock<IAuthorityMarkerPopupRepository> _repositoryMock = new();

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public async Task GetReportPopupAsync_WhenReportIdIsZeroOrNegative_ReturnsNullImmediatelyWithoutQueryingRepo(int invalidId)
        {
            // Arrange
            var service = CreateService();

            // Action
            var result = await service.GetReportPopupAsync(invalidId);

            // Assert
            result.Should().BeNull();
            _repositoryMock.Verify(r => r.GetReportAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetReportPopupAsync_WhenReportNotFound_ReturnsNull()
        {
            // Arrange
            int reportId = 99;
            _repositoryMock
                .Setup(r => r.GetReportAsync(reportId))
                .ReturnsAsync((Report?)null);

            var service = CreateService();

            // Action
            var result = await service.GetReportPopupAsync(reportId);

            // Assert
            result.Should().BeNull();
            _repositoryMock.Verify(r => r.GetReportAsync(reportId), Times.Once);
        }

        [Fact]
        public async Task GetReportPopupAsync_WhenReportExists_MapsAllPropertiesCorrectly()
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
                Category = new Category { Id = 3, Name = "Lighting" },
                SubmittedBy = new User { Id = "user-1", Name = "Alice Smith" },
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
            var result = await service.GetReportPopupAsync(reportId);

            // Assert
            result.Should().NotBeNull();
            result!.ReportId.Should().Be(42);
            result.Status.Should().Be("InProgress");
            result.CategoryName.Should().Be("Lighting");
            result.CreatedAt.Should().Be(report.UploadedAt);
            result.Photos.Should().HaveCount(2);
            result.Photos.Should().Equal("url1.jpg", "url2.jpg");
            result.Description.Should().Be("Broken street lamp.");
            result.Latitude.Should().Be(30.1234);
            result.Longitude.Should().Be(31.5678);
            result.MapUrl.Should().Be("https://www.google.com/maps?q=30.1234,31.5678");
            result.ReporterName.Should().Be("Alice Smith");

            _repositoryMock.Verify(r => r.GetReportAsync(reportId), Times.Once);
        }

        [Fact]
        public async Task GetReportPopupAsync_WhenCategoryOrReporterAreNull_MapsEmptyStringForProperties()
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
                ReportPics = new List<ReportPic>()
            };

            _repositoryMock
                .Setup(r => r.GetReportAsync(reportId))
                .ReturnsAsync(report);

            var service = CreateService();

            // Action
            var result = await service.GetReportPopupAsync(reportId);

            // Assert
            result.Should().NotBeNull();
            result!.CategoryName.Should().BeEmpty();
            result.ReporterName.Should().BeEmpty();
            result.Photos.Should().BeEmpty();
        }

        [Fact]
        public async Task GetReportPopupAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            int reportId = 42;
            _repositoryMock
                .Setup(r => r.GetReportAsync(reportId))
                .ThrowsAsync(new InvalidOperationException("DB connection failed"));

            var service = CreateService();

            // Action & Assert
            Func<Task> action = async () => await service.GetReportPopupAsync(reportId);
            await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("DB connection failed");
        }

        private AuthorityMarkerPopupService CreateService()
        {
            return new AuthorityMarkerPopupService(_repositoryMock.Object);
        }
    }
}
