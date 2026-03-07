using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace InfraReportingSystem.Domain.Entities
{
    
    public class Worker : User
    {
        public string? Specialization { get; set; }

        public ICollection<Report> AssignedReports { get; set; } = new List<Report>();
    }
}