using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Shared.DTOs.PublicUser.MarkerPopup;

namespace InfraReportingSystem.ServiceAbstractions.PublicUser.MarkerPopup
{
    public interface IPublicMarkerPopupService
    {
        Task<MarkerPopupDetailsDto?> GetReportDetailsAsync(int reportId);
    }
}
