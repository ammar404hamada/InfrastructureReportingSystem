using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Domain.Entities
{
    public class Role
    {

        public int Id { get; set; }
        public string Name { get; set; } = null!;

        // One Role -> Many Users
        public ICollection<User> Users { get; set; } = new List<User>();

    }
}
