using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Shared.DTOs.UserServices.Worker.CurrentTaskScreen
{
    public class MarkBlockedDto
    {
        [Required(ErrorMessage = "Reason is required when blocking a task.")]
        [MinLength(5, ErrorMessage = "Reason must be at least 5 characters. Please provide a detailed reason.")]
        public string Reason { get; set; } = null!;
    }
}
