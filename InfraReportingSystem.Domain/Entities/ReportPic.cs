using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Domain.Entities { 

    public class ReportPic
    {
        public int PicId { get; set; }
        [Required(ErrorMessage ="Picture URL is required!")]
        public string PicUrl { get; set; }
        public int ReportId { get; set; }
        public Report Report { get; set; } = null!;
    }
}