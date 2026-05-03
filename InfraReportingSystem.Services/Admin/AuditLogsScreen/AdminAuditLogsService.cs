using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Admin.AuditLogsScreen;
using InfraReportingSystem.ServiceAbstractions.Repositories.Admin.AuditLogsScreen;
using InfraReportingSystem.Shared.DTOs.Admin.AuditLogsScreen;
using InfraReportingSystem.Shared.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Services.Admin.AuditLogsScreen
{
    public class AdminAuditLogsService : IAdminAuditLogsService
    {
        private readonly IAdminAuditLogsRepository _repository;

        public AdminAuditLogsService(IAdminAuditLogsRepository repository)
        {
            _repository = repository;
        }

        public async Task<PaginatedResult<AuditLogDto>> GetLogsAsync(AuditLogsFilterDto filter)
        {
            filter.PageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            filter.PageSize = filter.PageSize < 1 ? 10 : filter.PageSize;
            filter.PageSize = filter.PageSize > 50 ? 50 : filter.PageSize;

            filter.Search = string.IsNullOrWhiteSpace(filter.Search)
                ? null
                : filter.Search.Trim();

            filter.Role = string.IsNullOrWhiteSpace(filter.Role)
                ? null
                : filter.Role.Trim();

            if (filter.StartDate.HasValue &&
                filter.EndDate.HasValue &&
                filter.StartDate.Value.Date > filter.EndDate.Value.Date)
            {
                return new PaginatedResult<AuditLogDto>
                {
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalCount = 0,
                    Items = new List<AuditLogDto>(),
                    Message = "Invalid date range."
                };
            }

            var normalizedRole = NormalizeRole(filter.Role);

            if (filter.Role != null && normalizedRole == null)
            {
                return new PaginatedResult<AuditLogDto>
                {
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalCount = 0,
                    Items = new List<AuditLogDto>(),
                    Message = "Invalid role filter."
                };
            }

            string? roleForRepo = normalizedRole == string.Empty
                ? null
                : normalizedRole;

            AuditActionType? actionType = null;

            if (!string.IsNullOrWhiteSpace(filter.ActionType))
            {
                if (!Enum.TryParse<AuditActionType>(
                    filter.ActionType,
                    true,
                    out var parsedAction))
                {
                    return new PaginatedResult<AuditLogDto>
                    {
                        PageNumber = filter.PageNumber,
                        PageSize = filter.PageSize,
                        TotalCount = 0,
                        Items = new List<AuditLogDto>(),
                        Message = "Invalid action type filter."
                    };
                }

                actionType = parsedAction;
            }

            var (logs, totalCount, userRoles) = await _repository.GetAuditLogsAsync(
                filter.PageNumber,
                filter.PageSize,
                filter.Search,
                filter.StartDate,
                filter.EndDate,
                roleForRepo,
                actionType);

            if (totalCount == 0)
            {
                return new PaginatedResult<AuditLogDto>
                {
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalCount = 0,
                    Items = new List<AuditLogDto>(),
                    Message = "No audit logs found matching the criteria."
                };
            }

            var items = logs.Select(log => new AuditLogDto
            {
                Timestamp = log.Timestamp,
                ActorName = log.User?.Name ?? "System",
                ActorEmail = log.User?.Email ?? "System",
                ActorRole = log.UserId != null &&
                            userRoles.TryGetValue(log.UserId, out var role)
                    ? role
                    : "System",
                ActionType = log.ActionType.ToString(),
                Target = !string.IsNullOrWhiteSpace(log.EntityName) &&
                         !string.IsNullOrWhiteSpace(log.EntityId)
                    ? $"{log.EntityName} {log.EntityId}"
                    : "System"
            }).ToList();

            return new PaginatedResult<AuditLogDto>
            {
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalCount = totalCount,
                Items = items,
                Message = string.Empty
            };
        }

        private static string? NormalizeRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return null;
            }

            var normalized = role
                .Replace(" ", "")
                .Trim()
                .ToLowerInvariant();

            return normalized switch
            {
                "admin" => "admin",
                "authority" => "authority",
                "worker" => "worker",
                "publicuser" => "publicuser",
                "system" => "system",
                "allroles" => string.Empty,
                _ => null
            };
        }
    }

}
