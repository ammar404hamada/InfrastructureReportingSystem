using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Shared.DTOs.PublicUser.MarkerPopup
{
    public class MarkerPopupDetailsDto
    {
        public int ReportId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Address { get; set; }           // future geocoding
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public IEnumerable<MarkerPopupPhotoDto> Photos { get; set; } = new List<MarkerPopupPhotoDto>();
        public MarkerPopupUserDto ReportedBy { get; set; } = new();
        public MarkerPopupUserDto? AssignedWorker { get; set; }
    }
}
