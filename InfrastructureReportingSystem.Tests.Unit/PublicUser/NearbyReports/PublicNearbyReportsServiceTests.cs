using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using InfraReportingSystem.ServiceAbstractions.Repositories.PublicUser.NearbyReports;
using InfraReportingSystem.Services.PublicUser.NearbyReports;
using InfraReportingSystem.Shared.DTOs.PublicUser.NearbyReports;
using Moq;
using Xunit;

namespace InfrastructureReportingSystem.Tests.Unit.PublicUser.NearbyReports
{
    public class PublicNearbyReportsServiceTests
    {
        private readonly Mock<IPublicNearbyReportsRepository> _repositoryMock = new();

        [Fact]
        public async Task GetNearbyReportsAsync_WithValidRadius_QueriesRepositoryCorrectly()
        {
            // Arrange
            double latitude = 30.1234;
            double longitude = 31.5678;
            double radiusInKm = 10.0;

            var nearbyReportsList = new List<NearbyReportDto>
            {
                new NearbyReportDto
                {
                    ReportId = 1,
                    Latitude = 30.1250,
                    Longitude = 31.5700,
                    DistanceInKm = 0.5
                }
            };

            _repositoryMock
                .Setup(r => r.GetNearbyReportsAsync(latitude, longitude, radiusInKm))
                .ReturnsAsync(nearbyReportsList);

            var service = CreateService();

            // Action
            var result = (await service.GetNearbyReportsAsync(latitude, longitude, radiusInKm)).ToList();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().ReportId.Should().Be(1);
            result.First().DistanceInKm.Should().Be(0.5);

            _repositoryMock.Verify(r => r.GetNearbyReportsAsync(latitude, longitude, radiusInKm), Times.Once);
        }

        [Fact]
        public async Task GetNearbyReportsAsync_WithRadiusGreaterThanMax_ClampsRadiusToFiftyKm()
        {
            // Arrange
            double latitude = 30.0;
            double longitude = 31.0;
            double excessiveRadius = 100.0;
            double clampedRadius = 50.0;

            _repositoryMock
                .Setup(r => r.GetNearbyReportsAsync(latitude, longitude, clampedRadius))
                .ReturnsAsync(new List<NearbyReportDto>());

            var service = CreateService();

            // Action
            var result = await service.GetNearbyReportsAsync(latitude, longitude, excessiveRadius);

            // Assert
            result.Should().BeEmpty();
            _repositoryMock.Verify(r => r.GetNearbyReportsAsync(latitude, longitude, clampedRadius), Times.Once);
            _repositoryMock.Verify(r => r.GetNearbyReportsAsync(latitude, longitude, excessiveRadius), Times.Never);
        }

        [Fact]
        public async Task GetNearbyReportsAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            double latitude = 30.0;
            double longitude = 31.0;
            _repositoryMock
                .Setup(r => r.GetNearbyReportsAsync(latitude, longitude, It.IsAny<double>()))
                .ThrowsAsync(new InvalidOperationException("DB down"));

            var service = CreateService();

            // Action & Assert
            Func<Task> action = async () => await service.GetNearbyReportsAsync(latitude, longitude, 5.0);
            await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("DB down");
        }

        private PublicNearbyReportsService CreateService()
        {
            return new PublicNearbyReportsService(_repositoryMock.Object);
        }
    }
}
