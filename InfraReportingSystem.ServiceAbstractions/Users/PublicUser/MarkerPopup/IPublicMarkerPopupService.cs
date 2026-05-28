using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Shared.DTOs.UserServices.PublicUser.MarkerPopup;

namespace InfraReportingSystem.ServiceAbstractions.Users.PublicUser.MarkerPopup
{
    public interface IPublicMarkerPopupService
    {
        Task<MarkerPopupDetailsDto?> GetReportDetailsAsync(int reportId);
    }
}
