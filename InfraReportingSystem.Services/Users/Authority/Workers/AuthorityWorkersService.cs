using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Authority.Workers;
using InfraReportingSystem.ServiceAbstractions.Users.Authority.Workers;
using InfraReportingSystem.Shared.DTOs.Common;
using InfraReportingSystem.Shared.DTOs.UserServices.Authority.Workers;
using Microsoft.Extensions.Logging;

namespace InfraReportingSystem.Services.Users.Authority.Workers
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
                .Select(tuple => new WorkerListDto
                {
                    Id = tuple.Worker.Id,
                    Name = tuple.Worker.Name,
                    Email = tuple.Worker.Email ?? string.Empty,
                    PhoneNumber = tuple.Worker.PhoneNumber ?? string.Empty,
                    Specialization = ((tuple.Worker as Domain.Entities.Worker))?.Specialization,
                    ActiveTaskCount = tuple.ActiveTaskCount
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