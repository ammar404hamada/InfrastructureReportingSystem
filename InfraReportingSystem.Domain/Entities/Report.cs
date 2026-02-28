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
        [Required(ErrorMessage ="Description is required!")]
        [MaxLength(1000, ErrorMessage ="Description cannot exceed 1000 characters!")]
        public string Description { get; set; }
        public ReportStatus Status { get; set; } = ReportStatus.Pending;
        [Range(-90, 90, ErrorMessage ="Latitude must be between -90 and 90")]
        public double Latitude { get; set; }
        [Range(-180, 180, ErrorMessage ="Longitude must be between -180 and 180")]
        public double Longitude { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public ICollection<ReportPic> ReportPics { get; set; } = new List<ReportPic>();
    }

}