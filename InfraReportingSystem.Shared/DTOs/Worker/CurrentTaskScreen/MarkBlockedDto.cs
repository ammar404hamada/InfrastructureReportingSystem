using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Shared.DTOs.Worker.CurrentTaskScreen
{
    public class MarkBlockedDto
    {
        [Required]
        [MinLength(3)]
        public string Reason { get; set; } = null!;
    }
}
