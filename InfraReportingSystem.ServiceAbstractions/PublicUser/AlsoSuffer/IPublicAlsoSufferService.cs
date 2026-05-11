using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.ServiceAbstractions.PublicUser.AlsoSuffer
{
    public interface IPublicAlsoSufferService
    {
        Task<AlsoSufferResponseDto> ConfirmAlsoSufferAsync(int reportId, string userId);
    }
}
