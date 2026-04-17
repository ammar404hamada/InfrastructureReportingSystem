using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Shared.DTOs.Worker
{
    public class WorkerTaskDto
    {
        public int Id { get; set; }

        public string Category { get; set; } = null!;
        public string Description { get; set; } = null!;
        public List<string> Photos { get; set; } = new();

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string SubmittedByName { get; set; } = null!;
        public DateTime SubmittedAt { get; set; }

        public string AssignedByName { get; set; } = null!;
        public DateTime? AssignedAt { get; set; }
    }
}
