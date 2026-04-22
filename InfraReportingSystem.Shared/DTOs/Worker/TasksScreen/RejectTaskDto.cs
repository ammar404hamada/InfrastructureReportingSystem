using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Shared.DTOs.Worker.TasksScreen
{
    public class RejectTaskDto
    {
        public string Reason { get; set; } = null!;
    }
}
