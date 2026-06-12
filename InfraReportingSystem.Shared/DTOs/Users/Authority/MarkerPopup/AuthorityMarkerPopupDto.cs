using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Shared.DTOs.UserServices.Authority.MarkerPopup
{
    public class AuthorityMarkerPopupDto
    {
        public int ReportId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<string> Photos { get; set; } = new();
        public string Description { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string MapUrl { get; set; } = string.Empty;
        public string ReporterName { get; set; } = string.Empty;
    }
}
