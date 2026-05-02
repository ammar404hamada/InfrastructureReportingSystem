using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.ServiceAbstractions.Authority;
using InfraReportingSystem.ServiceAbstractions.Repositories.Authority;
using InfraReportingSystem.ServiceAbstractions.Repositories.Authority.Workers;
using InfraReportingSystem.Shared.DTOs.Authority.Workers;
using InfraReportingSystem.Shared.DTOs.Common;
using Microsoft.Extensions.Logging;

namespace InfraReportingSystem.Services.Authority
{
    public class AuthorityWorkersService : IAuthorityWorkersService
    {
        private readonly IAuthorityWorkersRepository _repository;
        private readonly ILogger<AuthorityWorkersService> _logger;

        public AuthorityWorkersService(
            IAuthorityWorkersRepository repository,
            ILogger<AuthorityWorkersService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<PaginatedResult<WorkerListDto>> GetWorkersAsync(
            string? search,
            int pageNumber,
            int pageSize)
        {
            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var (workers, totalCount) = await _repository.GetWorkersAsync(search, pageNumber, pageSize);

            if (totalCount == 0)
            {
                _logger.LogInformation("No active workers found. Search: '{Search}'", search ?? "none");

                return new PaginatedResult<WorkerListDto>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = 0,
                    Items = new List<WorkerListDto>(),
                    Message = "No workers found. Try adjusting your search."
                };
            }

            var items = workers
                .Select(w => new WorkerListDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    Email = w.Email ?? string.Empty,
                    PhoneNumber = w.PhoneNumber ?? string.Empty,
                    Specialization = (w as InfraReportingSystem.Domain.Entities.Worker)?.Specialization
                })
                .ToList();

            return new PaginatedResult<WorkerListDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = items,
                Message = string.Empty
            };
        }
    }
}