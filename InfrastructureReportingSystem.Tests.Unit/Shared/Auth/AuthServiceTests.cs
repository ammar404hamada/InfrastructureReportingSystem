using FluentAssertions;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Shared.Auth;
using InfraReportingSystem.ServiceAbstractions.Shared.Auth;
using InfraReportingSystem.Services.Shared.Auth;
using InfraReportingSystem.Shared.DTOs.Shared.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

namespace InfrastructureReportingSystem.Tests.Unit.Shared.Auth;

public class AuthServiceTests
{
    private const string PublicUserRole = "PublicUser";
    private readonly Mock<UserManager<User>> _userManagerMock = CreateUserManagerMock();
    private readonly Mock<IOtpService> _otpServiceMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly IConfiguration _configuration = CreateConfiguration();

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ReturnsFailure()
    {
        var dto = CreateRegisterDto();
        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync(dto.Email))
            .ReturnsAsync(CreateUser(dto.Email, emailConfirmed: true, status: UserStatus.Active));

        var service = CreateService();

        var result = await service.RegisterAsync(dto);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("This email already exists.");
        _userManagerMock.Verify(
            manager => manager.CreateAsync(It.IsAny<User>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhenExistingUserIsNotConfirmed_ResendsOtpAndReturnsActionCode()
    {
        var dto = CreateRegisterDto();
        var user = CreateUser(dto.Email, emailConfirmed: false, status: UserStatus.Inactive);
        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        var service = CreateService();

        var result = await service.RegisterAsync(dto);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Please confirm your email first.");
        result.Code.Should().Be("EMAIL_CONFIRMATION_REQUIRED");
        _otpServiceMock.Verify(
            service => service.GenerateOtp(user.Id, OtpPurpose.EmailConfirmation),
            Times.Once);
        _userManagerMock.Verify(
            manager => manager.CreateAsync(It.IsAny<User>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhenCreateUserFails_ReturnsIdentityErrors()
    {
        var dto = CreateRegisterDto();
        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);
        _userManagerMock
            .Setup(manager => manager.CreateAsync(It.IsAny<User>(), dto.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Weak password." }));

        var service = CreateService();

        var result = await service.RegisterAsync(dto);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Weak password.");
        _userManagerMock.Verify(
            manager => manager.AddToRoleAsync(It.IsAny<User>(), PublicUserRole),
            Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhenRoleAssignmentFails_DeletesUserAndReturnsFailure()
    {
        var dto = CreateRegisterDto();
        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);
        _userManagerMock
            .Setup(manager => manager.CreateAsync(It.IsAny<User>(), dto.Password))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(manager => manager.AddToRoleAsync(It.IsAny<User>(), PublicUserRole))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role not found." }));
        _userManagerMock
            .Setup(manager => manager.DeleteAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        var service = CreateService();

        var result = await service.RegisterAsync(dto);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Role not found.");
        _userManagerMock.Verify(manager => manager.DeleteAsync(It.IsAny<User>()), Times.Once);
        _otpServiceMock.Verify(
            service => service.GenerateOtp(It.IsAny<string>(), It.IsAny<OtpPurpose>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhenOtpGenerationFails_DeletesUserAndReturnsFailure()
    {
        var dto = CreateRegisterDto();
        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);
        _userManagerMock
            .Setup(manager => manager.CreateAsync(It.IsAny<User>(), dto.Password))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(manager => manager.AddToRoleAsync(It.IsAny<User>(), PublicUserRole))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(manager => manager.DeleteAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);
        _otpServiceMock
            .Setup(service => service.GenerateOtp(It.IsAny<string>(), OtpPurpose.EmailConfirmation))
            .ThrowsAsync(new InvalidOperationException("Email service is unavailable."));

        var service = CreateService();

        var result = await service.RegisterAsync(dto);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Registration failed because the verification code could not be sent. Please try again later.");
        _userManagerMock.Verify(manager => manager.DeleteAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WhenValidData_ReturnsSuccessAndGeneratesEmailConfirmationOtp()
    {
        var dto = CreateRegisterDto();
        User? createdUser = null;
        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);
        _userManagerMock
            .Setup(manager => manager.CreateAsync(It.IsAny<User>(), dto.Password))
            .Callback<User, string>((user, _) => createdUser = user)
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(manager => manager.AddToRoleAsync(It.IsAny<User>(), PublicUserRole))
            .ReturnsAsync(IdentityResult.Success);

        var service = CreateService();

        var result = await service.RegisterAsync(dto);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Registration successful. Please confirm your email.");
        result.User.Should().NotBeNull();
        result.User!.Email.Should().Be(dto.Email);
        result.User.Role.Should().Be(PublicUserRole);
        createdUser.Should().NotBeNull();
        createdUser!.Status.Should().Be(UserStatus.Inactive);
        _otpServiceMock.Verify(
            otp => otp.GenerateOtp(createdUser.Id, OtpPurpose.EmailConfirmation),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ReturnsFailure()
    {
        var dto = CreateLoginDto();
        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        var result = await service.LoginAsync(dto);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("User does not exist.");
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsInvalid_ReturnsFailure()
    {
        var dto = CreateLoginDto();
        var user = CreateUser(dto.Email, emailConfirmed: true, status: UserStatus.Active);
        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(manager => manager.CheckPasswordAsync(user, dto.Password))
            .ReturnsAsync(false);

        var service = CreateService();

        var result = await service.LoginAsync(dto);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid password.");
        result.Code.Should().Be("INVALID_CREDENTIALS");
        _userManagerMock.Verify(manager => manager.AccessFailedAsync(user), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WhenEmailIsNotConfirmed_ReturnsFailure()
    {
        var dto = CreateLoginDto();
        var user = CreateUser(dto.Email, emailConfirmed: false, status: UserStatus.Inactive);
        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(manager => manager.CheckPasswordAsync(user, dto.Password))
            .ReturnsAsync(true);

        var service = CreateService();

        var result = await service.LoginAsync(dto);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Please confirm your email first.");
        result.Code.Should().Be("EMAIL_CONFIRMATION_REQUIRED");
        _otpServiceMock.Verify(
            otp => otp.GenerateOtp(user.Id, OtpPurpose.EmailConfirmation),
            Times.Once);
    }

    [Theory]
    [InlineData(UserStatus.Suspended, "Your account has been suspended.")]
    [InlineData(UserStatus.Locked, "Your account is locked.")]
    public async Task LoginAsync_WhenAccountCannotLogin_ReturnsFailure(UserStatus status, string expectedMessage)
    {
        var dto = CreateLoginDto();
        var user = CreateUser(dto.Email, emailConfirmed: true, status: status);
        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(manager => manager.CheckPasswordAsync(user, dto.Password))
            .ReturnsAsync(true);

        var service = CreateService();

        var result = await service.LoginAsync(dto);

        result.Success.Should().BeFalse();
        result.Message.Should().Be(expectedMessage);
    }

    [Fact]
    public async Task LoginAsync_WhenCredentialsAreValid_ReturnsTokenAndRefreshToken()
    {
        var dto = CreateLoginDto();
        var user = CreateUser(dto.Email, emailConfirmed: true, status: UserStatus.Active);
        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(manager => manager.CheckPasswordAsync(user, dto.Password))
            .ReturnsAsync(true);
        _userManagerMock
            .Setup(manager => manager.GetRolesAsync(user))
            .ReturnsAsync([PublicUserRole]);
        _userManagerMock
            .Setup(manager => manager.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var service = CreateService();

        var result = await service.LoginAsync(dto);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Login successful.");
        result.Token.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.User.Should().NotBeNull();
        result.User!.Role.Should().Be(PublicUserRole);
        user.RefreshTokens.Should().ContainSingle();
        user.RefreshTokens!.Single().UserId.Should().Be(user.Id);
        _userManagerMock.Verify(manager => manager.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task ConfirmEmailAsync_WhenUserDoesNotExist_ReturnsFalse()
    {
        var dto = new ConfirmEmailDto { UserEmail = "missing@example.com", OtpCode = "123456" };
        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync(dto.UserEmail))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        var result = await service.ConfirmEmailAsync(dto);

        result.Should().BeFalse();
        _otpServiceMock.Verify(
            otp => otp.VerifyOtp(It.IsAny<string>(), It.IsAny<OtpPurpose>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ConfirmEmailAsync_WhenOtpIsInvalid_ReturnsFalse()
    {
        var dto = new ConfirmEmailDto { UserEmail = "mostafa@example.com", OtpCode = "123456" };
        var user = CreateUser(dto.UserEmail);
        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync(dto.UserEmail))
            .ReturnsAsync(user);
        _otpServiceMock
            .Setup(otp => otp.VerifyOtp(user.Id, OtpPurpose.EmailConfirmation, dto.OtpCode))
            .ReturnsAsync(false);

        var service = CreateService();

        var result = await service.ConfirmEmailAsync(dto);

        result.Should().BeFalse();
        _userManagerMock.Verify(manager => manager.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmEmailAsync_WhenOtpIsValid_ActivatesUserAndReturnsTrue()
    {
        var dto = new ConfirmEmailDto { UserEmail = "mostafa@example.com", OtpCode = "123456" };
        var user = CreateUser(dto.UserEmail, emailConfirmed: false, status: UserStatus.Inactive);
        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync(dto.UserEmail))
            .ReturnsAsync(user);
        _otpServiceMock
            .Setup(otp => otp.VerifyOtp(user.Id, OtpPurpose.EmailConfirmation, dto.OtpCode))
            .ReturnsAsync(true);
        _userManagerMock
            .Setup(manager => manager.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var service = CreateService();

        var result = await service.ConfirmEmailAsync(dto);

        result.Should().BeTrue();
        user.EmailConfirmed.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public async Task ResendConfirmationEmailAsync_WhenUserIsMissing_ReturnsFalse()
    {
        const string email = "missing@example.com";
        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync(email))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        var result = await service.ResendConfirmationEmailAsync(email);

        result.Should().BeFalse();
        _otpServiceMock.Verify(
            otp => otp.GenerateOtp(It.IsAny<string>(), It.IsAny<OtpPurpose>()),
            Times.Never);
    }

    [Fact]
    public async Task ResendConfirmationEmailAsync_WhenEmailIsAlreadyConfirmed_ReturnsFalse()
    {
        const string email = "confirmed@example.com";
        var user = CreateUser(email, emailConfirmed: true, status: UserStatus.Active);
        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync(email))
            .ReturnsAsync(user);

        var service = CreateService();

        var result = await service.ResendConfirmationEmailAsync(email);

        result.Should().BeFalse();
        _otpServiceMock.Verify(
            otp => otp.GenerateOtp(It.IsAny<string>(), It.IsAny<OtpPurpose>()),
            Times.Never);
    }

    [Fact]
    public async Task ResendConfirmationEmailAsync_WhenUserIsUnconfirmed_GeneratesOtpAndReturnsTrue()
    {
        const string email = "unconfirmed@example.com";
        var user = CreateUser(email, emailConfirmed: false, status: UserStatus.Inactive);
        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync(email))
            .ReturnsAsync(user);

        var service = CreateService();

        var result = await service.ResendConfirmationEmailAsync(email);

        result.Should().BeTrue();
        _otpServiceMock.Verify(
            otp => otp.GenerateOtp(user.Id, OtpPurpose.EmailConfirmation),
            Times.Once);
    }

    [Fact]
    public async Task ForgotPasswordAsync_WhenUserIsMissing_DoesNotGenerateOtp()
    {
        var dto = new ForgotPasswordDto { Email = "missing@example.com" };
        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        await service.ForgotPasswordAsync(dto);

        _otpServiceMock.Verify(
            otp => otp.GenerateOtp(It.IsAny<string>(), It.IsAny<OtpPurpose>()),
            Times.Never);
    }

    [Fact]
    public async Task ForgotPasswordAsync_WhenUserExists_GeneratesPasswordResetOtp()
    {
        var dto = new ForgotPasswordDto { Email = "mostafa@example.com" };
        var user = CreateUser(dto.Email);
        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        var service = CreateService();

        await service.ForgotPasswordAsync(dto);

        _otpServiceMock.Verify(
            otp => otp.GenerateOtp(user.Id, OtpPurpose.PasswordReset),
            Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenUserIsMissing_ReturnsFailure()
    {
        var dto = CreateResetPasswordDto();
        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        var result = await service.ResetPasswordAsync(dto);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("User not found.");
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenOtpIsInvalid_ReturnsFailure()
    {
        var dto = CreateResetPasswordDto();
        var user = CreateUser(dto.Email);
        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);
        _otpServiceMock
            .Setup(otp => otp.VerifyOtp(user.Id, OtpPurpose.PasswordReset, dto.OtpCode))
            .ReturnsAsync(false);

        var service = CreateService();

        var result = await service.ResetPasswordAsync(dto);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid or expired OTP.");
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenIdentityResetFails_ReturnsFailure()
    {
        var dto = CreateResetPasswordDto();
        var user = CreateUser(dto.Email);
        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);
        _otpServiceMock
            .Setup(otp => otp.VerifyOtp(user.Id, OtpPurpose.PasswordReset, dto.OtpCode))
            .ReturnsAsync(true);
        _userManagerMock
            .Setup(manager => manager.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("password-reset-token");
        _userManagerMock
            .Setup(manager => manager.ResetPasswordAsync(user, "password-reset-token", dto.NewPassword))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password is too weak." }));

        var service = CreateService();

        var result = await service.ResetPasswordAsync(dto);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Password is too weak.");
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenInactiveWorkerResetsPassword_ActivatesUser()
    {
        var dto = CreateResetPasswordDto();
        var user = CreateUser(dto.Email, emailConfirmed: true, status: UserStatus.Inactive);
        _userManagerMock
            .Setup(manager => manager.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);
        _otpServiceMock
            .Setup(otp => otp.VerifyOtp(user.Id, OtpPurpose.PasswordReset, dto.OtpCode))
            .ReturnsAsync(true);
        _userManagerMock
            .Setup(manager => manager.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("password-reset-token");
        _userManagerMock
            .Setup(manager => manager.ResetPasswordAsync(user, "password-reset-token", dto.NewPassword))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(manager => manager.GetRolesAsync(user))
            .ReturnsAsync(["Worker"]);
        _userManagerMock
            .Setup(manager => manager.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var service = CreateService();

        var result = await service.ResetPasswordAsync(dto);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Password reset successfully.");
        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenNoUserHasToken_ReturnsFailure()
    {
        const string token = "missing-refresh-token";
        _refreshTokenRepositoryMock
            .Setup(repository => repository.FindUserWithRefreshTokenAsync(token))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        var result = await service.RefreshTokenAsync(token);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("No user has this refresh token");
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenIsInactive_ReturnsFailure()
    {
        const string token = "inactive-refresh-token";
        var user = CreateUser("mostafa@example.com");
        var refreshToken = new RefreshToken
        {
            Token = token,
            UserId = user.Id,
            User = user,
            CreatedOn = DateTime.UtcNow.AddDays(-2),
            ExpiresOn = DateTime.UtcNow.AddDays(-1)
        };
        _refreshTokenRepositoryMock
            .Setup(repository => repository.FindUserWithRefreshTokenAsync(token))
            .ReturnsAsync(user);
        _refreshTokenRepositoryMock
            .Setup(repository => repository.FindRefreshTokenAsync(token))
            .ReturnsAsync(refreshToken);

        var service = CreateService();

        var result = await service.RefreshTokenAsync(token);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("This refresh token is not active");
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenIsActive_RevokesOldTokenAndReturnsNewTokens()
    {
        const string token = "active-refresh-token";
        var user = CreateUser("mostafa@example.com", emailConfirmed: true, status: UserStatus.Active);
        var oldRefreshToken = new RefreshToken
        {
            Token = token,
            UserId = user.Id,
            User = user,
            CreatedOn = DateTime.UtcNow.AddDays(-1),
            ExpiresOn = DateTime.UtcNow.AddDays(1)
        };
        user.RefreshTokens = [oldRefreshToken];
        _refreshTokenRepositoryMock
            .Setup(repository => repository.FindUserWithRefreshTokenAsync(token))
            .ReturnsAsync(user);
        _refreshTokenRepositoryMock
            .Setup(repository => repository.FindRefreshTokenAsync(token))
            .ReturnsAsync(oldRefreshToken);
        _userManagerMock
            .Setup(manager => manager.GetRolesAsync(user))
            .ReturnsAsync([PublicUserRole]);
        _userManagerMock
            .Setup(manager => manager.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var service = CreateService();

        var result = await service.RefreshTokenAsync(token);

        result.Success.Should().BeTrue();
        result.Token.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        oldRefreshToken.RevokedOn.Should().NotBeNull();
        user.RefreshTokens.Should().HaveCount(2);
    }

    [Fact]
    public async Task LogoutAllAsync_WhenUserHasRefreshTokens_RevokesAllTokensAndSavesChanges()
    {
        const string userId = "user-id";
        var refreshTokens = new List<RefreshToken>
        {
            new()
            {
                Token = "first-token",
                UserId = userId,
                CreatedOn = DateTime.UtcNow.AddDays(-1),
                ExpiresOn = DateTime.UtcNow.AddDays(1)
            },
            new()
            {
                Token = "second-token",
                UserId = userId,
                CreatedOn = DateTime.UtcNow.AddDays(-1),
                ExpiresOn = DateTime.UtcNow.AddDays(1)
            }
        };
        _refreshTokenRepositoryMock
            .Setup(repository => repository.GetAllRefreshTokenByUserIdAsync(userId))
            .ReturnsAsync(refreshTokens);

        var service = CreateService();

        var result = await service.LogoutAllAsync(userId);

        result.Should().BeTrue();
        refreshTokens.Should().OnlyContain(token => token.RevokedOn != null);
        _refreshTokenRepositoryMock.Verify(repository => repository.SaveTokenChangesAsync(), Times.Once);
    }

    private AuthService CreateService()
    {
        return new AuthService(
            _userManagerMock.Object,
            _configuration,
            _otpServiceMock.Object,
            _refreshTokenRepositoryMock.Object);
    }

    private static RegisterDto CreateRegisterDto()
    {
        return new RegisterDto
        {
            Name = "Mostafa",
            Email = "mostafa@example.com",
            Phone = "01012345678",
            Password = "StrongPass123!",
            RePassword = "StrongPass123!"
        };
    }

    private static LoginDto CreateLoginDto()
    {
        return new LoginDto
        {
            Email = "mostafa@example.com",
            Password = "StrongPass123!"
        };
    }

    private static ResetPasswordDto CreateResetPasswordDto()
    {
        return new ResetPasswordDto
        {
            Email = "mostafa@example.com",
            OtpCode = "123456",
            NewPassword = "NewStrongPass123!",
            ConfirmPassword = "NewStrongPass123!"
        };
    }

    private static User CreateUser(
        string email,
        bool emailConfirmed = false,
        UserStatus status = UserStatus.Inactive)
    {
        return new User
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Mostafa",
            UserName = email,
            Email = email,
            EmailConfirmed = emailConfirmed,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            RefreshTokens = []
        };
    }

    private static IConfiguration CreateConfiguration()
    {
        var configurationValues = new Dictionary<string, string?>
        {
            ["JwtSettings:Key"] = "ThisIsAUnitTestSecretKeyThatIsLongEnoughForHmacSha256",
            ["JwtSettings:Issuer"] = "InfrastructureReportingSystem.Tests",
            ["JwtSettings:Audience"] = "InfrastructureReportingSystem.Tests",
            ["JwtSettings:DurationInDays"] = "1",
            ["JwtSettings:RefreshTokenDurationInDays"] = "7"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();
    }

    private static Mock<UserManager<User>> CreateUserManagerMock()
    {
        var storeMock = new Mock<IUserStore<User>>();

        return new Mock<UserManager<User>>(
            storeMock.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);
    }
}
