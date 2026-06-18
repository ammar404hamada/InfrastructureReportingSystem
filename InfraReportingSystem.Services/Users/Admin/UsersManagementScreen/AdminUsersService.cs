using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Shared;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Admin.UsersManagementScreen;
using InfraReportingSystem.ServiceAbstractions.Users.Admin.UsersManagementScreen;
using InfraReportingSystem.Shared.DTOs.Common;
using InfraReportingSystem.Shared.DTOs.UserServices.Admin.UsersManagementScreen;

namespace InfraReportingSystem.Services.Users.Admin.UsersManagementScreen
{
    public class AdminUsersService : IAdminUsersService
    {
        private readonly IAdminUsersRepository _repository;
        private readonly IAuditLogRepository _auditLogRepository;

        private static readonly HashSet<string> AllowedSortFields =
            new(StringComparer.OrdinalIgnoreCase) { "name", "email", "joindate" };

        public AdminUsersService(IAdminUsersRepository repository, IAuditLogRepository auditLogRepository)
        {
            _repository = repository;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<PaginatedResult<AdminUserDto>> GetUsersAsync(
            string? searchTerm,
            string? role,
            string? status,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize)
        {
            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);

            if (string.IsNullOrWhiteSpace(sortBy) || !AllowedSortFields.Contains(sortBy))
            {
                sortBy = "joindate";
            }

            if (!string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase))
            {
                sortDirection = "desc";
            }

            string? normalizedRole = role?.Trim().Replace(" ", "") switch
            {
                "Worker" => "Worker",
                "Authority" => "Authority",
                "PublicUser" => "PublicUser",
                _ => null
            };

            UserStatus? statusFilter = null;
            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<UserStatus>(status, true, out var parsedStatus))
            {
                statusFilter = parsedStatus;
            }

            var (users, totalCount) = await _repository.GetUsersAsync(
                searchTerm,
                normalizedRole,
                statusFilter,
                sortBy,
                sortDirection,
                pageNumber,
                pageSize);

            var items = users.Select(user => new AdminUserDto
            {
                Id = user.Id,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Role = GetRoleName(user),
                Status = user.Status.ToString(),
                JoinDate = user.CreatedAt
            }).ToList();

            return new PaginatedResult<AdminUserDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = items,
                Message = totalCount == 0
                    ? "No users found. Try adjusting your search or filters."
                    : string.Empty
            };
        }

        public async Task<AdminUserProfileResponseDto> GetUserProfileByIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return NotFoundProfileResponse();
            }

            var (user, role) = await _repository.GetUserProfileByIdAsync(userId);

            if (user == null)
            {
                return NotFoundProfileResponse();
            }

            return new AdminUserProfileResponseDto
            {
                Success = true,
                Message = "Profile retrieved successfully.",
                Profile = MapToProfileDto(user, role)
            };
        }

        public async Task<AdminUserProfileResponseDto> ChangeUserStatusAsync(AdminChangeUserStatusRequestDto request, string adminUserId)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                return new AdminUserProfileResponseDto
                {
                    Success = false,
                    Message = "User ID is required."
                };
            }

            if (string.IsNullOrWhiteSpace(request.Status))
            {
                return new AdminUserProfileResponseDto
                {
                    Success = false,
                    Message = "Status is required."
                };
            }

            if (!Enum.TryParse<UserStatus>(request.Status, true, out var newStatus))
            {
                return new AdminUserProfileResponseDto
                {
                    Success = false,
                    Message = $"Invalid status value '{request.Status}'. Valid values are Active, Inactive, Suspended, and Locked."
                };
            }

            if (newStatus == UserStatus.Deleted)
            {
                return new AdminUserProfileResponseDto
                {
                    Success = false,
                    Message = "Status cannot be set to Deleted."
                };
            }

            if (string.Equals(request.UserId, adminUserId, StringComparison.OrdinalIgnoreCase))
            {
                return new AdminUserProfileResponseDto
                {
                    Success = false,
                    Message = "You cannot change your own status."
                };
            }

            var (user, role) = await _repository.GetUserProfileByIdAsync(request.UserId);

            if (user == null)
            {
                return NotFoundProfileResponse();
            }

            var previousStatus = user.Status;
            user.Status = newStatus;

            await _repository.UpdateAsync(user);

            var auditLog = new AuditLog
            {
                UserId = adminUserId,
                ActionType = ToAuditActionType(newStatus),
                EntityName = "User",
                EntityId = user.Id,
                Details = user.Email != null
                    ? $"Admin changed user status for {user.Name} ({user.Email}) from {previousStatus} to {newStatus}."
                    : $"Admin changed user status for {user.Name} from {previousStatus} to {newStatus}.",
                Timestamp = DateTime.UtcNow
            };
            await _auditLogRepository.AddAsync(auditLog);

            return new AdminUserProfileResponseDto
            {
                Success = true,
                Message = "User status updated successfully.",
                Profile = MapToProfileDto(user, role)
            };
        }

        private static AuditActionType ToAuditActionType(UserStatus status) => status switch
        {
            UserStatus.Active => AuditActionType.AccountActivated,
            UserStatus.Inactive => AuditActionType.AccountDeactivated,
            UserStatus.Suspended => AuditActionType.AccountSuspended,
            UserStatus.Locked => AuditActionType.AccountLocked,
            _ => AuditActionType.AccountUpdated
        };

        private static AdminUserProfileDto MapToProfileDto(User user, string? role)
        {
            return new AdminUserProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = string.IsNullOrWhiteSpace(role) ? GetRoleName(user) : role,
                Status = user.Status.ToString(),
                ProfilePictureUrl = user.ProfilePictureUrl,
                JoinDate = user.CreatedAt,
                Specialization = (user as InfraReportingSystem.Domain.Entities.Worker)?.Specialization
            };
        }

        private static AdminUserProfileResponseDto NotFoundProfileResponse() => new()
        {
            Success = false,
            Message = "User not found."
        };

        private static string GetRoleName(User user) => user switch
        {
            InfraReportingSystem.Domain.Entities.Worker => "Worker",
            InfraReportingSystem.Domain.Entities.Authority => "Authority",
            _ => "Public User"
        };
    }
}
