using InfraReportingSystem.Shared.DTOs.Worker.TasksScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Shared.DTOs.Worker.TasksHistoryScreen
{
    public class HistoryTaskDto : WorkerTaskDto
    {
        public string? RejectionReason { get; set; }
    }
}
