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
        public string Name { get; set; } = null!;
        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}
