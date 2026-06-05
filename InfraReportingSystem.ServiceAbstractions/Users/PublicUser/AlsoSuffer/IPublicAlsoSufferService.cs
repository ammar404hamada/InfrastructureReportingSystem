using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InfraReportingSystem.Shared.DTOs.UserServices.PublicUser.AlsoSuffer;

namespace InfraReportingSystem.ServiceAbstractions.Users.PublicUser.AlsoSuffer
{
    public interface IPublicAlsoSufferService
    {
        Task<AlsoSufferResponseDto> ConfirmAlsoSufferAsync(int reportId, string userId);
    }
}
