using InfraReportingSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Domain.Entities { 

    public class Report
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public ReportStatus Status { get; set; } = ReportStatus.Submitted;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public ICollection<ReportPic> ReportPics { get; set; } = new List<ReportPic>();
        public string? WorkerId { get; set; }
        public Worker? Worker { get; set; }

        public string? AuthorityId { get; set; }
        public Authority? Authority { get; set; }
    }

}