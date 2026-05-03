using InfraReportingSystem.Shared.DTOs.Admin.UserCreationScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.ServiceAbstractions.Admin.UserCreationScreen
{
    public interface IAdminUserCreationService
    {
        Task<CreateUserResponseDto> CreateUserAsync(CreateUserRequestDto request, string adminUserId);
    }
}
