using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraReportingSystem.Shared.DTOs.Authority.MarkerPopup;

namespace InfraReportingSystem.ServiceAbstractions.Authority.MarkerPopup
{
    public interface IAuthorityMarkerPopupService
    {
        Task<AuthorityMarkerPopupDto?> GetReportPopupAsync(int reportId);
    }
}
