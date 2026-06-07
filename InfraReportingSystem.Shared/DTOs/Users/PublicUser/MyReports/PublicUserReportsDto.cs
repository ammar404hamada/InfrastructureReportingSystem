using System;
using System.Collections.Generic;

namespace InfraReportingSystem.Shared.DTOs.UserServices.PublicUser.MyReports
{
    public class PublicUserReportsDto
    {
        public int ReportId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public double Latitude { get; set; }
        
        public double Longitude { get; set; }

        public string? AssignedByName { get; set; }

        public string? AssignedToName { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? BlockageReason { get; set; }

        public List<string> PhotoUrls { get; set; } = new();
    }
}
