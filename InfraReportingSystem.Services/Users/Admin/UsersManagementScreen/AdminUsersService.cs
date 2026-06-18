using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Admin.UsersManagementScreen;
using InfraReportingSystem.ServiceAbstractions.Users.Admin.UsersManagementScreen;
using InfraReportingSystem.Shared.DTOs.Common;
using InfraReportingSystem.Shared.DTOs.UserServices.Admin.UsersManagementScreen;

namespace InfraReportingSystem.Services.Users.Admin.UsersManagementScreen
{
    public class AdminUsersService : IAdminUsersService
    {
        private readonly IAdminUsersRepository _repository;

        private static readonly HashSet<string> AllowedSortFields =
            new(StringComparer.OrdinalIgnoreCase) { "name", "email", "joindate" };

        public AdminUsersService(IAdminUsersRepository repository)
        {
            _repository = repository;
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
                Profile = new AdminUserProfileDto
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
                }
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
