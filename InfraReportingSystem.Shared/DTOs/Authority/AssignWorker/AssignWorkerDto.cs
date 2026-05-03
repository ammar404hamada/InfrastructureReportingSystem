using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Shared.DTOs.Authority.AssignWorker
{
    public class AssignWorkerDto
    {
        [Required(ErrorMessage = "WorkerId is required.")]
        public string WorkerId { get; set; } = null!;
    }
}
