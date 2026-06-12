using FluentAssertions;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Shared;
using InfraReportingSystem.ServiceAbstractions.Shared.Email;
using InfraReportingSystem.Services.Users.Admin.UserCreationScreen;
using InfraReportingSystem.Shared.DTOs.UserServices.Admin.UserCreationScreen;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace InfrastructureReportingSystem.Tests.Unit.Users.Admin.UserCreationScreen;

public class AdminUserCreationServiceTests
{
    private readonly Mock<UserManager<User>> _userManagerMock = CreateUserManagerMock();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock = new();
    private readonly IConfiguration _configuration = CreateConfiguration();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateUserAsync_WhenFullNameIsMissing_ReturnsErrorAndNoStateChanges(string? missingName)
    {
        var request = CreateRequest(missingName, "test@example.com", "01012345678", "Worker", "Plumbing");

        var service = CreateService();

        var result = await service.CreateUserAsync(request, "admin-1");

        result.Message.Should().Be("Full name is required.");
        _userManagerMock.Verify(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        _emailServiceMock.Verify(m => m.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _auditLogRepositoryMock.Verify(m => m.AddAsync(It.IsAny<AuditLog>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid-email")]
    [InlineData("invalid@")]
    [InlineData("@example.com")]
    public async Task CreateUserAsync_WhenEmailIsInvalid_ReturnsErrorAndNoStateChanges(string? invalidEmail)
    {
        var request = CreateRequest("Ahmad", invalidEmail, "01012345678", "Worker", "Plumbing");

        var service = CreateService();

        var result = await service.CreateUserAsync(request, "admin-1");

        result.Message.Should().Be("A valid email is required.");
        _userManagerMock.Verify(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateUserAsync_WhenEmailAlreadyInUse_ReturnsErrorAndNoStateChanges()
    {
        var request = CreateRequest("Ahmad", "existing@example.com", "01012345678", "Worker", "Plumbing");
        _userManagerMock
            .Setup(m => m.FindByEmailAsync(request.Email!))
            .ReturnsAsync(new User { Email = request.Email });

        var service = CreateService();

        var result = await service.CreateUserAsync(request, "admin-1");

        result.Message.Should().Be("Email is already in use.");
        _userManagerMock.Verify(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("01212345678")] // Starts with 012
    [InlineData("0101234567")]  // 10 digits
    [InlineData("011123456789")] // 12 digits
    [InlineData("abc12345678")]  // non-numeric characters
    public async Task CreateUserAsync_WhenPhoneNumberIsInvalid_ReturnsErrorAndNoStateChanges(string? invalidPhone)
    {
        var request = CreateRequest("Ahmad", "test@example.com", invalidPhone, "Worker", "Plumbing");
        _userManagerMock
            .Setup(m => m.FindByEmailAsync(request.Email!))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        var result = await service.CreateUserAsync(request, "admin-1");

        result.Message.Should().Be("Phone number must be 11 digits starting with 010 or 011.");
        _userManagerMock.Verify(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Admin")]
    [InlineData("PublicUser")]
    [InlineData("Supervisor")]
    public async Task CreateUserAsync_WhenRoleIsInvalid_ReturnsErrorAndNoStateChanges(string? invalidRole)
    {
        var request = CreateRequest("Ahmad", "test@example.com", "01012345678", invalidRole, "Plumbing");
        _userManagerMock
            .Setup(m => m.FindByEmailAsync(request.Email!))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        var result = await service.CreateUserAsync(request, "admin-1");

        result.Message.Should().Be("Role must be either 'Worker' or 'Authority'.");
        _userManagerMock.Verify(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateUserAsync_WhenWorkerRoleAndSpecializationIsMissing_ReturnsErrorAndNoStateChanges(string? missingSpec)
    {
        var request = CreateRequest("Ahmad", "test@example.com", "01012345678", "Worker", missingSpec);
        _userManagerMock
            .Setup(m => m.FindByEmailAsync(request.Email!))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        var result = await service.CreateUserAsync(request, "admin-1");

        result.Message.Should().Be("Specialization is required for Worker accounts.");
        _userManagerMock.Verify(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateUserAsync_WhenUserManagerCreateFails_ReturnsFailureAndNoStateChanges()
    {
        var request = CreateRequest("Ahmad", "test@example.com", "01012345678", "Worker", "Plumbing");
        _userManagerMock
            .Setup(m => m.FindByEmailAsync(request.Email!))
            .ReturnsAsync((User?)null);
        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too simple." }));

        var service = CreateService();

        var result = await service.CreateUserAsync(request, "admin-1");

        result.Message.Should().Be("Password too simple.");
        _userManagerMock.Verify(m => m.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        _emailServiceMock.Verify(m => m.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _auditLogRepositoryMock.Verify(m => m.AddAsync(It.IsAny<AuditLog>()), Times.Never);
    }

    [Fact]
    public async Task CreateUserAsync_WhenRoleAssignmentFails_DeletesUserAndReturnsFailure()
    {
        var request = CreateRequest("Ahmad", "test@example.com", "01012345678", "Worker", "Plumbing");
        User? createdUser = null;
        _userManagerMock
            .Setup(m => m.FindByEmailAsync(request.Email!))
            .ReturnsAsync((User?)null);
        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .Callback<User, string>((u, _) => createdUser = u)
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(m => m.AddToRoleAsync(It.IsAny<User>(), "Worker"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role assignment failed." }));
        _userManagerMock
            .Setup(m => m.DeleteAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        var service = CreateService();

        var result = await service.CreateUserAsync(request, "admin-1");

        result.Message.Should().Be("Role assignment failed.");
        createdUser.Should().NotBeNull();
        _userManagerMock.Verify(m => m.DeleteAsync(createdUser!), Times.Once);
        _emailServiceMock.Verify(m => m.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _auditLogRepositoryMock.Verify(m => m.AddAsync(It.IsAny<AuditLog>()), Times.Never);
    }

    [Fact]
    public async Task CreateUserAsync_WhenWorkerRoleAndValidInputs_CreatesWorkerSendsEmailCreatesAuditLogAndReturnsSuccess()
    {
        var request = CreateRequest("  Ahmad Aly  ", "test@example.com", "01012345678", "  Worker  ", "  Plumbing  ");
        User? createdUser = null;
        _userManagerMock
            .Setup(m => m.FindByEmailAsync(request.Email!))
            .ReturnsAsync((User?)null);
        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .Callback<User, string>((u, _) => createdUser = u)
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(m => m.AddToRoleAsync(It.IsAny<User>(), "Worker"))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(m => m.GeneratePasswordResetTokenAsync(It.IsAny<User>()))
            .ReturnsAsync("reset-token-123");

        var service = CreateService();

        var result = await service.CreateUserAsync(request, "admin-1");

        result.UserId.Should().NotBeNullOrWhiteSpace();
        result.Message.Should().Be("Account created. The user will receive an email to set their password.");

        createdUser.Should().NotBeNull();
        createdUser.Should().BeOfType<InfraReportingSystem.Domain.Entities.Worker>();
        var worker = (InfraReportingSystem.Domain.Entities.Worker)createdUser!;
        worker.Name.Should().Be("Ahmad Aly");
        worker.UserName.Should().Be("test@example.com");
        worker.Email.Should().Be("test@example.com");
        worker.PhoneNumber.Should().Be("01012345678");
        worker.Status.Should().Be(UserStatus.Inactive);
        worker.Specialization.Should().Be("Plumbing");

        _userManagerMock.Verify(m => m.CreateAsync(worker, It.Is<string>(p => p.Length == 16 && p.EndsWith("Aa1!"))), Times.Once);
        _userManagerMock.Verify(m => m.AddToRoleAsync(worker, "Worker"), Times.Once);
        _userManagerMock.Verify(m => m.GeneratePasswordResetTokenAsync(worker), Times.Once);

        _emailServiceMock.Verify(
            s => s.SendEmailAsync(
                "test@example.com",
                "Set your password â€“ Infrastructure Reporting System",
                It.Is<string>(body => body.Contains("http://localhost:3000/reset-password?userId=") && body.Contains("reset-token-123"))),
            Times.Once);

        _auditLogRepositoryMock.Verify(
            r => r.AddAsync(It.Is<AuditLog>(log =>
                log.EntityId == worker.Id &&
                log.ActionType == AuditActionType.AccountCreated &&
                log.EntityName == "User" &&
                log.UserId == "admin-1" &&
                log.Details == $"Admin created Worker account for {worker.Name} ({worker.Email})."
            )),
            Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_WhenAuthorityRoleAndValidInputs_CreatesAuthoritySendsEmailCreatesAuditLogAndReturnsSuccess()
    {
        var request = CreateRequest("Ahmad Aly", "test@example.com", "01112345678", "Authority", null);
        User? createdUser = null;
        _userManagerMock
            .Setup(m => m.FindByEmailAsync(request.Email!))
            .ReturnsAsync((User?)null);
        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .Callback<User, string>((u, _) => createdUser = u)
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(m => m.AddToRoleAsync(It.IsAny<User>(), "Authority"))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(m => m.GeneratePasswordResetTokenAsync(It.IsAny<User>()))
            .ReturnsAsync("reset-token-123");

        var service = CreateService();

        var result = await service.CreateUserAsync(request, "admin-1");

        result.UserId.Should().NotBeNullOrWhiteSpace();
        result.Message.Should().Be("Account created. The user will receive an email to set their password.");

        createdUser.Should().NotBeNull();
        createdUser.Should().BeOfType<InfraReportingSystem.Domain.Entities.Authority>();
        var authority = (InfraReportingSystem.Domain.Entities.Authority)createdUser!;
        authority.Name.Should().Be("Ahmad Aly");
        authority.UserName.Should().Be("test@example.com");
        authority.Email.Should().Be("test@example.com");
        authority.PhoneNumber.Should().Be("01112345678");
        authority.Status.Should().Be(UserStatus.Inactive);

        _userManagerMock.Verify(m => m.CreateAsync(authority, It.IsAny<string>()), Times.Once);
        _userManagerMock.Verify(m => m.AddToRoleAsync(authority, "Authority"), Times.Once);
        _userManagerMock.Verify(m => m.GeneratePasswordResetTokenAsync(authority), Times.Once);

        _emailServiceMock.Verify(
            s => s.SendEmailAsync(
                "test@example.com",
                "Set your password â€“ Infrastructure Reporting System",
                It.Is<string>(body => body.Contains("http://localhost:3000/reset-password?userId=") && body.Contains("reset-token-123"))),
            Times.Once);

        _auditLogRepositoryMock.Verify(
            r => r.AddAsync(It.Is<AuditLog>(log =>
                log.EntityId == authority.Id &&
                log.ActionType == AuditActionType.AccountCreated &&
                log.EntityName == "User" &&
                log.UserId == "admin-1" &&
                log.Details == $"Admin created Authority account for {authority.Name} ({authority.Email})."
            )),
            Times.Once);
    }

    private AdminUserCreationService CreateService()
    {
        return new AdminUserCreationService(
            _userManagerMock.Object,
            _configuration,
            _emailServiceMock.Object,
            _auditLogRepositoryMock.Object);
    }

    private static CreateUserRequestDto CreateRequest(
        string? fullName,
        string? email,
        string? phoneNumber,
        string? role,
        string? specialization)
    {
        return new CreateUserRequestDto
        {
            FullName = fullName!,
            Email = email!,
            PhoneNumber = phoneNumber!,
            Role = role!,
            Specialization = specialization
        };
    }

    private static IConfiguration CreateConfiguration()
    {
        var configurationValues = new Dictionary<string, string?>
        {
            ["FrontendUrl"] = "http://localhost:3000"
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
