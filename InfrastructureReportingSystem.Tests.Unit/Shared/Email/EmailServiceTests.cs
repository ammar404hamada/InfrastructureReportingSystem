using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using InfraReportingSystem.Services.Shared.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace InfrastructureReportingSystem.Tests.Unit.Shared.Email
{
    public class EmailServiceTests
    {
        private readonly Mock<ILogger<EmailService>> _loggerMock = new();

        [Fact]
        public void Constructor_ShouldInitializeSuccessfully()
        {
            var service = new EmailService(new ConfigurationBuilder().Build(), _loggerMock.Object);
            service.Should().NotBeNull();
        }

        [Fact]
        public async Task SendEmailAsync_WhenSendingIsDisabled_LogsAndReturnsImmediately()
        {
            var configValues = new Dictionary<string, string?>
            {
                ["EmailSettings:EnableSending"] = "false"
            };
            var config = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();
            var service = new EmailService(config, _loggerMock.Object);

            await service.SendEmailAsync("test@example.com", "Subject", "Body");

            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Email sending is disabled")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Theory]
        [InlineData("", "user", "pass", "123")]       // Missing Host
        [InlineData("host", "", "pass", "123")]       // Missing User
        [InlineData("host", "user", "", "123")]       // Missing Pass
        [InlineData("host", "user", "pass", "")]      // Missing Port
        [InlineData("host", "user", "pass", "abc")]   // Invalid Port
        public async Task SendEmailAsync_WhenConfigurationIsIncomplete_ThrowsInvalidOperationException(
            string? host, string? user, string? pass, string? port)
        {
            var configValues = new Dictionary<string, string?>
            {
                ["EmailSettings:EnableSending"] = "true",
                ["EmailSettings:Host"] = host,
                ["EmailSettings:Port"] = port,
                ["EmailSettings:UserName"] = user,
                ["EmailSettings:Password"] = pass,
                ["EmailSettings:FromEmail"] = "sender@example.com"
            };
            var config = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();
            var service = new EmailService(config, _loggerMock.Object);

            Func<Task> action = async () => await service.SendEmailAsync("test@example.com", "Subject", "Body");

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Email settings are missing or incomplete.");
        }

        [Fact]
        public async Task SendEmailAsync_WhenEmailAddressIsInvalid_ThrowsInvalidOperationException()
        {
            var configValues = new Dictionary<string, string?>
            {
                ["EmailSettings:EnableSending"] = "true",
                ["EmailSettings:Host"] = "localhost",
                ["EmailSettings:Port"] = "25",
                ["EmailSettings:UserName"] = "test",
                ["EmailSettings:Password"] = "test",
                ["EmailSettings:FromEmail"] = "sender@example.com"
            };
            var config = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();
            var service = new EmailService(config, _loggerMock.Object);

            Func<Task> action = async () => await service.SendEmailAsync("invalid @@@ email", "Subject", "Body");

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*is not a valid email address*");
        }

        [Fact]
        public async Task SendEmailAsync_WhenConnectionFails_PropagatesException()
        {
            // Point to a closed port
            var configValues = new Dictionary<string, string?>
            {
                ["EmailSettings:EnableSending"] = "true",
                ["EmailSettings:Host"] = "127.0.0.1",
                ["EmailSettings:Port"] = "54321", 
                ["EmailSettings:UserName"] = "user",
                ["EmailSettings:Password"] = "pass",
                ["EmailSettings:FromEmail"] = "sender@example.com"
            };
            var config = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();
            var service = new EmailService(config, _loggerMock.Object);

            Func<Task> action = async () => await service.SendEmailAsync("test@example.com", "Subject", "Body");

            await action.Should().ThrowAsync<Exception>();
        }
    }
}
