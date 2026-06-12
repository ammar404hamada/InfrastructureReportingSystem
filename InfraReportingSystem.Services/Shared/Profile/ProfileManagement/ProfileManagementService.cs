using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Shared;
using InfraReportingSystem.ServiceAbstractions.Repositories.Shared.Profile.ProfileManagement;
using InfraReportingSystem.ServiceAbstractions.Shared.Images;
using InfraReportingSystem.ServiceAbstractions.Shared.Profile.ProfileManagement;
using InfraReportingSystem.Shared.DTOs.Shared.Profile.ProfileManagement;
using System.IO;
using System.Threading.Tasks;

namespace InfraReportingSystem.Services.Shared.Profile.ProfileManagement
{
    public class ProfileManagementService : IProfileManagementService
    {
        private readonly IProfileManagementRepository _profileManagementRepository;
        private readonly IImageService _imageService;
        private readonly IAuditLogRepository _auditLogRepository;

        public ProfileManagementService(
            IProfileManagementRepository profileManagementRepository,
            IImageService imageService,
            IAuditLogRepository auditLogRepository)
        {
            _profileManagementRepository = profileManagementRepository;
            _imageService = imageService;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<UpdateProfileResponseDto> UpdateProfileInfoAsync(string userId, UpdateProfileInfoDto dto)
        {
            var user = await _profileManagementRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return new UpdateProfileResponseDto
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            bool isUpdated = false;

            if (!string.IsNullOrWhiteSpace(dto.Name) && user.Name != dto.Name)
            {
                user.Name = dto.Name;
                isUpdated = true;
            }

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber) && user.PhoneNumber != dto.PhoneNumber)
            {
                user.PhoneNumber = dto.PhoneNumber;
                isUpdated = true;
            }

            if (isUpdated)
            {
                var succeeded = await _profileManagementRepository.UpdateUserAsync(user);
                if (!succeeded)
                {
                    return new UpdateProfileResponseDto
                    {
                        Success = false,
                        Message = "Failed to update profile info."
                    };
                }

                await _auditLogRepository.AddAsync(new AuditLog
                {
                    UserId = user.Id,
                    ActionType = AuditActionType.AccountUpdated,
                    EntityName = "User",
                    EntityId = user.Id,
                    Details = "User updated their profile information."
                });
            }

            return new UpdateProfileResponseDto
            {
                Success = true,
                Message = "Profile updated successfully.",
                Data = new ProfileDataDto
                {
                    Name = user.Name,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    PhotoUrl = user.ProfilePictureUrl,
                    Status = user.Status.ToString()
                }
            };
        }

        public async Task<UpdateProfileResponseDto> UpdateProfilePhotoAsync(string userId, Stream fileStream, string fileName)
        {
            var user = await _profileManagementRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return new UpdateProfileResponseDto
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            var folderPath = $"profiles/{userId}";
            var photoUrl = await _imageService.UploadImageAsync(fileStream, fileName, folderPath);

            if (string.IsNullOrEmpty(photoUrl))
            {
                return new UpdateProfileResponseDto
                {
                    Success = false,
                    Message = "Failed to upload the profile picture."
                };
            }

            user.ProfilePictureUrl = photoUrl;
            var succeeded = await _profileManagementRepository.UpdateUserAsync(user);

            if (!succeeded)
            {
                return new UpdateProfileResponseDto
                {
                    Success = false,
                    Message = "Failed to update user's profile picture in the database."
                };
            }

            await _auditLogRepository.AddAsync(new AuditLog
            {
                UserId = user.Id,
                ActionType = AuditActionType.AccountUpdated,
                EntityName = "User",
                EntityId = user.Id,
                Details = "User updated their profile picture."
            });

            return new UpdateProfileResponseDto
            {
                Success = true,
                Message = "Profile picture updated successfully.",
                Data = new ProfileDataDto
                {
                    Name = user.Name,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    PhotoUrl = user.ProfilePictureUrl,
                    Status = user.Status.ToString()
                }
            };
        }
    }
}
