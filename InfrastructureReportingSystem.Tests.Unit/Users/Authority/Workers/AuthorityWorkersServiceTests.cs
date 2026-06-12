using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Authority.Workers;
using InfraReportingSystem.Services.Users.Authority.Workers;
using InfraReportingSystem.Shared.DTOs.UserServices.Authority.Workers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace InfrastructureReportingSystem.Tests.Unit.Users.Authority.Workers
{
    public class AuthorityWorkersServiceTests
    {
        private readonly Mock<IAuthorityWorkersRepository> _repositoryMock = new();
        private readonly Mock<ILogger<AuthorityWorkersService>> _loggerMock = new();

        [Fact]
        public async Task GetWorkersAsync_WhenNoWorkersFound_ReturnsEmptyResultWithMessage()
        {
            // Arrange
            string? search = "elect";
            int pageNumber = 1;
            int pageSize = 15;

            _repositoryMock
                .Setup(r => r.GetWorkersAsync(search, pageNumber, pageSize))
                .ReturnsAsync((new List<(User Worker, int ActiveTaskCount)>(), 0));

            var service = CreateService();

            // Action
            var result = await service.GetWorkersAsync(search, pageNumber, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.PageNumber.Should().Be(pageNumber);
            result.PageSize.Should().Be(pageSize);
            result.TotalCount.Should().Be(0);
            result.Items.Should().BeEmpty();
            result.Message.Should().Be("No workers found. Try adjusting your search.");
            _repositoryMock.Verify(r => r.GetWorkersAsync(search, pageNumber, pageSize), Times.Once);
        }

        [Fact]
        public async Task GetWorkersAsync_WithValidWorkers_ReturnsMappedPaginatedResult()
        {
            // Arrange
            string? search = null;
            int pageNumber = 1;
            int pageSize = 10;

            var workerEntity = new InfraReportingSystem.Domain.Entities.Worker
            {
                Id = "worker-101",
                Name = "Bob Builder",
                Email = "bob@example.com",
                PhoneNumber = "01012345678",
                Specialization = "Carpentry"
            };

            var nonWorkerEntity = new User
            {
                Id = "user-202",
                Name = "John Doe",
                Email = null, // Test null email mapping fallback
                PhoneNumber = null // Test null phone mapping fallback
            };

            var workers = new List<(User Worker, int ActiveTaskCount)>
            {
                (workerEntity, 2),
                (nonWorkerEntity, 0)
            };

            _repositoryMock
                .Setup(r => r.GetWorkersAsync(search, pageNumber, pageSize))
                .ReturnsAsync((workers, 2));

            var service = CreateService();

            // Action
            var result = await service.GetWorkersAsync(search, pageNumber, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(2);
            result.Items.Should().HaveCount(2);
            result.Message.Should().BeEmpty();

            var dto1 = result.Items[0];
            dto1.Id.Should().Be("worker-101");
            dto1.Name.Should().Be("Bob Builder");
            dto1.Email.Should().Be("bob@example.com");
            dto1.PhoneNumber.Should().Be("01012345678");
            dto1.Specialization.Should().Be("Carpentry");
            dto1.ActiveTaskCount.Should().Be(2);

            var dto2 = result.Items[1];
            dto2.Id.Should().Be("user-202");
            dto2.Name.Should().Be("John Doe");
            dto2.Email.Should().BeEmpty();
            dto2.PhoneNumber.Should().BeEmpty();
            dto2.Specialization.Should().BeNull(); // Cast to specialized worker fails, maps to null
            dto2.ActiveTaskCount.Should().Be(0);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(-5, 1)]
        [InlineData(3, 3)]
        public async Task GetWorkersAsync_ClampsPageNumber(int inputPage, int expectedPage)
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.GetWorkersAsync(null, expectedPage, 10))
                .ReturnsAsync((new List<(User, int)>(), 0));

            var service = CreateService();

            // Action
            await service.GetWorkersAsync(null, inputPage, 10);

            // Assert
            _repositoryMock.Verify(r => r.GetWorkersAsync(null, expectedPage, 10), Times.Once);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(-10, 1)]
        [InlineData(50, 50)]
        [InlineData(100, 100)]
        [InlineData(120, 100)]
        public async Task GetWorkersAsync_ClampsPageSize(int inputSize, int expectedSize)
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.GetWorkersAsync(null, 1, expectedSize))
                .ReturnsAsync((new List<(User, int)>(), 0));

            var service = CreateService();

            // Action
            await service.GetWorkersAsync(null, 1, inputSize);

            // Assert
            _repositoryMock.Verify(r => r.GetWorkersAsync(null, 1, expectedSize), Times.Once);
        }

        [Fact]
        public async Task GetWorkersAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.GetWorkersAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException("DB offline"));

            var service = CreateService();

            // Action & Assert
            Func<Task> action = async () => await service.GetWorkersAsync(null, 1, 10);
            await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("DB offline");
        }

        private AuthorityWorkersService CreateService()
        {
            return new AuthorityWorkersService(_repositoryMock.Object, _loggerMock.Object);
        }
    }
}
