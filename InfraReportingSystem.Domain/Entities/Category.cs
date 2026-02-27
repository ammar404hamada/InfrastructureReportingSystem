using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Domain.Entities { 
    public class Category
    {

        public int Id { get; set; }
        [Required(ErrorMessage ="Category name is required!")]
        [MaxLength(100, ErrorMessage ="Name cannot exceed 100 characters!")]
        public string Name { get; set; }

        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}
