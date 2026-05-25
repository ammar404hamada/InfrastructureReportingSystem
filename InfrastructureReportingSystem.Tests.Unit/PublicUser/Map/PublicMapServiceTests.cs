using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.PublicUser.Map;
using InfraReportingSystem.Services.PublicUser.Map;
using InfraReportingSystem.Shared.DTOs.PublicUser.Map;
using Moq;
using Xunit;

namespace InfrastructureReportingSystem.Tests.Unit.PublicUser.Map
{
    public class PublicMapServiceTests
    {
        private readonly Mock<IPublicMapRepository> _repositoryMock = new();

        [Fact]
        public async Task GetMapReportsAsync_WithNullParameters_CallsRepositoryWithNullAndMapsCorrectly()
        {
            // Arrange
            var report1 = new Report
            {
                Id = 1,
                Latitude = 30.1111,
                Longitude = 31.1111,
                Status = ReportStatus.Submitted,
                Category = new Category { Id = 10, Name = "Electricity" }
            };

            var report2 = new Report
            {
                Id = 2,
                Latitude = 30.2222,
                Longitude = 31.2222,
                Status = ReportStatus.Assigned,
                Category = null! // Test category fallback
            };

            var reportsList = new List<Report> { report1, report2 };

            _repositoryMock
                .Setup(r => r.GetMapReportsAsync(null, null))
                .ReturnsAsync(reportsList);

            var service = CreateService();

            // Action
            var result = (await service.GetMapReportsAsync(null, null)).ToList();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            var dto1 = result[0];
            dto1.ReportId.Should().Be(1);
            dto1.Latitude.Should().Be(30.1111);
            dto1.Longitude.Should().Be(31.1111);
            dto1.CategoryName.Should().Be("Electricity");
            dto1.Status.Should().Be("Submitted");

            var dto2 = result[1];
            dto2.ReportId.Should().Be(2);
            dto2.Latitude.Should().Be(30.2222);
            dto2.Longitude.Should().Be(31.2222);
            dto2.CategoryName.Should().BeEmpty();
            dto2.Status.Should().Be("Assigned");

            _repositoryMock.Verify(r => r.GetMapReportsAsync(null, null), Times.Once);
        }

        [Fact]
        public async Task GetMapReportsAsync_WithSpecificParameters_QueriesRepositoryCorrectly()
        {
            // Arrange
            int categoryId = 5;
            var status = ReportStatus.InProgress;

            _repositoryMock
                .Setup(r => r.GetMapReportsAsync(categoryId, status))
                .ReturnsAsync(new List<Report>());

            var service = CreateService();

            // Action
            var result = await service.GetMapReportsAsync(categoryId, status);

            // Assert
            result.Should().BeEmpty();
            _repositoryMock.Verify(r => r.GetMapReportsAsync(categoryId, status), Times.Once);
        }

        [Fact]
        public async Task GetMapReportsAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.GetMapReportsAsync(It.IsAny<int?>(), It.IsAny<ReportStatus?>()))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            var service = CreateService();

            // Action & Assert
            Func<Task> action = async () => await service.GetMapReportsAsync(null, null);
            await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("DB error");
        }

        private PublicMapService CreateService()
        {
            return new PublicMapService(_repositoryMock.Object);
        }
    }
}
