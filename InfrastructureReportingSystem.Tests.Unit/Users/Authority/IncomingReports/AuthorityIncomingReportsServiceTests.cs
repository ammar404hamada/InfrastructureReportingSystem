using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Authority.IncomingReports;
using InfraReportingSystem.Services.Users.Authority.IncomingReports;
using InfraReportingSystem.Shared.DTOs.UserServices.Authority.IncomingReports;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace InfrastructureReportingSystem.Tests.Unit.Users.Authority.IncomingReports
{
    public class AuthorityIncomingReportsServiceTests
    {
        private readonly Mock<IAuthorityIncomingReportsRepository> _repositoryMock = new();
        private readonly Mock<ILogger<AuthorityIncomingReportsService>> _loggerMock = new();

        [Fact]
        public async Task GetIncomingReportsAsync_WhenNoReportsFound_ReturnsEmptyResultWithMessage()
        {
            // Arrange
            string? search = "trash";
            string? sortBy = "updatedat";
            string? sortDirection = "asc";
            int pageNumber = 1;
            int pageSize = 10;

            _repositoryMock
                .Setup(r => r.GetIncomingReportsAsync(search, sortBy, sortDirection, pageNumber, pageSize))
                .ReturnsAsync((new List<Report>(), 0));

            var service = CreateService();

            // Action
            var result = await service.GetIncomingReportsAsync(search, sortBy, sortDirection, pageNumber, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.PageNumber.Should().Be(pageNumber);
            result.PageSize.Should().Be(pageSize);
            result.TotalCount.Should().Be(0);
            result.Items.Should().BeEmpty();
            result.Message.Should().Be("No incoming reports found. Try adjusting your search or filters.");
        }

        [Fact]
        public async Task GetIncomingReportsAsync_WithValidReports_ReturnsMappedPaginatedResult()
        {
            // Arrange
            string? search = null;
            string? sortBy = null;
            string? sortDirection = null;
            int pageNumber = 1;
            int pageSize = 10;

            var submitter = new User
            {
                Id = "sub-1",
                Name = "John Reporter",
                Email = "john.rep@example.com",
                ProfilePictureUrl = "profile.jpg"
            };

            var report = new Report
            {
                Id = 42,
                Description = "A major pothole on the main street causing traffic build-up.",
                Latitude = 30.123456,
                Longitude = 31.654321,
                UploadedAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 2, 15, 30, 0, DateTimeKind.Utc),
                Status = ReportStatus.Submitted,
                SubmittedBy = submitter,
                ReportPics = new List<ReportPic>
                {
                    new ReportPic { PicId = 101, PicUrl = "pic1.jpg" },
                    new ReportPic { PicId = 102, PicUrl = "pic2.jpg" },
                    new ReportPic { PicId = 103, PicUrl = "pic3.jpg" },
                    new ReportPic { PicId = 104, PicUrl = "pic4.jpg" }
                }
            };

            _repositoryMock
                .Setup(r => r.GetIncomingReportsAsync(null, null, "desc", pageNumber, pageSize))
                .ReturnsAsync((new List<Report> { report }, 1));

            var service = CreateService();

            // Action
            var result = await service.GetIncomingReportsAsync(search, sortBy, sortDirection, pageNumber, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(1);
            result.Items.Should().HaveCount(1);
            result.Message.Should().BeEmpty();

            var item = result.Items.First();
            item.ReportId.Should().Be(42);
            item.Title.Should().Be("A major pothole on the main street causing traffic build-up.");
            item.Description.Should().Be(report.Description);
            item.Location.Should().Be("30.1235, 31.6543"); // 30.123456 and 31.654321 formatted with F4
            item.UpdatedAt.Should().Be(report.UpdatedAt);
            item.Status.Should().Be("Submitted");
            item.ReporterName.Should().Be("John Reporter");
            item.ReporterEmail.Should().Be("john.rep@example.com");
            item.ReporterImage.Should().Be("profile.jpg");
            item.PhotosCount.Should().Be(4);
            item.PhotosPreview.Should().HaveCount(3);
            item.PhotosPreview.Should().Equal("pic1.jpg", "pic2.jpg", "pic3.jpg");
        }

        [Fact]
        public async Task GetIncomingReportsAsync_WhenUpdatedAtIsNull_FallsBackToUploadedAt()
        {
            // Arrange
            var report = new Report
            {
                Id = 1,
                Description = "Short description",
                Latitude = 30.0,
                Longitude = 31.0,
                UploadedAt = new DateTime(2026, 5, 20, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAt = null,
                Status = ReportStatus.Submitted
            };

            _repositoryMock
                .Setup(r => r.GetIncomingReportsAsync(null, null, "desc", 1, 10))
                .ReturnsAsync((new List<Report> { report }, 1));

            var service = CreateService();

            // Action
            var result = await service.GetIncomingReportsAsync(null, null, null, 1, 10);

            // Assert
            result.Items.First().UpdatedAt.Should().Be(report.UploadedAt);
        }

        [Theory]
        [InlineData("A short description of the issue.", "A short description of the issue.")]
        [InlineData("This description is longer than eighty characters to check if ellipsis is appended correctly in the mapped title.", "This description is longer than eighty characters to check if ellipsis is append\u2026")]
        [InlineData(null, "")]
        [InlineData("", "")]
        public async Task GetIncomingReportsAsync_ChecksTitleEllipsisTruncation(string? description, string expectedTitle)
        {
            // Arrange
            var report = new Report
            {
                Id = 1,
                Description = description ?? "",
                Latitude = 30.0,
                Longitude = 31.0,
                UploadedAt = DateTime.UtcNow,
                Status = ReportStatus.Submitted
            };

            _repositoryMock
                .Setup(r => r.GetIncomingReportsAsync(null, null, "desc", 1, 10))
                .ReturnsAsync((new List<Report> { report }, 1));

            var service = CreateService();

            // Action
            var result = await service.GetIncomingReportsAsync(null, null, null, 1, 10);

            // Assert
            result.Items.First().Title.Should().Be(expectedTitle);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(-5, 1)]
        [InlineData(2, 2)]
        public async Task GetIncomingReportsAsync_ClampsPageNumber(int inputPage, int expectedPage)
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.GetIncomingReportsAsync(null, null, "desc", expectedPage, 10))
                .ReturnsAsync((new List<Report>(), 0));

            var service = CreateService();

            // Action
            await service.GetIncomingReportsAsync(null, null, null, inputPage, 10);

            // Assert
            _repositoryMock.Verify(r => r.GetIncomingReportsAsync(null, null, "desc", expectedPage, 10), Times.Once);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(-10, 1)]
        [InlineData(25, 25)]
        [InlineData(50, 50)]
        [InlineData(60, 50)]
        public async Task GetIncomingReportsAsync_ClampsPageSize(int inputSize, int expectedSize)
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.GetIncomingReportsAsync(null, null, "desc", 1, expectedSize))
                .ReturnsAsync((new List<Report>(), 0));

            var service = CreateService();

            // Action
            await service.GetIncomingReportsAsync(null, null, null, 1, inputSize);

            // Assert
            _repositoryMock.Verify(r => r.GetIncomingReportsAsync(null, null, "desc", 1, expectedSize), Times.Once);
        }

        [Theory]
        [InlineData("title", "title")]
        [InlineData("updatedat", "updatedat")]
        [InlineData("reporter", "reporter")]
        [InlineData("TITLE", "TITLE")]
        [InlineData("invalid", null)]
        [InlineData("", "")]
        [InlineData(null, null)]
        public async Task GetIncomingReportsAsync_ValidatesSortByValues(string? inputSortBy, string? expectedSortBy)
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.GetIncomingReportsAsync(null, expectedSortBy, "desc", 1, 10))
                .ReturnsAsync((new List<Report>(), 0));

            var service = CreateService();

            // Action
            await service.GetIncomingReportsAsync(null, inputSortBy, null, 1, 10);

            // Assert
            _repositoryMock.Verify(r => r.GetIncomingReportsAsync(null, expectedSortBy, "desc", 1, 10), Times.Once);
        }

        [Theory]
        [InlineData("asc", "asc")]
        [InlineData("desc", "desc")]
        [InlineData("ASC", "ASC")]
        [InlineData("DESC", "DESC")]
        [InlineData("invalid", "desc")]
        [InlineData("", "desc")]
        [InlineData(null, "desc")]
        public async Task GetIncomingReportsAsync_ValidatesSortDirectionValues(string? inputDir, string expectedDir)
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.GetIncomingReportsAsync(null, null, expectedDir, 1, 10))
                .ReturnsAsync((new List<Report>(), 0));

            var service = CreateService();

            // Action
            await service.GetIncomingReportsAsync(null, null, inputDir, 1, 10);

            // Assert
            _repositoryMock.Verify(r => r.GetIncomingReportsAsync(null, null, expectedDir, 1, 10), Times.Once);
        }

        [Fact]
        public async Task GetIncomingReportsAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.GetIncomingReportsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException("Database failure"));

            var service = CreateService();

            // Action & Assert
            Func<Task> action = async () => await service.GetIncomingReportsAsync(null, null, null, 1, 10);
            await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("Database failure");
        }

        private AuthorityIncomingReportsService CreateService()
        {
            return new AuthorityIncomingReportsService(_repositoryMock.Object, _loggerMock.Object);
        }
    }
}
