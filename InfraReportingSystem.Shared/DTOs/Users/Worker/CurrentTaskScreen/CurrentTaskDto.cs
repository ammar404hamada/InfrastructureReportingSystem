using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Shared.DTOs.UserServices.Worker.CurrentTaskScreen
{
    public class CurrentTaskDto
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Photos { get; set; } = new();
        public string Status { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string SubmittedByName { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public string AssignedByName { get; set; } = string.Empty;
        public DateTime? AssignedAt { get; set; }
        public string MapUrl { get; set; } = string.Empty;
    }

}
