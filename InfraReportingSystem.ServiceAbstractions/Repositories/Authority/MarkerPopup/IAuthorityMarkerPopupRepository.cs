using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;

namespace InfraReportingSystem.ServiceAbstractions.Repositories.Authority.MarkerPopup
{
    public interface IAuthorityMarkerPopupRepository
    {
        Task<Report?> GetReportAsync(int reportId);
    }
}
