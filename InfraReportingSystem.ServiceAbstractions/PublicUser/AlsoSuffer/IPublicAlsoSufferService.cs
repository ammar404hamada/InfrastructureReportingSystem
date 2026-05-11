using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InfraReportingSystem.Shared.DTOs.PublicUser.AlsoSuffer;

namespace InfraReportingSystem.ServiceAbstractions.PublicUser.AlsoSuffer
{
    public interface IPublicAlsoSufferService
    {
        Task<AlsoSufferResponseDto> ConfirmAlsoSufferAsync(int reportId, string userId);
    }
}
