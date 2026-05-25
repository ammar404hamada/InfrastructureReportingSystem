using System;
using System.Threading.Tasks;
using FluentAssertions;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Email;
using InfraReportingSystem.ServiceAbstractions.Repositories.Auth;
using InfraReportingSystem.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace InfrastructureReportingSystem.Tests.Unit.Auth
{
    public class OtpServiceTests
    {
        private readonly Mock<IOtpRepository> _otpRepositoryMock = new();
        private readonly Mock<IEmailService> _emailServiceMock = new();
        private readonly Mock<UserManager<User>> _userManagerMock = CreateUserManagerMock();

        [Fact]
        public void Constructor_ShouldInitializeSuccessfully()
        {
            var service = CreateService();
            service.Should().NotBeNull();
        }

        [Fact]
        public async Task GenerateOtp_WhenUserNotFound_ThrowsInvalidOperationException()
        {
            const string userId = "non-existent-user";
            _userManagerMock
                .Setup(m => m.FindByIdAsync(userId))
                .ReturnsAsync((User?)null);

            var service = CreateService();

            Func<Task> action = async () => await service.GenerateOtp(userId, OtpPurpose.EmailConfirmation);

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot send OTP because the user or user email was not found.");

            _otpRepositoryMock.Verify(r => r.Invalidate(userId, OtpPurpose.EmailConfirmation), Times.Once);
            _otpRepositoryMock.Verify(r => r.SaveOtp(It.IsAny<OtpVerification>()), Times.Once);
            _emailServiceMock.Verify(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GenerateOtp_WhenUserEmailIsMissing_ThrowsInvalidOperationException(string? missingEmail)
        {
            const string userId = "user-123";
            var user = new User { Id = userId, Name = "Alice", Email = missingEmail };
            _userManagerMock
                .Setup(m => m.FindByIdAsync(userId))
                .ReturnsAsync(user);

            var service = CreateService();

            Func<Task> action = async () => await service.GenerateOtp(userId, OtpPurpose.EmailConfirmation);

            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot send OTP because the user or user email was not found.");

            _otpRepositoryMock.Verify(r => r.Invalidate(userId, OtpPurpose.EmailConfirmation), Times.Once);
            _otpRepositoryMock.Verify(r => r.SaveOtp(It.IsAny<OtpVerification>()), Times.Once);
            _emailServiceMock.Verify(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GenerateOtp_WhenSuccess_InvalidatesOldCodesSavesNewAndSendsEmail()
        {
            const string userId = "user-123";
            const string email = "alice@example.com";
            var user = new User { Id = userId, Name = "Alice", Email = email };

            _userManagerMock
                .Setup(m => m.FindByIdAsync(userId))
                .ReturnsAsync(user);

            OtpVerification? savedOtp = null;
            _otpRepositoryMock
                .Setup(r => r.SaveOtp(It.IsAny<OtpVerification>()))
                .Callback<OtpVerification>(otp => savedOtp = otp)
                .Returns(Task.CompletedTask);

            var service = CreateService();

            await service.GenerateOtp(userId, OtpPurpose.EmailConfirmation);

            _otpRepositoryMock.Verify(r => r.Invalidate(userId, OtpPurpose.EmailConfirmation), Times.Once);
            _otpRepositoryMock.Verify(r => r.SaveOtp(It.IsAny<OtpVerification>()), Times.Once);

            savedOtp.Should().NotBeNull();
            savedOtp!.UserId.Should().Be(userId);
            savedOtp.Purpose.Should().Be(OtpPurpose.EmailConfirmation);
            savedOtp.IsUsed.Should().BeFalse();
            savedOtp.OtpCode.Should().HaveLength(6);
            int.TryParse(savedOtp.OtpCode, out _).Should().BeTrue();

            _emailServiceMock.Verify(
                s => s.SendEmailAsync(
                    email,
                    It.Is<string>(subj => subj.Contains("EmailConfirmation")),
                    It.Is<string>(body => body.Contains(savedOtp.OtpCode))),
                Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task VerifyOtp_WhenOtpCodeIsNullOrWhiteSpace_ReturnsFalse(string? invalidCode)
        {
            var service = CreateService();

            var result = await service.VerifyOtp("user-123", OtpPurpose.EmailConfirmation, invalidCode!);

            result.Should().BeFalse();
            _otpRepositoryMock.Verify(r => r.FindOtp(It.IsAny<string>(), It.IsAny<OtpPurpose>()), Times.Never);
        }

        [Fact]
        public async Task VerifyOtp_WhenNoOtpExists_ReturnsFalse()
        {
            const string userId = "user-123";
            _otpRepositoryMock
                .Setup(r => r.FindOtp(userId, OtpPurpose.EmailConfirmation))
                .ReturnsAsync((OtpVerification?)null);

            var service = CreateService();

            var result = await service.VerifyOtp(userId, OtpPurpose.EmailConfirmation, "123456");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task VerifyOtp_WhenOtpCodeDoesNotMatch_ReturnsFalse()
        {
            const string userId = "user-123";
            var existingOtp = new OtpVerification { OtpCode = "123456", UserId = userId, Purpose = OtpPurpose.EmailConfirmation };

            _otpRepositoryMock
                .Setup(r => r.FindOtp(userId, OtpPurpose.EmailConfirmation))
                .ReturnsAsync(existingOtp);

            var service = CreateService();

            var result = await service.VerifyOtp(userId, OtpPurpose.EmailConfirmation, "654321");

            result.Should().BeFalse();
            existingOtp.IsUsed.Should().BeFalse();
            _otpRepositoryMock.Verify(r => r.Invalidate(It.IsAny<string>(), It.IsAny<OtpPurpose>()), Times.Never);
        }

        [Fact]
        public async Task VerifyOtp_WhenOtpCodeMatches_InvalidatesAndReturnsTrue()
        {
            const string userId = "user-123";
            var existingOtp = new OtpVerification { OtpCode = "123456", UserId = userId, Purpose = OtpPurpose.EmailConfirmation, IsUsed = false };

            _otpRepositoryMock
                .Setup(r => r.FindOtp(userId, OtpPurpose.EmailConfirmation))
                .ReturnsAsync(existingOtp);

            var service = CreateService();

            var result = await service.VerifyOtp(userId, OtpPurpose.EmailConfirmation, "  123456  "); // trim check

            result.Should().BeTrue();
            existingOtp.IsUsed.Should().BeTrue();
            _otpRepositoryMock.Verify(r => r.Invalidate(userId, OtpPurpose.EmailConfirmation), Times.Once);
        }

        private OtpService CreateService()
        {
            return new OtpService(_otpRepositoryMock.Object, _emailServiceMock.Object, _userManagerMock.Object);
        }

        private static Mock<UserManager<User>> CreateUserManagerMock()
        {
            var storeMock = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(
                storeMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }
    }
}
