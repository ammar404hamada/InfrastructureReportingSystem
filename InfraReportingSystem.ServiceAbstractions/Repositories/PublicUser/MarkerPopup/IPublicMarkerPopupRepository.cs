using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;

namespace InfraReportingSystem.ServiceAbstractions.Repositories.PublicUser.MarkerPopup
{
    public interface IPublicMarkerPopupRepository
    {
        Task<Report?> GetReportAsync(int reportId);
    }
}
