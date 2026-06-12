using System.Linq;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Profile;
using InfraReportingSystem.ServiceAbstractions.Repositories.Shared;
using InfraReportingSystem.ServiceAbstractions.Repositories.Shared.Profile;
using InfraReportingSystem.Shared.DTOs.Shared.Profile.ProfileManagement;
using InfraReportingSystem.Shared.DTOs.Shared.Profile.ViewProfile;
using Microsoft.AspNetCore.Identity;

namespace InfraReportingSystem.Services.Profile
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _profileRepository;
        private readonly UserManager<User> _userManager;
        private readonly IAuditLogRepository _auditLogRepository;

        public ProfileService(
            IProfileRepository profileRepository,
            UserManager<User> userManager,
            IAuditLogRepository auditLogRepository)
        {
            _profileRepository = profileRepository;
            _userManager = userManager;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<UserProfileResponseDto> GetProfileAsync(string userId)
        {
            var (user, roles) = await _profileRepository.GetUserProfileAsync(userId);
            
            if (user == null)
            {
                return new UserProfileResponseDto
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            var specialization = ((user as Domain.Entities.Worker))?.Specialization;

            var profileDto = new UserProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Status = user.Status.ToString(),
                CreatedAt = user.CreatedAt,
                Roles = roles.ToList(),
                Specialization = specialization
            };

            return new UserProfileResponseDto
            {
                Success = true,
                Message = "Profile retrieved successfully.",
                Profile = profileDto
            };
        }

        public async Task<ChangePasswordResponseDto> ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new ChangePasswordResponseDto
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, dto.CurrentPassword);
            if (!isCurrentPasswordValid)
            {
                return new ChangePasswordResponseDto
                {
                    Success = false,
                    Message = "Current password is incorrect."
                };
            }

            if (dto.NewPassword == dto.CurrentPassword)
            {
                return new ChangePasswordResponseDto
                {
                    Success = false,
                    Message = "New password must be different from the current password."
                };
            }

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                return new ChangePasswordResponseDto
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            await _auditLogRepository.AddAsync(new AuditLog
            {
                UserId = user.Id,
                ActionType = AuditActionType.PasswordChanged,
                EntityName = "User",
                EntityId = user.Id,
                Details = "User changed their password."
            });

            return new ChangePasswordResponseDto
            {
                Success = true,
                Message = "Password changed successfully."
            };
        }
    }
}
