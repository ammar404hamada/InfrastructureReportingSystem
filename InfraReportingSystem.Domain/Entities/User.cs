using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace InfraReportingSystem.Domain.Entities
{
    public class User : IdentityUser
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string Pic_url { get; set; }


        public int? RoleId { get; set; }
        public Role Role { get; set; }
        public ICollection<Report> SubmittedReports { get; set; }
    }
}