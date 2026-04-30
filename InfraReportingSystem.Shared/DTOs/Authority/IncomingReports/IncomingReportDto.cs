using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Shared.DTOs.Authority.IncomingReports
{
    public class IncomingReportDto
    {
        public int ReportId { get; set; }

       
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        
        public DateTime UpdatedAt { get; set; }

        public string Status { get; set; } = string.Empty;

        public string ReporterName { get; set; } = string.Empty;
        public string ReporterEmail { get; set; } = string.Empty;

       
        public string ReporterImage { get; set; } = string.Empty;

        public int PhotosCount { get; set; }

       
        public List<string> PhotosPreview { get; set; } = new();
    }
}

