using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Shared.DTOs.Users.PublicUser.SubmitReport
{
    public class SubmitReportRequestDto
    {
        public IFormFile Image { get; set; } = null!;
        public int CategoryId { get; set; }
        public string Description { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
